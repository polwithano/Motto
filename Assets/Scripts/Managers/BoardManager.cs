using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Animation;
using Events.Core;
using Events.Game;
using Events.Rounds;
using Events.Score;
using UnityEngine;
using Models;
using Views;

namespace Managers
{
    public class BoardManager : MonoBehaviourSingleton<BoardManager>
    {
        [field: SerializeField] public List<RectTransform> Slots { get; private set; } = new();
        [field: SerializeField] public List<SlotView> SlotViews { get; private set; } = new();

        [SerializeField] private Transform tileAnimationLayer; 
        
        private SlotView _previewedSlot;
        private Vector3 _selectedTilePosition;

        public SlotView GetPreviewedSlot() => _previewedSlot;

        #region Mono
        private void Start()
        {
            foreach (var slot in Slots)
                SlotViews.Add(slot.GetComponent<SlotView>());
        }

        private void Update()
        {
            if (TileController.Instance.SelectedTile != null && !TileController.Instance.IsOverRedraw)
            {
                _selectedTilePosition = TileController.Instance.SelectedTile.transform.position;
                DisplayPreviewedSlot();
            }
            else if (TileController.Instance.IsOverRedraw && _previewedSlot != null)
            {
                _previewedSlot.DisablePreviewFeedback();
                _previewedSlot = null;
            }
        }

        private void OnEnable()
        {
            Bus<TileMoveRequestEvent>.OnEvent += HandleTileMoveRequest;
            Bus<BoardUpdatedEvent>.OnEvent += HandleBoardUpdated;
            Bus<BoardClearedEvent>.OnEvent += HandleBoardCleared; 
            Bus<RoundStartedEvent>.OnEvent += HandleRoundStarted;
            Bus<WordProcessedEvent>.OnEvent += HandleWordProcessed;
            Bus<TileRedrawCompletedEvent>.OnEvent += HandleRedrawCompleted; 
        }

        private void OnDisable()
        {
            Bus<TileMoveRequestEvent>.OnEvent -= HandleTileMoveRequest;
            Bus<BoardUpdatedEvent>.OnEvent -= HandleBoardUpdated;
            Bus<BoardClearedEvent>.OnEvent -= HandleBoardCleared; 
            Bus<RoundStartedEvent>.OnEvent -= HandleRoundStarted;
            Bus<WordProcessedEvent>.OnEvent -= HandleWordProcessed; 
            Bus<TileRedrawCompletedEvent>.OnEvent -= HandleRedrawCompleted; 
        }

        private void OnDestroy() => OnDisable();
        #endregion

        #region Events
        private void HandleTileMoveRequest(TileMoveRequestEvent evt)
        {
            if (evt.Tile == null) return;

            if (evt.TargetPosition == GamePosition.Board)
            {
                var slotView = evt.TargetSlot != null ? evt.TargetSlot : GetFirstEmptySlot()?.GetComponent<SlotView>();
                if (slotView == null)
                {
                    Debug.LogError($"No Empty Slot found on the board, tile {evt.Tile.gameObject.name} cannot be moved.");
                    return;
                }

                AnimateTileToBoard(evt.Tile, slotView);
                return;
            }

            AnimateTileToHand(evt.Tile);
        }

        private void HandleBoardUpdated(BoardUpdatedEvent evt)
        {
            DisplayDefaultPreviewedSlot();
        }

        private void HandleRoundStarted(RoundStartedEvent evt)
        {
            DisplayDefaultPreviewedSlot();
        }
        
        private void HandleWordProcessed(WordProcessedEvent evt)
        {
            DisablePreviewedSlot();
        }
        
        private void HandleBoardCleared(BoardClearedEvent evt)
        {
            DisplayDefaultPreviewedSlot();
        }
        
        private void HandleRedrawCompleted(TileRedrawCompletedEvent evt)
        {
            DisplayDefaultPreviewedSlot();
        }
        #endregion
        
        private void AnimateTileToBoard(TileView tileView, SlotView slotView)
        {
            var target = slotView.GetComponent<RectTransform>();
            tileView.BeginFreeMove(tileAnimationLayer); 

            AnimationHelper.AnimateRectTransformToPosition(
                tileView.RectTransform,
                target.position,
                () =>
                {
                    AddTileToBoard(tileView, slotView);
                    Bus<BoardUpdatedEvent>.Raise(new BoardUpdatedEvent(GetCurrentSlotString(), GetTilesInSlots()));
                    Bus<TileMoveCompletedEvent>.Raise(new TileMoveCompletedEvent(tileView));
                }
            );
        }

        private void AnimateTileToHand(TileView tileView)
        {
            var targetPos = HandView.Instance.Container.position;

            AnimationHelper.AnimateRectTransformToPosition(
                tileView.RectTransform,
                targetPos,
                () =>
                {
                    AddTileToHand(tileView); 
                    Bus<BoardUpdatedEvent>.Raise(
                        new BoardUpdatedEvent(GetCurrentSlotString(), GetTilesInSlots())
                    );
                }
            );
        }

        private void AddTileToBoard(TileView tileView, SlotView slotView)
        {
            var slot = slotView.GetComponent<RectTransform>();

            tileView.transform.SetParent(slot.transform);
            tileView.transform.localPosition = Vector3.zero;
            tileView.transform.localScale = Vector3.one;

            tileView.SetInHand(false);
        }

        private void AddTileToHand(TileView tileView)
        {
            tileView.transform.SetParent(HandView.Instance.Container);
            tileView.transform.localPosition = Vector3.zero;
            tileView.transform.localScale = Vector3.one;

            tileView.SetInHand(true);
        }
        
        public RectTransform GetFirstEmptySlot()
        {
            return Slots.FirstOrDefault(slot => slot.childCount == 0);
        }

        private string GetCurrentSlotString()
        {
            return string.Concat(Slots.Select(slot =>
            {
                var tile = slot.GetComponentInChildren<TileView>();
                return tile != null ? tile.Tile.Character : string.Empty;
            }));
        }

        public async Task ClearSlotsAsync()
        {
            var tasks = new List<Task>();
            var index = 0;

            foreach (var slot in Slots)
            {
                if (slot.childCount == 0)
                    continue;

                if (slot.GetChild(0).TryGetComponent<TileView>(out var tileView))
                    tasks.Add(tileView.AnimateOnTileSlotClearedAsync(index++));
            }

            await Task.WhenAll(tasks);
            
            foreach (var slot in Slots)
            {
                foreach (Transform child in slot)
                {
                    DestroyImmediate(child.gameObject);
                }
            }

            Bus<BoardClearedEvent>.Raise(new BoardClearedEvent());
        }

        public TileView GetTileViewFromTile(Tile tile)
        {
            foreach (var slot in Slots)
            {
                var view = slot.GetComponentInChildren<TileView>();
                if (view == null) continue;
                if (view.Tile.ID == tile.ID) return view;
            }

            return null;
        }

        private List<Tile> GetTilesInSlots()
        {
            var tiles = new List<Tile>();

            foreach (var slot in Slots)
            {
                var tileView = slot.GetComponentInChildren<TileView>();
                if (tileView != null)
                    tiles.Add(tileView.Tile);
            }

            return tiles;
        }

        private void DisablePreviewedSlot()
        {
            _previewedSlot?.DisablePreviewFeedback();
            _previewedSlot = null;
        }
        
        private void DisplayDefaultPreviewedSlot()
        {
            _previewedSlot?.DisablePreviewFeedback();

            var firstEmpty = GetFirstEmptySlot();
            if (firstEmpty == null)
            {
                _previewedSlot = null;
                return;
            }

            _previewedSlot = firstEmpty.GetComponent<SlotView>();
            _previewedSlot.EnablePreviewFeedback();
        }

        private void DisplayPreviewedSlot()
        {
            SlotView closest = null;
            var minDistance = float.MaxValue;

            for (int i = 0; i < Slots.Count; i++)
            {
                if (!IsPlayableEmptySlot(i))
                    continue;

                var slot = Slots[i];
                var distance = Vector3.Distance(_selectedTilePosition, slot.position);

                if (distance < minDistance)
                {
                    minDistance = distance;
                    closest = slot.GetComponent<SlotView>();
                }
            }

            if (closest == null || _previewedSlot == closest)
                return;

            _previewedSlot?.DisablePreviewFeedback();
            _previewedSlot = closest;
            _previewedSlot.EnablePreviewFeedback();
        }

        private bool IsPlayableEmptySlot(int index)
        {
            if (Slots[index].childCount > 0)
                return false;

            var hasLeft = index > 0 && Slots[index - 1].childCount > 0;
            var hasRight = index < Slots.Count - 1 && Slots[index + 1].childCount > 0;

            if (!hasLeft && !hasRight)
                return index == 0;

            return hasLeft || hasRight;
        }
    }
}

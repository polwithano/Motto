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
        [field: SerializeField] public List<SlotView> SlotViews  { get; private set; } = new();
        [field: SerializeField] public SlotView PreviewedSlot    { get; private set; }
        
        // Dragged and/or lerped tiles are temporarily assigned to another canvas layer. 
        [SerializeField] private RectTransform draggedTileCanvas; 
        
        private Vector3 _selectedTilePosition;
        
        #region Mono
        private void Start()
        {
            foreach (var slot in Slots)
                SlotViews.Add(slot.GetComponent<SlotView>());
        }

        private void Update()
        {
            if (TileController.Instance.SelectedTile && !TileController.Instance.IsOverRedraw)
            {
                _selectedTilePosition = TileController.Instance.SelectedTile.transform.position;
                DisplayPreviewedSlot();
            }
            else if (TileController.Instance.IsOverRedraw && PreviewedSlot)
            {
                PreviewedSlot.DisablePreviewFeedback();
                PreviewedSlot = null;
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
        #endregion

        #region Event Handlers
        private void HandleTileMoveRequest(TileMoveRequestEvent evt)
        {
            if (!evt.Tile) return;

            if (evt.TargetPosition == GamePosition.Board)
            {
                var slotView = evt.TargetSlot ? evt.TargetSlot : GetFirstEmptySlot()?.GetComponent<SlotView>();
                if (!slotView)
                {
                    Debug.LogError("No Empty Slot found on the board." +
                                   $" Tile {evt.Tile.gameObject.name} cannot be moved.");
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
        
        #region Tile Animation
        private void AnimateTileToBoard(TileView tileView, SlotView slotView)
        {
            var target = slotView.GetComponent<RectTransform>();
            tileView.BeginFreeMove(draggedTileCanvas); 

            AnimationHelper.AnimateRectTransformToPosition(
                tileView.RectTransform,
                target.position,
                () =>
                {
                    BoardManagerExtensions.AddTileToBoard(tileView, slotView);
                    Bus<BoardUpdatedEvent>.Raise(new BoardUpdatedEvent(ConcatenatedTileDataCharacters(), GetTileModelsInSlots()));
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
                    BoardManagerExtensions.AddTileToHand(tileView); 
                    Bus<BoardUpdatedEvent>.Raise(
                        new BoardUpdatedEvent(ConcatenatedTileDataCharacters(), GetTileModelsInSlots())
                    );
                }
            );
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
                    // This is a potential issue in builds. 
                    // In Editor Destroy create other kind of issues. 
                    DestroyImmediate(child.gameObject);
                }
            }

            Bus<BoardClearedEvent>.Raise(new BoardClearedEvent());
        }
        #endregion
        
        #region Previewed Slots
        private void DisablePreviewedSlot()
        {
            PreviewedSlot?.DisablePreviewFeedback();
            PreviewedSlot = null;
        }
        
        private void DisplayDefaultPreviewedSlot()
        {
            PreviewedSlot?.DisablePreviewFeedback();

            var firstEmpty = GetFirstEmptySlot();
            if (firstEmpty == null)
            {
                PreviewedSlot = null;
                return;
            }

            PreviewedSlot = firstEmpty.GetComponent<SlotView>();
            PreviewedSlot.EnablePreviewFeedback();
        }

        private void DisplayPreviewedSlot()
        {
            SlotView closest = null;
            var minDistance = float.MaxValue;

            for (var i = 0; i < Slots.Count; i++)
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

            if (!closest || PreviewedSlot == closest)
                return;

            PreviewedSlot?.DisablePreviewFeedback();
            PreviewedSlot = closest;
            PreviewedSlot.EnablePreviewFeedback();
        }
        #endregion
        
        private RectTransform GetFirstEmptySlot()
        {
            return Slots.FirstOrDefault(slot => slot.childCount == 0);
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

        /// <summary>
        /// Return the word currently displayed in the board.
        /// Concat all the characters stored in each TileView.TileData 
        /// </summary>
        private string ConcatenatedTileDataCharacters()
        {
            return string.Concat(Slots.Select(slot =>
            {
                var tile = slot.GetComponentInChildren<TileView>();
                return tile ? tile.Tile.Character.ToString() : string.Empty;
            }));
        }
        
        public TileView GetTileViewFromModel(Tile tile)
        {
            foreach (var slot in Slots)
            {
                var view = slot.GetComponentInChildren<TileView>();
                if (!view) continue;
                if (view.Tile.ID == tile.ID) return view;
            }

            return null;
        }

        private List<Tile> GetTileModelsInSlots()
        {
            var tiles = new List<Tile>();

            foreach (var slot in Slots)
            {
                var tileView = slot.GetComponentInChildren<TileView>();
                if (tileView)
                    tiles.Add(tileView.Tile);
            }

            return tiles;
        }
    }

    public static class BoardManagerExtensions
    {
        public static void AddTileToBoard(TileView tileView, SlotView slotView)
        {
            var slot = slotView.transform;

            tileView.transform.SetParent(slot.transform);
            tileView.transform.localPosition = Vector3.zero;
            tileView.transform.localScale = Vector3.one;

            tileView.SetInHand(false);
        }

        public static void AddTileToHand(TileView tileView)
        {
            tileView.transform.SetParent(HandView.Instance.Container);
            tileView.transform.localPosition = Vector3.zero;
            tileView.transform.localScale = Vector3.one;

            tileView.SetInHand(true);
        }
    }
}

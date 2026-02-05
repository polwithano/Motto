using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Events;
using UnityEngine;
using Models;
using Views;

namespace Managers
{
    public class BoardManager : MonoBehaviourSingleton<BoardManager>
    {
        [field: SerializeField] public List<RectTransform> Slots { get; private set; } = new(); 
        [field: SerializeField] public List<SlotView>  SlotViews { get; private set; } = new();

        private SlotView _previewedSlot;
        private Vector3 _selectedTilePosition; 
        
        public SlotView GetPreviewedSlot() =>  _previewedSlot;
        
        #region Mono Methods

        private void Start()
        {
            foreach (var slot in Slots)
            {
                SlotViews.Add(slot.GetComponent<SlotView>());
            }
        }

        private void Update()
        {
            if (TileController.Instance.SelectedTile != null && !TileController.Instance.IsOverRedraw)
            {
                _selectedTilePosition = TileController.Instance.SelectedTile.transform.position;
                DisplayPreviewedSlot();
            }
            else if (TileController.Instance.SelectedTile == null && _previewedSlot != null)
            {
                _previewedSlot.DisablePreviewFeedback();
                _previewedSlot = null;
            }
            else if (TileController.Instance.IsOverRedraw && _previewedSlot != null)
            {
                _previewedSlot.DisablePreviewFeedback();
                _previewedSlot = null;
            }
        }
        
        private void OnEnable()
        {
            GameEvents.OnTileAddedToBoard += HandleOnTileAddedToBoard;
            GameEvents.OnTileDropConfirmed += HandleOnTileDropConfirmed; 
            GameEvents.OnTileRemovedFromBoard += HandleOnTileRemovedFromBoard;
        }

        private void OnDisable()
        {
            GameEvents.OnTileAddedToBoard -= HandleOnTileAddedToBoard;
            GameEvents.OnTileDropConfirmed -= HandleOnTileDropConfirmed;
            GameEvents.OnTileRemovedFromBoard -= HandleOnTileRemovedFromBoard;
        }
        
        private void OnDestroy() => OnDisable();
        #endregion
        
        #region Subscribed Methods
        private void HandleOnTileAddedToBoard(TileView tileView)
        {
            AddTileToBoard(tileView);
            GameEvents.RaiseOnBoardUpdated(GetCurrentSlotString(), GetTilesInSlots());
        }
        
        private void HandleOnTileDropConfirmed(TileView tileView, SlotView slotView)
        {
            AddTileToBoard(tileView, slotView);
            GameEvents.RaiseOnBoardUpdated(GetCurrentSlotString(), GetTilesInSlots());
        }

        private void HandleOnTileRemovedFromBoard(TileView tileView)
        {
            AddTileToHand(tileView);
            GameEvents.RaiseOnBoardUpdated(GetCurrentSlotString(), GetTilesInSlots());
        }
        #endregion

        private void AddTileToBoard(TileView tileView, SlotView slotView = null)
        {
            RectTransform slot; 
            
            if (slotView == null)
                slot = GetFirstEmptySlot();
            else
                slot = slotView.GetComponent<RectTransform>();
            
            tileView.transform.SetParent(slot.transform);
            tileView.transform.localPosition = Vector3.zero;
            tileView.transform.localScale = Vector3.one;
            
            tileView.SetInHand(false);
        }

        private void AddTileToHand(TileView tileView)
        {
            tileView.transform.SetParent(HandView.Instance.Container);
            tileView.transform.localPosition = Vector3.zero;
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
                var tile = slot.GetComponentInChildren<Views.TileView>();
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
                {
                    tasks.Add(tileView.AnimateOnTileSlotClearedAsync(index++));
                }
            }

            await Task.WhenAll(tasks);
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
                var tileView = slot.GetComponentInChildren<Views.TileView>();
                if (tileView != null)
                    tiles.Add(tileView.Tile);
            }

            return tiles;
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

                // Priority => Left -> Distance
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

            // First Slot
            if (!hasLeft && !hasRight)
                return index == 0;

            // Hole between two tiles
            return hasLeft || hasRight;
        }
    }
}
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Events;
using UnityEngine;
using Models;
using NUnit.Framework;
using Views;

namespace Managers
{
    public class BoardManager : MonoBehaviourSingleton<BoardManager>
    {
        [field: SerializeField] public List<RectTransform> Slots { get; private set; } = new(); 
        
        #region Mono Methods
        private void OnEnable()
        {
            GameEvents.OnTileAddedToBoard += HandleOnTileAddedToBoard;
            GameEvents.OnTileRemovedFromBoard += HandleOnTileRemovedFromBoard;
        }

        private void OnDisable()
        {
            GameEvents.OnTileAddedToBoard -= HandleOnTileAddedToBoard;
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

        private void HandleOnTileRemovedFromBoard(TileView tileView)
        {
            AddTileToHand(tileView);
            GameEvents.RaiseOnBoardUpdated(GetCurrentSlotString(), GetTilesInSlots());
        }
        #endregion

        private void AddTileToBoard(TileView tileView)
        {
            var slot = GetFirstEmptySlot(); 
            tileView.transform.SetParent(slot.transform);
            tileView.transform.localPosition = Vector3.zero;
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
    }
}
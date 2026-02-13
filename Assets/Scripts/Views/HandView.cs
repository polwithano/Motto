using System.Linq;
using Events;
using Events.Core;
using Events.Game;
using Events.Inputs;
using Models;
using UnityEngine;
using UnityEngine.Rendering;

namespace Views
{
    public class HandView : MonoBehaviourSingleton<HandView>
    {
        [SerializeField] private TileView viewPrefab;
        
        [field: SerializeField] public Transform Container { get; private set; }
        [field: SerializeField] public SerializedDictionary<string, TileView> ViewsById { get; private set; } = new();

        #region Mono
        private void OnEnable()
        {
            Bus<TileRedrawCompletedEvent>.OnEvent += HandleOnTileRedrawPerformed; 
            Bus<TileDraggedEvent>.OnEvent += HandleOnTileDragged;
        }

        private void OnDisable()
        {
            Bus<TileRedrawCompletedEvent>.OnEvent -= HandleOnTileRedrawPerformed; 
            Bus<TileDraggedEvent>.OnEvent -= HandleOnTileDragged;
        }

        private void OnDestroy() => OnDisable();
        #endregion
        
        #region Subscribed
        private void HandleOnTileRedrawPerformed(TileRedrawCompletedEvent evt)
        {
            var tile = evt.PreviousTile;
            var newTile = evt.NewTile;
            
            if (tile == null || newTile == null)
                return;
            
            int siblingIndex = -1;

            // Store the sibling index before destroying
            if (ViewsById.TryGetValue(tile.ID, out var oldView) && oldView != null)
            {
                siblingIndex = oldView.transform.GetSiblingIndex();
                Destroy(oldView.gameObject);
                ViewsById.Remove(tile.ID);
            }

            // Instantiate the new tile
            var newView = Instantiate(viewPrefab, Container);
            
            newView.Populate(newTile);
            newView.OnTileRedraw();

            // If we had a valid index, set it back in the hierarchy
            if (siblingIndex >= 0)
                newView.transform.SetSiblingIndex(siblingIndex);

            // Register in dictionary
            ViewsById[newTile.ID] = newView;
        }

        private void HandleOnTileDragged(TileDraggedEvent evt)
        {
            // Set the tiles views blocking raycast to false whenever a tile is dragged. 
            SetTileViewsRaycast(evt.EventType);
        }
        #endregion

        public void InstantiateHand(TileHand hand)
        {
            DestroyViews();

            foreach (var tile in hand.Tiles)
            {
                InstantiateTile(tile);
            }
        }
        
        public bool TryGetView(Tile tile, out TileView view)
        {
            if (tile == null)
            {
                view = null;
                return false;
            }

            return ViewsById.TryGetValue(tile.ID, out view);
        }

        public TileView GetView(Tile tile)
        {
            return tile != null && ViewsById.TryGetValue(tile.ID, out var view) ? view : null;
        }

        private void InstantiateTile(Tile tile)
        {
            var view = Instantiate(viewPrefab, Container);
            view.Populate(tile); 
            ViewsById[tile.ID] = view; 
        }

        private void DestroyView(Tile tile)
        {
            if (tile == null) return;

            if (ViewsById.TryGetValue(tile.ID, out var view))
            {
                if (view != null)
                    Destroy(view.gameObject);

                ViewsById.Remove(tile.ID);
            }
        }

        private void DestroyViews()
        {
            foreach (var kvp in ViewsById.Where(kvp => kvp.Value != null))
            {
                Destroy(kvp.Value.gameObject);
            }

            ViewsById.Clear();
        }

        private void SetTileViewsRaycast(DragEventType dragEventType)
        {
            var interactable = dragEventType != DragEventType.DragStart;

            foreach (var kvp in ViewsById.Where(kvp => kvp.Value != null))
            {
                kvp.Value.gameObject.TryGetComponent(out CanvasGroup canvasGroup);
                {
                    canvasGroup.interactable = interactable;
                    canvasGroup.blocksRaycasts = interactable;
                }
            }
        }
    }
}

using Events;
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
            GameEvents.OnTileRedrawPerformed += HandleOnTileRedrawPerformed;
        }

        private void OnDisable()
        {
            GameEvents.OnTileRedrawPerformed -= HandleOnTileRedrawPerformed;
        }

        private void OnDestroy() => OnDisable();
        #endregion
        
        #region Subscribed
        private void HandleOnTileRedrawPerformed(Tile tile, Tile newTile)
        {
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
        #endregion

        public void InstantiateHand(TileHand hand)
        {
            DestroyViews();

            foreach (var tile in hand.Tiles)
            {
                InstantiateTile(tile);
            }
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
            foreach (var kvp in ViewsById)
            {
                if (kvp.Value != null)
                    Destroy(kvp.Value.gameObject);
            }

            ViewsById.Clear();
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
    }
}

using System.Collections.Generic;
using Events;
using UnityEngine;
using UnityEngine.EventSystems;
using Views;

namespace Managers
{
    public class TileController : MonoBehaviourSingleton<TileController>
    {
        [field: SerializeField] public TileView SelectedTile { get;  private set; }
        
        #region Mono
        private void OnEnable()
        {
            InputManager.Instance.OnLeftClick += HandleLeftClick;
            InputManager.Instance.OnRightClick += HandleRightClick;
        }

        private void OnDisable()
        {
            InputManager.Instance.OnLeftClick -= HandleLeftClick;
            InputManager.Instance.OnRightClick -= HandleRightClick;
        }

        private void OnDestroy() => OnDisable(); 
        #endregion
        
        #region Subscribed
        private void HandleLeftClick(Vector2 screenPos)
        {
            SelectTile(screenPos);
        }

        private void HandleRightClick(Vector2 screenPos)
        {
            RedrawTile(screenPos);
        }
        #endregion

        private void SelectTile(Vector2 screenPos)
        {
            var tileView = RaycastTile(screenPos);
            if (tileView == null) return;
            
            GameEvents.RaiseOnTileSelected(tileView);
        }

        private void RedrawTile(Vector2 screenPos)
        {
            var tileView = RaycastTile(screenPos);
            if (tileView && tileView.IsInHand)
            {
                GameEvents.RaiseOnTileRedraw(tileView.Tile);
            }
        }
        
        private TileView RaycastTile(Vector2 screenPos)
        {
            var ped = new PointerEventData(EventSystem.current) { position = screenPos };
            var results = new List<RaycastResult>();
            EventSystem.current.RaycastAll(ped, results);

            foreach (var hit in results)
            {
                var tile = hit.gameObject.GetComponentInParent<TileView>();
                if (tile != null)
                    return tile;
            }

            return null;
        }
    }
}

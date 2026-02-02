using System.Collections.Generic;
using Events;
using UnityEngine;
using UnityEngine.EventSystems;
using Views;

namespace Managers
{
    public class TileController : MonoBehaviourSingleton<TileController>
    {
        [SerializeField] private Transform dragLayer;
        
        [field: SerializeField] public TileView SelectedTile { get;  private set; }
        
        private InputManager _input;
        private RectTransform _dragLayerRect;
        private TileView _draggedTile;
        private Vector2 _dragOffset;
        
        private Canvas _canvas;
        private Camera _uiCamera;
        
        #region Mono

        private void Start()
        {
            _input = InputManager.Instance;
            _dragLayerRect = dragLayer as RectTransform;
            
            _canvas = _dragLayerRect.GetComponentInParent<Canvas>(true);

            _uiCamera = _canvas.renderMode == RenderMode.ScreenSpaceOverlay
                ? null
                : (_canvas.worldCamera != null ? _canvas.worldCamera : Camera.main);
            
            Debug.Log($"Canvas={_canvas.name} mode={_canvas.renderMode} cam={_uiCamera}");
        }

        private void Update()
        {
            if (_draggedTile == null)
            {
                TryStartDrag();
            }
            else
            {
                UpdateDrag();
                TryEndDrag();
            }
        }
        
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

        private void TryStartDrag()
        {
            if (_input.PointerJustPressed || !_input.IsDragging)
                return;

            var tile = RaycastTile(_input.PointerPosition);
            if (tile == null || !tile.IsInHand)
                return;

            _draggedTile = tile;
            _draggedTile.BeginDrag(dragLayer);
            
            SelectedTile = _draggedTile;

            UpdateDrag();
        }

        private void UpdateDrag()
        {
            RectTransformUtility.ScreenPointToWorldPointInRectangle(
                _draggedTile.RectTransform,
                _input.PointerPosition,
                _uiCamera,
                out var worldPos
            );

            _draggedTile.RectTransform.position = worldPos;
        }

        private void TryEndDrag()
        {
            if (!_input.PointerJustReleased)
                return;

            _draggedTile.EndDrag();
            _draggedTile = null;
            
            SelectedTile = _draggedTile;
        }

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

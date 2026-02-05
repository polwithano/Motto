using System.Collections.Generic;
using DG.Tweening;
using Events;
using Events.Core;
using Events.Inputs;
using UnityEngine;
using UnityEngine.EventSystems;
using Views;

namespace Managers
{
    public class TileController : MonoBehaviourSingleton<TileController>
    {
        [SerializeField] private RedrawView redrawView;
        [SerializeField] private LayerMask drawLayer; 
        [SerializeField] private LayerMask handLayer; 
        [SerializeField] private Transform dragLayer;
        [SerializeField] private float snapToSlotDistance; 
        
        [field: SerializeField] public TileView SelectedTile { get;  private set; }
        [field: SerializeField] public bool IsOverRedraw { get; private set; }
        
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
        }

        private void Update()
        {
            if (_draggedTile == null)
            {
                if (TryStartDrag())
                {
                    Bus<TileDraggedEvent>.Raise(new TileDraggedEvent(_draggedTile, DragEventType.DragStart));
                }
            }
            else
            {
                UpdateDrag();
                if (TryEndDrag())
                {
                    Bus<TileDraggedEvent>.Raise(new TileDraggedEvent(_draggedTile, DragEventType.DragEnd));
                    _draggedTile = null;
                }
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

        private bool TryStartDrag()
        {
            if (_input.PointerJustPressed || !_input.IsDragging)
                return false;

            var tile = RaycastTile(_input.PointerPosition);
            if (tile == null || !tile.IsInHand)
                return false;

            _draggedTile = tile;
            _draggedTile.BeginDrag(dragLayer);
            
            SelectedTile = _draggedTile;

            UpdateDrag();
            
            return true;
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
            
            var isOverRedrawNow = IsPointerOverLayer(_input.PointerPosition, drawLayer);

            if (isOverRedrawNow != IsOverRedraw)
            {
                IsOverRedraw = isOverRedrawNow;
                redrawView.SetHovered(IsOverRedraw);
            }
        }

        private bool TryEndDrag()
        {
            if (!_input.PointerJustReleased)
                return false;

            var tile = _draggedTile;

            IsOverRedraw = false;
            SelectedTile = null;
            
            redrawView.SetHovered(IsOverRedraw);

            var pointerPos = _input.PointerPosition;

            if (IsPointerOverLayer(pointerPos, handLayer))
            {
                tile.EndDrag();
                return true;
            }
            
            if (IsPointerOverLayer(pointerPos, drawLayer))
            {
                AnimateTileToPosition(
                    tile, 
                    redrawView.GetComponent<RectTransform>().position,
                    () => GameEvents.RaiseOnTileRedraw(tile.Tile));
                return true;
            }
            
            var slot = BoardManager.Instance.GetPreviewedSlot();
            if (slot != null && IsCloseEnoughFromSlot(tile.RectTransform, slot.GetComponent<RectTransform>()))
            {
                AnimateTileToPosition(
                    tile, 
                    slot.GetComponent<RectTransform>().position, 
                    () => GameEvents.RaiseOnTileDropConfirmed(tile, slot));
                return true; 
            }
            
            tile.EndDrag();
            return true; 
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

        private bool IsCloseEnoughFromSlot(RectTransform tile, RectTransform slot)
        {
            return Vector2.Distance(tile.position, slot.position) <= snapToSlotDistance;
        }

        private bool IsPointerOverLayer(Vector2 screenPos, LayerMask mask)
        {
            var ped = new PointerEventData(EventSystem.current)
            {
                position = screenPos
            };

            var results = new List<RaycastResult>();
            EventSystem.current.RaycastAll(ped, results);

            foreach (var hit in results)
            {
                Debug.Log($"Hit: {hit.gameObject.name} | layer={LayerMask.LayerToName(hit.gameObject.layer)}");
                if ((mask.value & (1 << hit.gameObject.layer)) != 0)
                {
                   // Debug.Log("Pointer over layer: " + LayerMask.LayerToName(hit.gameObject.layer));
                    return true;
                }
            }

            return false;
        }
        
        private void AnimateTileToSlot(TileView tile, SlotView slot)
        {
            tile.RectTransform
                .DOMove(slot.GetComponent<RectTransform>().position, 0.225f)
                .SetEase(Ease.InExpo)
                .OnComplete(() =>
                {
                    GameEvents.RaiseOnTileDropConfirmed(tile, slot);
                });
        }

        private void AnimateTileToPosition(TileView tile, Vector3 position, System.Action onComplete = null)
        {
            tile.RectTransform
                .DOMove(position, 0.225f)
                .SetEase(Ease.InExpo)
                .OnComplete(() => onComplete?.Invoke());
        }
    }
}

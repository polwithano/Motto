using System.Collections.Generic;
using Animation;
using Events;
using Events.Core;
using Events.Game;
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
        private Canvas _canvas;
        private Camera _uiCamera;
        
        private RectTransform _dragLayerRect;
        private TileView _draggedTile;
        private Vector2 _dragOffset;
        
        #region Mono
        private void Start()
        {
            _input = InputManager.Instance;
            _dragLayerRect = dragLayer as RectTransform;

            if (_dragLayerRect != null) _canvas = _dragLayerRect.GetComponentInParent<Canvas>(true);

            _uiCamera = _canvas.renderMode == RenderMode.ScreenSpaceOverlay
                ? null : (_canvas.worldCamera != null ? _canvas.worldCamera : Camera.main);
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

            var pointerPos = _input.PointerPosition;
            var tile = _draggedTile;

            IsOverRedraw = false;
            SelectedTile = null;
            
            redrawView.SetHovered(IsOverRedraw);
            
            if (IsPointerOverLayer(pointerPos, handLayer))
            {
                tile.EndDrag();
                return true;
            }
            
            if (IsPointerOverLayer(pointerPos, drawLayer))
            {
                AnimationHelper.AnimateRectTransformToPosition(
                    tile.RectTransform, 
                    redrawView.GetComponent<RectTransform>().position,
                    () => Bus<TileRedrawEvent>.Raise(new TileRedrawEvent(tile, tile.Tile)));

                return true;
            }
            
            var slot = BoardManager.Instance.GetPreviewedSlot();
            var target = slot.GetComponent<RectTransform>();
            
            if (slot && AnimationHelper.AreRectTransformCloseEnough(tile.RectTransform, target, snapToSlotDistance))
            {
                AnimationHelper.AnimateRectTransformToPosition(
                    tile.RectTransform, 
                    target.position, 
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
            
            var newPosition = tileView.IsInHand ? GamePosition.Board : GamePosition.Hand;
            
            // Moving the tile from the hand to the board
            if (newPosition == GamePosition.Board)
            {
                var emptySlot = BoardManager.Instance.GetFirstEmptySlot();
                if (!emptySlot)
                {
                    Debug.LogError("No Empty Slot found on the board" +
                                   $"{tileView.gameObject.name} cannot be moved.");
                    return;
                }
                AnimationHelper.AnimateRectTransformToPosition(
                    tileView.RectTransform,
                    emptySlot.transform.position,
                    () => Bus<TilePositionUpdatedEvent>.Raise(
                        new TilePositionUpdatedEvent(newPosition, tileView))
                    );
            }
            // Moving the tile from the board to the hand
            if (newPosition == GamePosition.Hand)
            {
                Bus<TilePositionUpdatedEvent>.Raise(new TilePositionUpdatedEvent(newPosition, tileView));
            }
            
            // Bus<TileSelectedEvent>.Raise(new TileSelectedEvent(tileView, tileView.Tile));
        }

        private void RedrawTile(Vector2 screenPos)
        {
            var tileView = RaycastTile(screenPos);
            if (tileView && tileView.IsInHand)
            {
                Bus<TileRedrawEvent>.Raise(new TileRedrawEvent(tileView, tileView.Tile));
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
                // Debug.Log($"Hit: {hit.gameObject.name} | layer={LayerMask.LayerToName(hit.gameObject.layer)}");
                if ((mask.value & (1 << hit.gameObject.layer)) != 0)
                {
                   // Debug.Log("Pointer over layer: " + LayerMask.LayerToName(hit.gameObject.layer));
                    return true;
                }
            }

            return false;
        }
    }
}

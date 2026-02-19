using System.Collections.Generic;
using Animation;
using Events.Core;
using Events.Game;
using Events.Inputs;
using Events.Score;
using FSM;
using FSM.States;
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
        private bool _canInteract; 
        
        #region Mono
        private void Start()
        {
            _input = InputManager.Instance;
            
            _dragLayerRect = dragLayer as RectTransform;

            if (_dragLayerRect != null) _canvas = _dragLayerRect.GetComponentInParent<Canvas>(true);

            _uiCamera = _canvas.renderMode == RenderMode.ScreenSpaceOverlay
                ? null : (_canvas.worldCamera != null ? _canvas.worldCamera : Camera.main);

            _canInteract = true; 
        }

        private void Update()
        {
            if (!GameManager.Instance.IsRoundPlayState()) return;
            if (!_canInteract) return; 
            
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
            
            Bus<ScoringSequenceStartedEvent>.OnEvent += HandleScoringSequenceStarted;
            Bus<ScoringSequenceOverEvent>.OnEvent += HandleScoringSequenceOver; 
        }
        
        private void OnDisable()
        {
            InputManager.Instance.OnLeftClick -= HandleLeftClick;
            InputManager.Instance.OnRightClick -= HandleRightClick;
            
            Bus<ScoringSequenceStartedEvent>.OnEvent -= HandleScoringSequenceStarted;
            Bus<ScoringSequenceOverEvent>.OnEvent -= HandleScoringSequenceOver; 
        }
        private void OnDestroy() => OnDisable(); 
        #endregion
        
        #region Subscribed
        private void HandleLeftClick(Vector2 screenPos)
        {
            if (!GameManager.Instance.IsRoundPlayState()) return;
            if (!_canInteract) return; 
            
            TryToggleTileByClick(screenPos);
        }

        private void HandleRightClick(Vector2 screenPos)
        {
            if (!GameManager.Instance.IsRoundPlayState()) return;
            if (!_canInteract) return;
            
            RedrawTile(screenPos);
        }
        
        private void HandleScoringSequenceStarted(ScoringSequenceStartedEvent evt) => _canInteract = false;
        private void HandleScoringSequenceOver(ScoringSequenceOverEvent evt) => _canInteract = true; 
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
            
            // Ending drag on the player's hand. 
            if (IsPointerOverLayer(pointerPos, handLayer))
            {
                tile.EndDrag();
                Bus<TileMoveRequestEvent>.Raise(new TileMoveRequestEvent(tile, GamePosition.Hand, null));
                return true;
            }
            
            // Ending drag on the redraw area. 
            if (IsPointerOverLayer(pointerPos, drawLayer))
            {
                tile.EndDrag();
                Bus<TileRedrawEvent>.Raise(new TileRedrawEvent(tile, tile.Tile));
                return true;
            }
            
            // Ending drag on the board or near the board area. 
            var slot = BoardManager.Instance.GetPreviewedSlot();
            if (slot != null)
            {
                var target = slot.GetComponent<RectTransform>();
                if (AnimationHelper.AreRectTransformCloseEnough(tile.RectTransform, target, snapToSlotDistance))
                {
                    tile.EndDrag();
                    Bus<TileMoveRequestEvent>.Raise(new TileMoveRequestEvent(tile, GamePosition.Board, slot));
                    return true;
                }
            }
            
            // Fallback => cancel the drag
            tile.EndDrag();
            return true; 
        }

        private void TryToggleTileByClick(Vector2 screenPos)
        {
            var tileView = RaycastTile(screenPos);
            if (tileView == null) return;

            // Move from the hand to the board. 
            if (tileView.IsInHand)
            {
                Bus<TileMoveRequestEvent>.Raise(new TileMoveRequestEvent(tileView, GamePosition.Board, null));
                return;
            }

            // Move from the board to the hand. 
            Bus<TileMoveRequestEvent>.Raise(new TileMoveRequestEvent(tileView, GamePosition.Hand, null));
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

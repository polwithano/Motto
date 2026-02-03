using TMPro;
using UnityEngine;
using DG.Tweening;
using Managers;
using UnityEngine.InputSystem;

namespace UI.Tooltips
{
    public abstract class TooltipBase : MonoBehaviour
    {
        public enum TooltipAnchor
        {
            TopLeft,
            TopRight,
            BottomLeft,
            BottomRight
        }

        [Header("Base UI References")]
        [SerializeField] protected CanvasGroup canvasGroup;
        [SerializeField] protected RectTransform root;

        [Header("Positioning")]
        [SerializeField] private TooltipAnchor anchorPosition = TooltipAnchor.TopRight;
        [SerializeField] private Vector2 padding = new (16f, 16f); 

        [Header("Animation Settings")]
        [SerializeField] private float fadeDuration = 0.2f;
        [SerializeField] private float scalePop = 1.05f;

        private Canvas _canvas;
        private Camera _uiCamera;
        private bool _isActive;
        private InputManager _inputManager;

        protected virtual void Awake()
        {
            _canvas = GetComponentInParent<Canvas>();
            _inputManager = InputManager.Instance;

            // Detect correct camera
            if (_canvas.renderMode == RenderMode.ScreenSpaceOverlay)
                _uiCamera = null;
            else
                _uiCamera = _canvas.worldCamera != null ? _canvas.worldCamera : Camera.main;

            HideInstant();
        }

        protected virtual void Update()
        {
            if (!_isActive || canvasGroup.alpha <= 0f) return;

            var pointerPos = _inputManager.PointerPosition; 

            if (_canvas == null || root == null) return;

            var parentRect = root.parent as RectTransform;

            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                parentRect,
                pointerPos,
                _uiCamera,
                out var localPos);

            var size = root.rect.size;
            var offset = CalculateOffset(size);

            var targetPos = localPos + offset;
            root.anchoredPosition = ClampToCanvas(targetPos);
        }

        public virtual void Show()
        {
            canvasGroup.alpha = 0f;
            root.localScale = Vector3.one * 0.95f;
            _isActive = true;

            var seq = DOTween.Sequence();
            seq.Append(canvasGroup.DOFade(1f, fadeDuration));
            seq.Join(root.DOScale(scalePop, fadeDuration * 1.2f).SetEase(Ease.OutBack));
            seq.Append(root.DOScale(1f, fadeDuration * 0.5f).SetEase(Ease.OutSine));
            
            gameObject.SetActive(true);
        }

        public virtual void Hide()
        {
            _isActive = false;
            canvasGroup.DOFade(0f, 0.15f)
                .OnComplete(() => gameObject.SetActive(false));
        }

        public void HideInstant()
        {
            _isActive = false;
            canvasGroup.alpha = 0f;
            gameObject.SetActive(false);
        }

        public abstract void Populate(object data);
        
        private Vector2 CalculateOffset(Vector2 size)
        {
            switch (anchorPosition)
            {
                case TooltipAnchor.TopLeft:
                    return new Vector2(-size.x - padding.x, padding.y);
                case TooltipAnchor.TopRight:
                    return new Vector2(padding.x, padding.y);
                case TooltipAnchor.BottomLeft:
                    return new Vector2(-size.x - padding.x, -size.y - padding.y);
                case TooltipAnchor.BottomRight:
                    return new Vector2(padding.x, -size.y - padding.y);
                default:
                    return Vector2.zero;
            }
        }
        
        private Vector2 ClampToCanvas(Vector2 position)
        {
            var parentRect = root.parent as RectTransform;
            var canvasSize = parentRect.rect.size;
            var tooltipSize = root.rect.size;

            var pivot = root.pivot;

            float minX = -canvasSize.x * 0.5f + tooltipSize.x * pivot.x;
            float maxX =  canvasSize.x * 0.5f - tooltipSize.x * (1f - pivot.x);

            float minY = -canvasSize.y * 0.5f + tooltipSize.y * pivot.y;
            float maxY =  canvasSize.y * 0.5f - tooltipSize.y * (1f - pivot.y);

            position.x = Mathf.Clamp(position.x, minX, maxX);
            position.y = Mathf.Clamp(position.y, minY, maxY);

            return position;
        }
    }
}

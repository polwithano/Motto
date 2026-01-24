using TMPro;
using UnityEngine;
using DG.Tweening;
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
        [SerializeField] private Vector2 padding = new Vector2(16f, 16f); // small margin from cursor

        [Header("Animation Settings")]
        [SerializeField] private float fadeDuration = 0.2f;
        [SerializeField] private float scalePop = 1.05f;

        private Canvas _canvas;
        private Camera _uiCamera;
        private bool _isActive = false;

        protected virtual void Awake()
        {
            _canvas = GetComponentInParent<Canvas>();

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

            // Read mouse position
            Vector2 mousePos = Mouse.current != null
                ? Mouse.current.position.ReadValue()
                : Vector2.zero;

            // Convert screen to local
            if (_canvas == null || root == null) return;

            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                _canvas.transform as RectTransform, mousePos, _uiCamera, out var localPos);

            // Get frame size
            Vector2 size = root.rect.size;

            // Calculate offset dynamically based on anchor
            Vector2 offset = CalculateOffset(size);

            root.anchoredPosition = localPos + offset;
        }

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

        public virtual void Show()
        {
            gameObject.SetActive(true);
            canvasGroup.alpha = 0f;
            root.localScale = Vector3.one * 0.95f;
            _isActive = true;

            Sequence seq = DOTween.Sequence();
            seq.Append(canvasGroup.DOFade(1f, fadeDuration));
            seq.Join(root.DOScale(scalePop, fadeDuration * 1.2f).SetEase(Ease.OutBack));
            seq.Append(root.DOScale(1f, fadeDuration * 0.5f).SetEase(Ease.OutSine));
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
    }
}

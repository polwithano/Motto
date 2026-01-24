using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;

namespace UI.Animations
{
    /// <summary>
    /// Adds a punch + hover scale effect when the user hovers over the UI element.
    /// </summary>
    public class PunchHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [Header("Punch Settings")]
        [SerializeField] private float punchScaleAmount = 1.15f;  // how strong the punch pop is
        [SerializeField] private float punchDuration = 0.2f;
        [SerializeField] private int punchVibrato = 8;
        [SerializeField] private float punchElasticity = 1f;

        [Header("Hover Hold Settings")]
        [SerializeField] private float hoverScaleAmount = 1.05f;  // steady scale while hovered
        [SerializeField] private float hoverTransitionDuration = 0.15f;

        [Header("Exit Settings")]
        [SerializeField] private float exitPunchScale = 1.1f;
        [SerializeField] private float exitPunchDuration = 0.2f;
        [SerializeField] private int exitVibrato = 8;
        [SerializeField] private float exitElasticity = 1f;

        private Vector3 _originalScale;
        private Tween _activeTween;

        private void Awake()
        {
            _originalScale = transform.localScale;
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            _activeTween?.Kill();

            // Create a sequence for entry animation
            Sequence seq = DOTween.Sequence();

            // 1️⃣ Quick punch up
            seq.Append(transform.DOPunchScale(
                new Vector3(punchScaleAmount - 1f, punchScaleAmount - 1f, 0f),
                punchDuration,
                punchVibrato,
                punchElasticity
            ));

            // 2️⃣ Then gently scale to hover hold amount
            seq.Append(transform.DOScale(_originalScale * hoverScaleAmount, hoverTransitionDuration)
                .SetEase(Ease.OutBack));

            _activeTween = seq;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            _activeTween?.Kill();

            Sequence seq = DOTween.Sequence();

            // 1️⃣ Small punch down for feedback
            seq.Append(transform.DOPunchScale(
                new Vector3(exitPunchScale - 1f, exitPunchScale - 1f, 0f),
                exitPunchDuration,
                exitVibrato,
                exitElasticity
            ));

            // 2️⃣ Then smoothly return to original scale
            seq.Append(transform.DOScale(_originalScale, 0.2f)
                .SetEase(Ease.OutBack));

            _activeTween = seq;
        }

        private void OnDisable()
        {
            _activeTween?.Kill();
            transform.localScale = _originalScale;
        }
    }
}

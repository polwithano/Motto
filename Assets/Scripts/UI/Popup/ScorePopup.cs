using DG.Tweening;
using Models;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Popup
{
    public class ScorePopup : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI text;
        [SerializeField] private CanvasGroup canvasGroup;

        [SerializeField] private Color scoreTargetColor;
        [SerializeField] private Color modifierTargetColor; 

        [Header("Anim")]
        [SerializeField] private float lifetime = 0.6f;
        [SerializeField] private float floatDistance = 0.5f;
        [SerializeField] private Ease moveEase = Ease.OutQuad;
        [SerializeField] private Ease fadeEase = Ease.InQuad;

        public void Play(int points, ScoreEffectTarget target)
        {
            text.text = $"+{points}";
            var bg = GetComponent<Image>();
            if (target == ScoreEffectTarget.Score) bg.color = scoreTargetColor;
            if (target == ScoreEffectTarget.Modifier) bg.color = modifierTargetColor;
            
            Animate();
        }

        private void Animate()
        {
            // Reset state in case this prefab is reused
            canvasGroup.alpha = 1f;

            Vector3 start = transform.localPosition;
            Vector3 end = start + Vector3.up * floatDistance;

            // Create a sequence for clean timing control
            Sequence seq = DOTween.Sequence();

            // Move upwards
            seq.Join(transform.DOLocalMove(end, lifetime)
                .SetEase(moveEase));

            // Fade out over time
            seq.Join(canvasGroup.DOFade(0f, lifetime)
                .SetEase(fadeEase));

            // Destroy after animation
            seq.OnComplete(() => Destroy(gameObject));
        }
    }
}
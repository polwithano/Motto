using DG.Tweening;
using UnityEngine;

namespace UI.Animation
{
    public class AnimationPunch : MonoBehaviour
    {
        [SerializeField] private float scale;
        [SerializeField] private float duration;
        [SerializeField] private int vibrato;
        [SerializeField] private float elasticity;
        [SerializeField] private Ease ease;
    
        public void Animate()
        {
            var rect = GetComponent<RectTransform>();
            if (rect == null || !rect.gameObject.activeInHierarchy) return;
    
            rect.DOKill();
            rect.localScale = Vector3.one;
            rect.DOPunchScale(Vector3.one * scale, duration, vibrato, elasticity)
                .SetEase(ease)
                .SetLink(gameObject); 
        }
    }
}

using DG.Tweening;
using UnityEngine;

namespace Animation
{
    public static class AnimationHelper
    {
        public static void AnimateRectTransformToPosition(
            RectTransform transform, 
            Vector3 position, 
            System.Action onComplete = null,
            float duration = 0.225f,
            Ease ease = Ease.InExpo)
        {
            transform
                .DOMove(position, duration)
                .SetEase(ease)
                .OnComplete(() => onComplete?.Invoke());
        }

        public static bool AreRectTransformCloseEnough(RectTransform transform, RectTransform target, float distance)
        {
            return Vector3.Distance(transform.position, target.position) <= distance;
        }
    }
}
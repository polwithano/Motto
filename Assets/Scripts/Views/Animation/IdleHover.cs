using DG.Tweening;
using UnityEngine;

namespace Views.Animation
{
    public class IdleHover : MonoBehaviour
    {
        [Header("Hover Settings")]
        [SerializeField] private float hoverDistance = 10f;        // pixels of up/down motion
        [SerializeField] private float hoverDuration = 2f;         // time for one full up-down cycle
        [SerializeField] private float randomOffset = 0.5f;        // random desync factor
        [SerializeField] private float horizontalDrift = 5f;       // optional horizontal movement
        [SerializeField] private float driftDuration = 3f;

        private Vector3 _startPos;
        private Sequence _hoverSequence;

        private void Start()
        {
            _startPos = transform.localPosition;

            // Add a random starting delay and offset so each charm moves differently
            float startDelay = Random.Range(0f, hoverDuration * randomOffset);
            float randomY = Random.Range(-2f, 2f);
            float randomX = Random.Range(-2f, 2f);

            transform.localPosition = _startPos + new Vector3(randomX, randomY, 0f);

            CreateHoverSequence(startDelay);
        }

        private void CreateHoverSequence(float delay)
        {
            _hoverSequence = DOTween.Sequence();

            // Vertical bobbing
            _hoverSequence.Append(
                transform.DOLocalMoveY(_startPos.y + hoverDistance, hoverDuration)
                    .SetEase(Ease.InOutSine)
            );
            _hoverSequence.Append(
                transform.DOLocalMoveY(_startPos.y - hoverDistance, hoverDuration)
                    .SetEase(Ease.InOutSine)
            );

            // Loop forever
            _hoverSequence.SetLoops(-1, LoopType.Yoyo);
            _hoverSequence.SetDelay(delay);

            // Optional horizontal drift (looped separately)
            transform.DOLocalMoveX(_startPos.x + Random.Range(-horizontalDrift, horizontalDrift), driftDuration)
                .SetEase(Ease.InOutSine)
                .SetLoops(-1, LoopType.Yoyo)
                .SetDelay(delay * 0.5f);
        }

        private void OnDisable()
        {
            // Clean up to avoid leaks if object gets disabled/destroyed
            _hoverSequence?.Kill();
            transform.DOKill();
        }
    }
}

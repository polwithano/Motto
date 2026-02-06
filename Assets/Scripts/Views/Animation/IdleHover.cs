using DG.Tweening;
using UnityEngine;

namespace Views.Animation
{
    public class IdleHover : MonoBehaviour
    {
        [Header("Hover Settings")]
        [SerializeField] private float hoverDistance = 10f;
        [SerializeField] private float hoverDuration = 2f;
        [SerializeField] private float randomOffset = 0.5f;
        [SerializeField] private float horizontalDrift = 5f;
        [SerializeField] private float driftDuration = 3f;

        private Vector3 _startPos;
        private Sequence _hoverSequence;
        private Tween _horizontalTween;
        private bool _initialized;

        private void Awake()
        {
            _startPos = transform.localPosition;
        }

        private void OnEnable()
        {
            Play();
        }

        private void OnDisable()
        {
            Stop();
        }

        private void OnDestroy()
        {
            Stop();
        }

        public void Play()
        {
            if (_hoverSequence != null && _hoverSequence.IsActive())
                return;

            if (!_initialized)
            {
                ApplyRandomOffset();
                _initialized = true;
            }

            CreateHoverSequence();
        }

        public void Stop()
        {
            _hoverSequence?.Kill();
            _hoverSequence = null;

            _horizontalTween?.Kill();
            _horizontalTween = null;

            transform.localPosition = _startPos;
        }

        private void ApplyRandomOffset()
        {
            var randomY = Random.Range(-2f, 2f);
            var randomX = Random.Range(-2f, 2f);
            transform.localPosition = _startPos + new Vector3(randomX, randomY, 0f);
        }

        private void CreateHoverSequence()
        {
            var delay = Random.Range(0f, hoverDuration * randomOffset);

            _hoverSequence = DOTween.Sequence()
                .Append(
                    transform.DOLocalMoveY(_startPos.y + hoverDistance, hoverDuration)
                        .SetEase(Ease.InOutSine)
                )
                .Append(
                    transform.DOLocalMoveY(_startPos.y - hoverDistance, hoverDuration)
                        .SetEase(Ease.InOutSine)
                )
                .SetLoops(-1, LoopType.Yoyo)
                .SetDelay(delay);

            _horizontalTween = transform
                .DOLocalMoveX(
                    _startPos.x + Random.Range(-horizontalDrift, horizontalDrift),
                    driftDuration
                )
                .SetEase(Ease.InOutSine)
                .SetLoops(-1, LoopType.Yoyo)
                .SetDelay(delay * 0.5f);
        }
    }
}

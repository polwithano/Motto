using DG.Tweening;
using UnityEngine;

namespace Animation.Menu
{
    public class TitleAnimation : MonoBehaviour
    {
        private RectTransform[] _letters; 
        private Vector3[] _positions;

        private void Awake()
        {
            _letters = new RectTransform[transform.childCount];
            _positions = new Vector3[transform.childCount];
            
            for (var i = 0; i < _letters.Length; i++)
            {
                _letters[i] = transform.GetChild(i).GetComponent<RectTransform>();;
                _positions[i] = _letters[i].anchoredPosition;
            }
        }

        private void Start()
        {
            AnimateTiles();
        }

        private void AnimateTiles()
        {
            var screenTop = ((RectTransform)transform).rect.height / 2f;
            
            for (var i = 0; i < _letters.Length; i++)
            {
                var letter = _letters[i];
                var targetPos = _positions[i];
                var startY = screenTop + letter.rect.height * 2;

                letter.anchoredPosition = new Vector2(targetPos.x, startY);
                letter.localScale = Vector3.one;

                var impactPlayed = false;
                var wasFalling = false;
                var prevY = 0f;
                var index = i;
                
                letter.DOAnchorPos(targetPos, 0.65f)
                    .SetDelay(i * 0.125f)
                    .SetEase(Ease.OutBounce)
                    .OnStart(() =>
                    {
                        impactPlayed = false;
                        prevY = letter.anchoredPosition.y;
                        wasFalling = true;
                    })
                    .OnUpdate(() =>
                    {
                        if (impactPlayed) return;

                        var y = letter.anchoredPosition.y;
                        var dy = y - prevY;

                        if (dy < -0.0005f)
                        {
                            wasFalling = true;
                        }
                        else if (wasFalling && dy > 0.0005f)
                        {
                            impactPlayed = true;
                            PlayImpact(letter, index);
                        }
                        prevY = y;
                    });
            }
        }
        
        private void PlayImpact(RectTransform letter, int index)
        {
            var impact = DOTween.Sequence();

            impact.Append(
                letter.DOScale(new Vector3(1.25f, 0.6f, 1f), 0.08f)
                    .SetEase(Ease.OutQuad)
            );

            impact.Append(
                letter.DOScale(new Vector3(0.9f, 1.15f, 1f), 0.1f)
                    .SetEase(Ease.OutQuad)
            );

            impact.Append(
                letter.DOScale(Vector3.one, 0.2f)
                    .SetEase(Ease.OutElastic)
            );
            
            impact.OnComplete(() =>
            {
                StartIdleAnimation(letter, index);
            });
        }
        
        private void StartIdleAnimation(RectTransform letter, int index)
        {
            var baseY = _positions[index].y;
            var randomOffset = Random.Range(0f, 2f);

            letter.DOAnchorPosY(baseY + 8f, 2.2f)
                .SetEase(Ease.InOutSine)
                .SetLoops(-1, LoopType.Yoyo)
                .SetDelay(randomOffset);

            letter.DORotate(new Vector3(0, 0, Random.Range(-4f, 4f)), 2.5f)
                .SetEase(Ease.InOutSine)
                .SetLoops(-1, LoopType.Yoyo)
                .SetDelay(randomOffset * 0.5f);

            letter.DOScale(1.06f, 2.2f)
                .SetEase(Ease.InOutSine)
                .SetLoops(-1, LoopType.Yoyo)
                .SetDelay(randomOffset * 0.6f);
        }
    }
}

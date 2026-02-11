using System;
using DG.Tweening;
using Events;
using Events.Core;
using Events.Rounds;
using Events.Score;
using Models;
using UnityEngine;

namespace Animation
{
    public class UIScoreLogEffect : MonoBehaviour
    {
        [SerializeField] private Material targetMaterial;
        [SerializeField] private float tweenDuration = 0.35f;
        [SerializeField] private Ease tweenEase = Ease.OutCubic;

        private static readonly int SplitValueID = Shader.PropertyToID("_SplitValue");

        private Tween _splitTween;

        private void OnEnable()
        {
            Bus<RoundStartedEvent>.OnEvent += HandleOnRoundStarted;
            Bus<ScoringStepProcessedEvent>.OnEvent += HandleScoreStepApplied;
        }

        private void OnDisable()
        {
            Bus<RoundStartedEvent>.OnEvent -= HandleOnRoundStarted;
            Bus<ScoringStepProcessedEvent>.OnEvent -= HandleScoreStepApplied;

            _splitTween?.Kill();
        }

        private void HandleOnRoundStarted(RoundStartedEvent evt)
        {
            _splitTween?.Kill();
            AnimateSplitValue(0f);
        }

        private void HandleScoreStepApplied(ScoringStepProcessedEvent evt)
        {
            var score = Mathf.Max(evt.Entry.EntryScore, 0f);
            var modifier = Mathf.Max(evt.Entry.EntryModifier, 0f);

            var targetValue = (modifier - score) / (modifier + score + 0.0001f);

            AnimateSplitValue(targetValue);
        }

        private void AnimateSplitValue(float targetValue)
        {
            _splitTween?.Kill();

            var currentValue = targetMaterial.GetFloat(SplitValueID);

            _splitTween = DOTween.To(
                    () => currentValue,
                    x =>
                    {
                        currentValue = x;
                        targetMaterial.SetFloat(SplitValueID, x);
                    },
                    targetValue,
                    tweenDuration
                )
                .SetEase(tweenEase);
        }
    }
}

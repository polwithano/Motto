using System.Collections.Generic;
using Models;
using Models.Rewards;
using UnityEngine;

namespace Processors
{
    public static class RoundRewardProcessor
    {
        public static RoundRewardResult ProcessRoundResult(Run run)
        {
            var context = run.GetCurrentRoundContext();
            var currentSoftCurrency = run.SoftCurrency; 
            var entries = new List<RoundRewardEntry>();

            // Value remaining words
            var bonusRemainingWords = context.WordsRemaining * 5;
            entries
                .Add(new RoundRewardEntry(
                    "Words remaining", 
                    context.WordsRemaining,
                    bonusRemainingWords));
            
            // Value remaining draws
            var bonusRemainingDraws = Mathf.FloorToInt(context.DrawsRemaining * .25f);
            entries
                .Add(new RoundRewardEntry(
                    "Draws remaining", 
                    context.DrawsRemaining,
                    bonusRemainingDraws));
            
            // Value score overshoot
            var overshoot = Mathf.Max(0, context.CurrentScore / context.TargetScore);
            var bonusOvershoot = Mathf.RoundToInt(overshoot * 2f); 
            entries
                .Add(new RoundRewardEntry(
                    "Score overshoot", 
                    overshoot,
                    bonusOvershoot));
            
            // Value round
            entries
                .Add(new RoundRewardEntry(
                    "Round reward", 
                    run.RoundIndex,
                    context.SoftCurrencyReward));
            
            // Value hoarding
            var bonusHoard = (int)(currentSoftCurrency / 10); 
            entries
                .Add(new RoundRewardEntry(
                    "Hoard bonus",
                    (int)currentSoftCurrency,
                    bonusHoard));
            
            return new RoundRewardResult(entries); ; 
        }
    }
}
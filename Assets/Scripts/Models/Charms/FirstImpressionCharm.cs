using UnityEngine;

namespace Models.Charms
{
    [CreateAssetMenu(menuName = "Charms/FirstImpression")]
    public class FirstImpressionCharm : Charm
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        public override bool TryApplyEffect(RoundContext context, int? index = null)
        {
            return index == 0;
        }

        public override void ApplyEffect(ScoreLog log)
        {
            var tileValue = log.Tiles[0].Points;
            var scoreEffect = new ScoreEffect(ScoreEffects[0].Target, tileValue);
            var entry = new ScoreLogEntry(log.Logs.Count, this, scoreEffect);
            log.AddEntry(entry);
        }
    }
}
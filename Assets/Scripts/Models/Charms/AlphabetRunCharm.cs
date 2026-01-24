using UnityEngine;

namespace Models.Charms
{
    [CreateAssetMenu(menuName = "Charms/AlphabetRun")]
    public class AlphabetRunCharm : Charm
    {
        public override bool TryApplyEffect(RoundContext context, int? index = null)
        {
            return false; 
        }
    }
}
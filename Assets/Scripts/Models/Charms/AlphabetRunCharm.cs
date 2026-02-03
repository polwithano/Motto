using System.Collections.Generic;
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

        public override bool WillPreviewEffect(string word, List<Tile> tiles = null, int? index = null)
        {
            return false;
        }
    }
}
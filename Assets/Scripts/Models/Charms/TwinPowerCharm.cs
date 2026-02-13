using System.Collections.Generic;
using Models.Charms.Core;
using Models.Rounds;
using UnityEngine;

namespace Models.Charms
{
    [CreateAssetMenu(menuName = "Charms/TwinPower")]
    public class TwinPowerCharm : Charm
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        public override bool TryApplyEffect(RoundContext context, int? index = null)
        {
            if (index == null) return false;

            var tiles = context.LastlyAddedTiles();
            if (tiles == null || tiles.Count == 0) return false;

            var i = index.Value;

            if (i <= 0 || i >= tiles.Count) return false;

            return tiles[i].Character == tiles[i - 1].Character;
        }

        public override bool WillPreviewEffect(string word, List<Tile> tiles = null, int? index = null)
        {
            if (string.IsNullOrEmpty(word)) return false;

            for (var i = 1; i < word.Length; i++)
            {
                if (word[i] == word[i - 1])
                    return true;
            }

            return false;
        }
    }
}

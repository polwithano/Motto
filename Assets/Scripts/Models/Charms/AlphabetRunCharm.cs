using System.Collections.Generic;
using Models.Charms.Core;
using Models.Rounds;
using UnityEngine;

namespace Models.Charms
{
    [CreateAssetMenu(menuName = "Charms/AlphabetRun")]
    public class AlphabetRunCharm : Charm
    {
        public override bool TryApplyEffect(RoundContext context, int? index = null)
        {
            if (index == null) return false;

            var tiles = context.LastlyAddedTiles();
            if (tiles == null || tiles.Count == 0) return false;

            int i = index.Value;

            if (i <= 0 || i >= tiles.Count) return false;

            char current = char.ToUpper(tiles[i].Character);
            char previous = char.ToUpper(tiles[i - 1].Character);

            return current == previous + 1;
        }


        public override bool WillPreviewEffect(string word, List<Tile> tiles = null, int? index = null)
        {
            if (string.IsNullOrEmpty(word)) return false;

            word = word.ToUpper();

            for (int i = 1; i < word.Length; i++)
            {
                if (word[i] == word[i - 1] + 1)
                    return true;
            }

            return false;
        }
    }
}
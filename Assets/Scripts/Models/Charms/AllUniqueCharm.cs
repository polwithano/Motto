using System.Collections.Generic;
using Models.Charms.Core;
using Models.Rounds;
using UnityEngine;

namespace Models.Charms
{
    [CreateAssetMenu(menuName = "Charms/AllUnique")]
    public class AllUniqueCharm : Charm
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        public override bool TryApplyEffect(RoundContext context, int? index = null)
        {
            return AreAllLettersUnique(context.Words[^1]); 
        }

        public override bool WillPreviewEffect(string word, List<Tile> tiles = null, int? index = null)
        {
            return AreAllLettersUnique(word); 
        }

        private bool AreAllLettersUnique(string word)
        {
            var seen = new HashSet<char>();
            var allUnique = true;

            foreach (var c in word)
            {
                if (seen.Contains(c))
                    allUnique = false;
                seen.Add(c);
            }

            return allUnique;
        }
    }
}

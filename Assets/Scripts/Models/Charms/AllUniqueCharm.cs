using System.Collections.Generic;
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
            HashSet<char> seenCharacters = new HashSet<char>();
            bool isApplied = true;
            
            foreach (char c in context.Words[^1])
            {
                if (seenCharacters.Contains(c))
                {
                    isApplied = false;
                }
                seenCharacters.Add(c);
            }
            
            return isApplied; 
        }
    }
}

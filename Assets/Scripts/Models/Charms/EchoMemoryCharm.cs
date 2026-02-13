using System.Collections.Generic;
using Models.Charms.Core;
using Models.Rounds;
using UnityEngine;

namespace Models.Charms
{
    [CreateAssetMenu(menuName = "Charms/EchoMemory")]
    public class EchoMemoryCharm : Charm
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        /// <param name="index"></param>
        /// <returns></returns>
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
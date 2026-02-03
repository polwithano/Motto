using System.Collections.Generic;
using UnityEngine;

namespace Models.Charms
{
    [CreateAssetMenu(menuName = "Charms/BlankPower")]
    public class BlankPowerCharm : Charm
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        public override bool TryApplyEffect(RoundContext context, int? index = null)
        {
            return index != null && context.Tiles[^1][(int)index].IsBlank; 
        }

        public override bool WillPreviewEffect(string word, List<Tile> tiles = null, int? index = null)
        {
            return index != null && tiles != null && tiles[(int)index].IsBlank; 
        }
    }
}
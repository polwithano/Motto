using System;
using System.Linq;
using UnityEngine;

namespace Models.Charms
{
    [CreateAssetMenu(menuName = "Charms/Palindrome")]
    public class PalindromeCharm : Charm
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
    }
}

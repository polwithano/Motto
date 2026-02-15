using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Models.Rewards
{
    [System.Serializable]
    public class RoundRewardResult
    {
        [field: SerializeField] public List<RoundRewardEntry> Entries { get; private set; }
        [field: SerializeField] public int TotalCurrency { get; private set; }

        public RoundRewardResult(List<RoundRewardEntry> entries)
        {
            Entries = entries;
            TotalCurrency = entries.Sum(e => e.SoftReward);
        }
    }
}
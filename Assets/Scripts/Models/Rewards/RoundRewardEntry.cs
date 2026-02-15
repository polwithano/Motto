using UnityEngine;

namespace Models.Rewards
{
    [System.Serializable]
    public class RoundRewardEntry
    {
        [field: SerializeField] public string Label      { get; private set; }
        [field: SerializeField] public string RawValue   { get;  private set; }
        [field: SerializeField] public int SoftReward    { get; private set; }

        public RoundRewardEntry(string label, string rawValue, int softReward)
        {
            Label = label;
            RawValue = rawValue;
            SoftReward = softReward;
        }
    }
}
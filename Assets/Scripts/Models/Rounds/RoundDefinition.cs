using UnityEngine;

namespace Models.Rounds
{
    [System.Serializable]
    public class RoundDefinition
    {
        [field: SerializeField] public RoundType RoundType        { get; private set; }
        [field: SerializeField] public int BaseTargetScore        { get; private set; }
        [field: SerializeField] public int BaseSoftCurrencyReward { get; private set; }
    }
}
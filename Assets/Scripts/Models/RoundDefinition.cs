using UnityEngine;

namespace Models
{
    [System.Serializable]
    public class RoundDefinition
    {
        [field: SerializeField] public RoundType RoundType { get; private set; }
        [field: SerializeField] public int BaseTargetScore { get; private set; }
    }
}
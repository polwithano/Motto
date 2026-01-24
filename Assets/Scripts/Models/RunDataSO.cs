using UnityEngine;

namespace Models
{
    [CreateAssetMenu(fileName = "RunDataSO", menuName = "Models/Run Data")]
    public class RunDataSO : ScriptableObject
    {
        [field: SerializeField] public RoundDefinition[] RoundSequence    { get; private set; }
    
        public RoundDefinition GetRoundData(int index) => RoundSequence[index];
    }
}
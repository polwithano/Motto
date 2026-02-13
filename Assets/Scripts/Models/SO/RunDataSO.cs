using Models.Rounds;
using UnityEngine;

namespace Models.SO
{
    [CreateAssetMenu(fileName = "RunDataSO", menuName = "Models/Run Data")]
    public class RunDataSO : ScriptableObject
    {
        [field: SerializeField] public RoundDefinition[] RoundsSequence { get; private set; }
    
        public RoundDefinition GetRoundData(int index) => RoundsSequence[index];
    }
}
using FSM;
using Models;
using UnityEngine;

namespace Managers
{
    public class GameManager : MonoBehaviourSingleton<GameManager>
    {
        [Header("Debug")]
        [field: SerializeField] public bool DisableWordCheck { get; private set; } = false;
        [field: SerializeField] public string LanguageCode   { get; private set; } = "en";
        
        [field: SerializeField] public GameStateMachine GameState                  { get; private set; }
        [field: SerializeField] public TileDistributionRuleSO TileDistributionRule { get; set; }
        [field: SerializeField] public RunDataSO RunData                           { get; set; }

        [field: SerializeField] public Run Run       { get; set; }
        [field: SerializeField] public TileDeck Deck { get; set; }
        [field: SerializeField] public TileHand Hand { get; set; }
        
        #region Mono
        private void Start()
        {
            GameState = gameObject.AddComponent<GameStateMachine>();
            GameState.ChangeState(new BootState(GameState));
        }
        #endregion
    }
}

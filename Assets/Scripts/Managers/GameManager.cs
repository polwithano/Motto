using FSM;
using FSM.States;
using Models;
using Models.Charms;
using Models.Charms.Core;
using Models.SO;
using UnityEngine;

namespace Managers
{
    public class GameManager : MonoBehaviourSingleton<GameManager>
    {
        [Header("Debug")]
        [field: SerializeField] public bool DisableWordCheck { get; private set; } = false;
        [field: SerializeField] public string LanguageCode   { get; private set; } = "en";
        
        [field: SerializeField] public GameStateMachine GameState                  { get; private set; }
        [field: SerializeField] public TileDistributionRuleSO TileDistributionRule { get; private set; }
        [field: SerializeField] public TileDistributionRuleSO StartingDeck         { get; private set; }
        [field: SerializeField] public CharmDatabase CharmsDatabase                { get; private set; }
        [field: SerializeField] public RunDataSO RunData                           { get; private set; }

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

        public bool IsControllerAllowed()
        {
            return GameState && GameState.CurrentState is RoundPlayState; 
        }
    }
}

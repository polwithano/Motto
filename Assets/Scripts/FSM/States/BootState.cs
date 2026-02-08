using Managers;
using Models;

namespace FSM.States
{
    [System.Serializable]
    public class BootState : GameState
    {
        public BootState(GameStateMachine machine) : base(machine) { }

        public override void Enter()
        {
            CharmManager.Instance.InitializeCharms();
            
            Game.Deck = new TileDeck(Game.StartingDeck);
            Game.Hand = new TileHand();
            Game.Run = new Run(Game.RunData);
        
            StateMachine.ChangeState(new RoundStartState(StateMachine));
        }
    }
}
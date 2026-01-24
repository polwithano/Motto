using Models;
using Managers;

namespace FSM
{
    public class BootState : GameState
    {
        public BootState(GameStateMachine machine) : base(machine) { }

        public override void Enter()
        {
            CharmManager.Instance.InitializeCharms();
            
            Game.Deck = new TileDeck(Game.TileDistributionRule);
            Game.Hand = new TileHand();
            Game.Run = new Run(Game.RunData);
        
            StateMachine.ChangeState(new RoundStartState(StateMachine));
        }
    }
}
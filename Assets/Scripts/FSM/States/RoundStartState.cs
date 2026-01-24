using Events;
using UnityEngine;
using Views;

namespace FSM
{
    public class RoundStartState : GameState
    {
        public RoundStartState(GameStateMachine machine) : base(machine) { }

        public override void Enter()
        {
            Game.Run.LoadContext();
            Game.Hand.FillFromDeck(Game.Deck);
            
            HandView.Instance.InstantiateHand(Game.Hand);

            GameEvents.RaiseOnRoundStarted(Game.Run.Round);
            
            StateMachine.ChangeState(new RoundPlayState(StateMachine));
        }

        public override void Tick()
        {
            
        }
    }
}
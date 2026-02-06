using Events;
using Events.Core;
using Events.Rounds;
using Views;

namespace FSM.States
{
    [System.Serializable]
    public class RoundStartState : GameState
    {
        public RoundStartState(GameStateMachine machine) : base(machine) { }

        public override void Enter()
        {
            Game.Run.LoadContext();
            Game.Hand.FillFromDeck(Game.Deck);
            
            HandView.Instance.InstantiateHand(Game.Hand);

            Bus<RoundStartedEvent>.Raise(new RoundStartedEvent(Game.Run.Round));
            
            StateMachine.ChangeState(new RoundPlayState(StateMachine));
        }

        public override void Tick()
        {
            
        }
    }
}
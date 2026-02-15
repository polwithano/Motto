using Events.Core;
using Events.Rounds;
using Events.UI;
using UnityEngine;

namespace FSM.States
{
    [System.Serializable]
    public class RoundOverState : GameState
    {
        public readonly RoundEndedStatus Status; 
        
        public RoundOverState(GameStateMachine machine, RoundEndedStatus status) : base(machine)
        {
            Status = status;
        }

        public override void Enter()
        {
            Bus<SetUIContainerStateEvent>.OnEvent += HandleOnUIStateUpdated;

            if (RunCanContinue())
            {
                Bus<SetUIContainerStateEvent>
                    .Raise(new SetUIContainerStateEvent(UIType.RoundOver, UIState.Opened));   
            }
            else
            {
                StateMachine.ChangeState(new RunOverState(StateMachine, Status));
            }
        }

        public override void Exit()
        {
            Bus<SetUIContainerStateEvent>.OnEvent -= HandleOnUIStateUpdated; 
        }
        public override void Tick() {}

        private void HandleOnUIStateUpdated(SetUIContainerStateEvent evt)
        {
            if (evt.Container != UIType.RoundOver) return; 
            if (evt.State != UIState.Closed) return;
            
            StateMachine.ChangeState(new ShopState(StateMachine));
        }
        
        private bool RunCanContinue() => Status == RoundEndedStatus.Success && Game.Run.TryIncrementRound(); 
    }
}
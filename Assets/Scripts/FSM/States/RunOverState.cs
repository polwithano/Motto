using Events.Core;
using Events.Rounds;
using Events.UI;
using UnityEngine;

namespace FSM.States
{
    [System.Serializable]
    public class RunOverState : GameState
    {
        public readonly RoundEndedStatus Status;

        public RunOverState(GameStateMachine machine, RoundEndedStatus status) : base(machine)
        {
            Status = status;
        }

        public override void Enter()
        {
            Debug.Log(Status == RoundEndedStatus.Success ? "Run is won" : "Run is lost");
            
            Bus<SetUIContainerStateEvent>
                .Raise(new SetUIContainerStateEvent(UIType.RunOver, UIState.Opened));
        }

        public override void Exit()
        {
            
        }
        public override void Tick() {}
    }
}
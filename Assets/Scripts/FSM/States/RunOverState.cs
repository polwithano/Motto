using Events.Rounds;
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
        }
        
        public override void Exit() {}
        public override void Tick() {}
    }
}
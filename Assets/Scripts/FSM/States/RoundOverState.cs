using Events.Rounds;
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
            Debug.Log(Status == RoundEndedStatus.Success ? "Round is won" : "Round is lost");
            
            if (CanTransitionToShop())
                StateMachine.ChangeState(new ShopState(StateMachine));
        }
        public override void Exit() { }
        public override void Tick() { }

        private bool CanTransitionToShop() => Status == RoundEndedStatus.Success && Game.Run.TryIncrementRound(); 
    }
}
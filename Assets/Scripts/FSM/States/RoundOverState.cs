using UnityEngine;

namespace FSM.States
{
    [System.Serializable]
    public class RoundOverState : GameState
    {
        public readonly bool WonRound; 
        
        public RoundOverState(GameStateMachine machine, bool wonRound) : base(machine)
        {
            WonRound = wonRound;
        }

        public override void Enter()
        {
            Debug.Log(WonRound ? "Round is won" : "Round is lost");
            
            if (CanTransitionToShop())
                StateMachine.ChangeState(new ShopState(StateMachine));
        }
        public override void Exit() { }
        public override void Tick() { }

        private bool CanTransitionToShop() => WonRound && Game.Run.TryIncrementRound(); 
    }
}
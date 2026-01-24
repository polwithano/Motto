using Managers;
using UnityEngine;

namespace FSM
{
    public class GameState : IGameState
    {
        protected GameManager Game => GameManager.Instance;
        protected readonly GameStateMachine StateMachine;

        protected GameState(GameStateMachine machine)
        {
            Debug.Log($"{GetType().Name} Constructor");
            StateMachine = machine;
        }

        public virtual void Enter() { }
        public virtual void Exit() { }
        public virtual void Tick() { }
    }
}
using Managers;
using UnityEngine;

namespace FSM
{
    [System.Serializable]
    public abstract class GameState : IGameState
    {
        protected static GameManager Game => GameManager.Instance;
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
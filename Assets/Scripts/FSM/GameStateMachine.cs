using UnityEngine;

namespace FSM
{
    [System.Serializable]
    public class GameStateMachine : MonoBehaviour
    {
        [field: SerializeField] public IGameState CurrentState { get; private set; }

        public void ChangeState(IGameState newState)
        {
            if (CurrentState == newState) return;

            CurrentState?.Exit();
            CurrentState = newState;
            CurrentState?.Enter();
        }

        private void Update()
        {
            CurrentState?.Tick();
        }
    }
}
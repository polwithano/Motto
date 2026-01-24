namespace FSM
{
    public interface IGameState
    { 
        void Enter();
        void Exit();
        void Tick();
    }
}
using Events;
using Managers;

namespace FSM
{
    public class ShopState : GameState
    {
        public ShopState(GameStateMachine machine) : base(machine) { }

        public override void Enter()
        {
            GameEvents.OnShopClosed += HandleOnShopClosed;
            GameEvents.OnShopReroll += HandleOnShopReroll; 
            
            ShopManager.Instance.InitializeShop();
            
            GameEvents.RaiseOnShopOpen();
        }
        
        public override void Exit()
        {
            GameEvents.OnShopClosed -= HandleOnShopClosed; 
            GameEvents.OnShopReroll -= HandleOnShopReroll;
        }

        public override void Tick()
        {
            
        }
        
        #region Event Handlers
        private void HandleOnShopClosed()
        {
            StateMachine.ChangeState(new RoundStartState(StateMachine));
        }

        private void HandleOnShopReroll()
        {
            ShopManager.Instance.RerollShop();
        }
        #endregion
    }
}
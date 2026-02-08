using Events;
using Events.Core;
using Events.Shop;
using Managers;

namespace FSM.States
{
    [System.Serializable]
    public class ShopState : GameState
    {
        public ShopState(GameStateMachine machine) : base(machine) { }

        public override void Enter()
        {
            ShopManager.Instance.InitializeShop();

            Bus<ShopStatusEvent>.OnEvent += HandleOnShopStatusUpdated; 
            Bus<ShopStatusEvent>.Raise(new ShopStatusEvent(ShopStatus.Open));
        }

        public override void Exit()
        {
            Bus<ShopStatusEvent>.OnEvent -= HandleOnShopStatusUpdated; 
        }

        public override void Tick()
        {
            
        }
        
        #region Event Handlers
        
        private void HandleOnShopStatusUpdated(ShopStatusEvent evt)
        {
            switch (evt.Status)
            {
                case ShopStatus.Open:
                    break; 
                case ShopStatus.Closed:
                    HandleOnShopClosed();
                    break;
                case ShopStatus.Reroll:
                    HandleOnShopReroll();
                    break;
            }
        }
        
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
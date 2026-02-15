using Events;
using Events.Core;
using Events.Game;
using Events.Shop;
using Events.UI;
using Interfaces;
using Managers;
using Models;
using Models.Charms;
using Models.Charms.Core;
using UnityEngine;

namespace FSM.States
{
    [System.Serializable]
    public class ShopState : GameState
    {
        public ShopState(GameStateMachine machine) : base(machine) { }

        public override void Enter()
        {
            ShopManager.Instance.InitializeShop();

            Bus<SetUIContainerStateEvent>.OnEvent += HandleOnShopStatusUpdated; 
            Bus<ShopRerollRequestEvent>.OnEvent += HandleOnShopRerollRequested; 
            Bus<PurchaseProcessedEvent>.OnEvent += HandleOnPurchaseProcessed; 
            
            Bus<SetUIContainerStateEvent>.Raise(new SetUIContainerStateEvent(
                UIType.Shop, 
                UIState.Opened));
        }

        public override void Exit()
        {
            Bus<SetUIContainerStateEvent>.OnEvent -= HandleOnShopStatusUpdated; 
            Bus<ShopRerollRequestEvent>.OnEvent -= HandleOnShopRerollRequested; 
            Bus<PurchaseProcessedEvent>.OnEvent -= HandleOnPurchaseProcessed; 
        }

        public override void Tick()
        {
            
        }
        
        #region Event Handlers
        private void HandleOnShopStatusUpdated(SetUIContainerStateEvent evt)
        {
            if (evt.Container != UIType.Shop) return; 
            if (evt.State != UIState.Closed) return;
            
            HandleOnShopClosed();
        }
        
        private void HandleOnShopClosed()
        {
            StateMachine.ChangeState(new RoundStartState(StateMachine));
        }
        
        private void HandleOnPurchaseProcessed(PurchaseProcessedEvent evt)
        {
            var buyable = evt.Buyable;
            
            switch (buyable)
            {
                case null:
                    return;
                case Charm charm:
                    AddNewCharm(charm);
                    break;
                case Tile tile:
                    AddNewTileToDeck(tile); 
                    break;
            }
            
            ShopManager.Instance.RemoveBundle(evt.ItemBundle);
            Bus<ShopInventoryUpdatedEvent>.Raise(new ShopInventoryUpdatedEvent());
        }
        
        private void HandleOnShopRerollRequested(ShopRerollRequestEvent args)
        {
            if (!GameManager.Instance.Run.TryPurchase(ShopManager.Instance.RerollPrice)) return;
            
            ShopManager.Instance.RerollShop();
            
            Bus<CurrencyUpdatedEvent>.Raise(new CurrencyUpdatedEvent(
                CurrencyType.Soft, 
                GameManager.Instance.Run.SoftCurrency));
            
            Bus<ShopInventoryUpdatedEvent>.Raise(new ShopInventoryUpdatedEvent());
        }
        #endregion

        private void AddNewTileToDeck(Tile tile)
        {
            Game.Deck.AddTileToDrawPile(tile);
        }

        private void AddNewCharm(Charm charm)
        {
            CharmManager.Instance.AddCharm(charm);
        }
    }
}
using Events;
using Events.Core;
using Events.Game;
using Events.Shop;
using Interfaces;
using Managers;
using Models;
using Models.Charms;
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

            Bus<ShopStateEvent>.OnEvent += HandleOnShopStatusUpdated;
            Bus<ShopRerollRequestEvent>.OnEvent += HandleOnShopRerollRequested; 
            Bus<PurchaseProcessedEvent>.OnEvent += HandleOnPurchaseProcessed; 
            
            Bus<ShopStateEvent>.Raise(new ShopStateEvent(Events.Shop.ShopState.Opened));
        }

        public override void Exit()
        {
            Bus<ShopStateEvent>.OnEvent -= HandleOnShopStatusUpdated; 
            Bus<ShopRerollRequestEvent>.OnEvent -= HandleOnShopRerollRequested; 
            Bus<PurchaseProcessedEvent>.OnEvent -= HandleOnPurchaseProcessed; 
        }

        public override void Tick()
        {
            
        }
        
        #region Event Handlers
        private void HandleOnShopStatusUpdated(ShopStateEvent evt)
        {
            switch (evt.State)
            {
                case Events.Shop.ShopState.Opened:
                    break; 
                case Events.Shop.ShopState.Closed:
                    HandleOnShopClosed();
                    break;
            }
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
            
            Bus<CurrencyUpdatedEvent>.Raise(new CurrencyUpdatedEvent(GameManager.Instance.Run.Currency));
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
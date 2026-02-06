using System;
using System.Collections.Generic;
using Models;
using Models.Charms;
using Views;

namespace Events
{
    public static class GameEvents
    {
        #region Shop Events
        public static event Action OnShopOpened; 
        public static event Action OnShopClosed;
        public static event Action OnShopReroll;
        public static event Action OnPurchaseProcessed; 
        public static void RaiseOnShopOpen() => OnShopOpened?.Invoke();
        public static void RaiseOnShopClosed() => OnShopClosed?.Invoke();
        public static void RaiseOnShopReroll() => OnShopReroll?.Invoke();
        public static void RaiseOnPurchaseProcessed() => OnPurchaseProcessed?.Invoke();
        #endregion
        
        #region Score Events
        public static event Action OnWordScored; 
        public static event Action<string, List<Tile>> OnScoringStarted;
        
        public static event Action<ScoreLogEntry> OnScoreStepStarted;
        public static event Action<ScoreLogEntry, Action> OnScoreStepApplied;
        public static event Action<ScoreLog> OnScoreSequenceCompleted;
        
        public static void RaiseOnWordScored() =>  OnWordScored?.Invoke();
        public static void RaiseOnScoringStarted(string word, List<Tile> tiles)
            => OnScoringStarted?.Invoke(word, tiles);
        public static void RaiseOnScoreStepStarted(ScoreLogEntry entry)
            => OnScoreStepStarted?.Invoke(entry);
        public static void RaiseOnScoreStepApplied(ScoreLogEntry entry, Action action)
            => OnScoreStepApplied?.Invoke(entry, action);
        public static void RaiseOnScoreSequenceCompleted(ScoreLog entry)
            => OnScoreSequenceCompleted?.Invoke(entry);
        #endregion
        
        #region Game Events
        public static event Action<Tile, Tile> OnTileRedrawPerformed;
        public static event Action<TileView, SlotView> OnTileDropConfirmed; 
        
        public static void RaiseOnTileRedrawPerformed(Tile tile, Tile newTile) 
            => OnTileRedrawPerformed?.Invoke(tile, newTile);

        public static void RaiseOnTileDropConfirmed(TileView tile, SlotView slot)
            => OnTileDropConfirmed?.Invoke(tile, slot); 
        #endregion
    }
}
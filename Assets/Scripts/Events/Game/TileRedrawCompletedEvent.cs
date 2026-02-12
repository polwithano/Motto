using Events.Core;
using Models;

namespace Events.Game
{
    public struct TileRedrawCompletedEvent : IEvent
    {
        public Tile PreviousTile { get; private set; }
        public Tile NewTile { get; private set; }
        public int RemainingDrawsCount { get; private set; }
        public int RemainingCardsInDeck { get; private set; }

        public TileRedrawCompletedEvent(
            Tile previousTile, 
            Tile newTile, 
            int remainingDrawsCount, 
            int remainingCardsInDeck)
        {
            PreviousTile = previousTile;
            NewTile = newTile;
            RemainingDrawsCount = remainingDrawsCount;
            RemainingCardsInDeck = remainingCardsInDeck;
        }
    }
}
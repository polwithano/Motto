using Events.Core;
using Views;

namespace Events.Game
{
    public enum TilePosition
    {
        Hand, 
        Board
    }
    
    public struct TilePositionUpdatedEvent : IEvent
    {
        public TilePosition Position { get; private set; }
        public TileView View { get; private set; }

        public TilePositionUpdatedEvent(TilePosition position, TileView view)
        {
            Position = position;
            View = view;
        }
    }
}
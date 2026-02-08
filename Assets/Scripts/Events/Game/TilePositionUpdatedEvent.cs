using Events.Core;
using Views;

namespace Events.Game
{
    public enum GamePosition
    {
        Hand, 
        Board, 
        Redraw
    }
    
    public struct TilePositionUpdatedEvent : IEvent
    {
        public GamePosition Position { get; private set; }
        public TileView View { get; private set; }

        public TilePositionUpdatedEvent(GamePosition position, TileView view)
        {
            Position = position;
            View = view;
        }
    }
}
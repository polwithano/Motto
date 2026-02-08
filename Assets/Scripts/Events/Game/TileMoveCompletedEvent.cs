using Events.Core;
using Views;

namespace Events.Game
{
    public struct TileMoveCompletedEvent : IEvent
    {
        public TileView View { get; private set; }
        
        public TileMoveCompletedEvent(TileView view)
        {
            View = view;
        }
    }
}
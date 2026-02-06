using Events.Core;
using Models;
using Views;

namespace Events.Game
{
    public struct TileRedrawEvent : IEvent
    {
        public TileView View { get; private set; }
        public Tile Model {get; private set;}

        public TileRedrawEvent(TileView view, Tile model)
        {
            View = view;
            Model = model;
        }
    }
}
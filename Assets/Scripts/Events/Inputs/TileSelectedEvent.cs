using Events.Core;
using Models;
using Views;

namespace Events.Inputs
{
    public struct TileSelectedEvent : IEvent
    {
        public TileView View { get; private set; }
        public Tile Model { get; private set; }

        public TileSelectedEvent(TileView view, Tile model)
        {
            View = view;
            Model = model;
        }
    }
}
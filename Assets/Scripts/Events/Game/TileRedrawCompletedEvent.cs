using Events.Core;
using Models;

namespace Events.Game
{
    public struct TileRedrawCompletedEvent : IEvent
    {
        public Tile PreviousTile { get; private set; }
        public Tile NewTile { get; private set; }

        public TileRedrawCompletedEvent(Tile previousTile, Tile newTile)
        {
            PreviousTile = previousTile;
            NewTile = newTile;
        }
    }
}
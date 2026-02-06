using Events.Core;
using Views;

namespace Events.Game
{
    public struct TileMoveRequestEvent : IEvent
    {
        public TileView Tile { get; private set; }
        public GamePosition TargetPosition { get; private set; }
        public SlotView TargetSlot { get; private set; } // null => auto

        public TileMoveRequestEvent(TileView tile, GamePosition targetPosition, SlotView targetSlot)
        {
            Tile = tile;
            TargetPosition = targetPosition;
            TargetSlot = targetSlot;
        }
    }
}
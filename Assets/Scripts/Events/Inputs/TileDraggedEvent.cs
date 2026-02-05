using Events.Core;
using Models;
using Views;

namespace Events.Inputs
{
    public enum DragEventType
    {
        DragStart, 
        DragEnd
    }
    
    public struct TileDraggedEvent : IEvent
    {
        public DragEventType EventType { get; private set; }
        public TileView DraggedTile    { get; private set; }
        public Tile Model              { get; private set; }

        public TileDraggedEvent(TileView view, DragEventType eventType)
        {
            DraggedTile = view;
            Model = view.Tile; 
            EventType = eventType;
        }
    }
}
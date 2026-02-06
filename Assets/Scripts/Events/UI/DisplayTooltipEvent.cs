using Events.Core;

namespace Events.UI
{
    public struct DisplayTooltipEvent<T> : IEvent
    {
        public T ModelToDisplay { get; private set; }
        
        public DisplayTooltipEvent(T model)
        {
            ModelToDisplay = model;
        }
    }
}
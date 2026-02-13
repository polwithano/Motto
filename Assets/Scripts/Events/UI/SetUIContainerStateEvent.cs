using Events.Core;

namespace Events.UI
{
    public struct SetUIContainerStateEvent : IEvent
    {
        public UIType Container { get; private set; }
        public UIState State    { get; private set; }

        public SetUIContainerStateEvent(UIType container, UIState state)
        {
            Container = container;
            State = state;
        }
    }
}
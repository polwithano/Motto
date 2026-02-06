using Events.Core;

namespace Events.Score
{
    public enum WordValidationStatus
    {
        Validated, 
        Invalidated
    }
    
    public struct WordValidationEvent : IEvent
    {
        public WordValidationStatus Status { get; private set; }
        public string Word { get; private set; }

        public WordValidationEvent(WordValidationStatus status, string word)
        {
            Status = status;
            Word = word;
        }
    }
}
using System.Collections.Generic;
using Events.Core;
using Models;

namespace Events.Score
{
    public struct ScoringSequenceStartedEvent : IEvent
    {
        public string Word {get; private set;}
        public List<Tile> Tiles {get; private set;}

        public ScoringSequenceStartedEvent(string word, List<Tile> tiles)
        {
            Word = word;
            Tiles = tiles;
        }
    }
}
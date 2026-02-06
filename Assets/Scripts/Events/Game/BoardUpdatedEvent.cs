using System.Collections.Generic;
using Events.Core;
using Models;

namespace Events.Game
{
    public struct BoardUpdatedEvent : IEvent
    {
        public string Word { get; private set; }
        public List<Tile> Tiles { get; private set; }

        public BoardUpdatedEvent(string word, List<Tile> tiles)
        {
            Word = word;
            Tiles = tiles;
        }
    }
}
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Models
{
    [System.Serializable]
    public class TileHand
    {
        [field: SerializeField] public int MaxHandSize    { get; private set; }
        [field: SerializeField] public List<Tile> Tiles   { get; private set; } = new();

        public TileHand(int maxHandSize = 7)
        {
            MaxHandSize = maxHandSize;
        }

        public void FillFromDeck(TileDeck deck)
        {
            while (Tiles.Count < MaxHandSize)
            {
                var tile = deck.Draw(1)[0]; 
                Tiles.Add(tile);
            }
        }

        public void TryAddTile(Tile tile)
        {
            if (Tiles.Contains(tile)) return;
            if (Tiles.Count >= MaxHandSize) return;
            
            Tiles.Add(tile);
        }

        public void RemoveTile(Tile tile)
        {
            Tiles.RemoveAll(t => t.ID == tile.ID);
        }
        
        public void RemoveTiles(IEnumerable<Tile> tilesToRemove)
        {
            foreach (var tile in tilesToRemove)
                Tiles.Remove(tile);
        }
    }
}
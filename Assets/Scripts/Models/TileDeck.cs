using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

namespace Models
{
    [Serializable]
    public class TileDeck
    {
        private const uint MAX_TILES_PER_DECK = 256; 
        
        [field: SerializeField] public List<Tile> DrawPile      { get; private set; } = new();
        [field: SerializeField] public List<Tile> DiscardPile   { get; private set; } = new();

        private TileDistributionRuleSO _distributionRule; 
        
        public TileDeck(TileDistributionRuleSO distributionRule)
        {
            _distributionRule = distributionRule;
            InitializeDeck();
        }
        
        private void InitializeDeck()
        {
            DrawPile.Clear();
            DiscardPile.Clear();

            foreach (var rule in _distributionRule.CharacterRules)
            {
                for (var i = 0; i < rule.countPerCharacter; i++)
                {
                    var tile = new Tile(
                        rule.character, 
                        rule.pointValue, 
                        rule.isBlank, 
                        _distributionRule.DefaultTileModifier);
                    
                    DrawPile.Add(tile);
                }
            }
            
            Debug.Log($"[TileDeck] Initialized deck with {DrawPile.Count} tiles.");
            Shuffle();
        }

        public void Shuffle()
        {
            for (var i = 0; i < DrawPile.Count; i++)
            {
                var rand = Random.Range(i, DrawPile.Count);
                (DrawPile[i], DrawPile[rand]) = (DrawPile[rand], DrawPile[i]);
            }
        }

        public bool TryDraw(out Tile tile)
        {
            tile = null;

            if (DrawPile.Count == 0)
            {
                RecycleDiscardPile();

                if (DrawPile.Count == 0)
                    return false;
            }

            tile = DrawPile[0];
            DrawPile.RemoveAt(0);
            return true;
        }
        
        public void RecycleDiscardPile()
        {
            if (DiscardPile.Count == 0)
            {
                Debug.LogWarning("[TileDeck] Attempted recycle but discard pile was empty.");
                return;
            }
            
            DrawPile.AddRange(DiscardPile);
            DiscardPile.Clear();
            Shuffle();

            Debug.Log("[TileDeck] Recycled discard pile back into deck.");
        }
        
        public void Discard(Tile tile)
        {
            DiscardPile.Add(tile);
        }

        public void DiscardRange(IEnumerable<Tile> tiles)
        {
            DiscardPile.AddRange(tiles);
        }

        public void AddTileToDrawPile(Tile tile)
        {
            if (TilesCount() >= MAX_TILES_PER_DECK) return;
            
            DrawPile.Add(tile);
            Shuffle();
            
            Debug.Log($"Tile {tile.Character} added to deck");
        }
        
        private int TilesCount() => DrawPile.Count + DiscardPile.Count;

    }
}
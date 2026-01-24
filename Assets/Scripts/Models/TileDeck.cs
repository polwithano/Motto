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
            for (int i = 0; i < DrawPile.Count; i++)
            {
                int rand = Random.Range(i, DrawPile.Count);
                (DrawPile[i], DrawPile[rand]) = (DrawPile[rand], DrawPile[i]);
            }
        }

        public List<Tile> Draw(int count)
        {
            var drawn = new List<Tile>();

            for (var i = 0; i < count; i++)
            {
                if (DrawPile.Count == 0) RecycleDiscardPile();
                
                drawn.Add(DrawPile[0]);
                DrawPile.RemoveAt(0);
            }

            return drawn; 
        }
        
        public void RecycleDiscardPile()
        {
            if (DiscardPile.Count == 0) return;

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

    }
}
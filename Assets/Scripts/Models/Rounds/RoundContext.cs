using System.Collections.Generic;
using UnityEngine;

namespace Models.Rounds
{
    [System.Serializable]
    public class RoundContext
    {
        [field: SerializeField] public RoundDefinition Definition  { get; private set; }
        [field: SerializeField] public int TargetScore             { get; private set; }
        [field: SerializeField] public int CurrentScore            { get; private set; }
        [field: SerializeField] public int SoftCurrencyReward      { get; private set; }
        [field: SerializeField] public int WordsRemaining          { get; private set; }
        [field: SerializeField] public int DrawsRemaining          { get; private set; }
        [field: SerializeField] public List<string> Words          { get; private set; }
        [field: SerializeField] public List<int> ScoreIncrements   { get; private set; }
        
        public List<List<Tile>> Tiles { get; private set; }

        public RoundContext(RoundDefinition definition)
        {
            Definition = definition;
            TargetScore = definition.BaseTargetScore;
            SoftCurrencyReward = definition.BaseSoftCurrencyReward;

            CurrentScore = 0;
            WordsRemaining = 5;
            DrawsRemaining = 20; 
            Words = new List<string>();
            ScoreIncrements = new List<int>();
            Tiles = new List<List<Tile>>();
        }

        public void AddWord(string word, List<Tile> tiles)
        {
            Words.Add(word); 
            Tiles.Add(tiles);
        }
        
        public void AddScore(int value)
        {
            ScoreIncrements.Add(value);
            CurrentScore += value;
        }

        public bool AllowDraw() => DrawsRemaining > 0;
        public void RemoveAttempt() => WordsRemaining--;
        public void RemoveDraw() => DrawsRemaining--;
        public string LastlyAddedWord() => Words[^1]; 
        public List<Tile> LastlyAddedTiles() => Tiles.Count > 0 ? Tiles[^1] : null;
        public int GetLastScore() => ScoreIncrements[^1];
        public float CompletionPercentage => (float)CurrentScore / TargetScore;
        public bool IsCompleted => CurrentScore >= TargetScore; 
    }
}
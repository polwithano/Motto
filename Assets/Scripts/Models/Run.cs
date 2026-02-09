using System.Collections.Generic;
using UnityEngine;

namespace Models
{
    [System.Serializable]
    public class Run
    {
        [field: SerializeField] public int RoundIndex             { get; private set; }
        [field: SerializeField] public RoundContext Round         { get; private set; }
        [field: SerializeField] public List<RoundContext> Rounds  { get; private set; }
        
        [field: SerializeField] public uint Currency               { get; private set; }

        private RunDataSO _data;

        public Run(RunDataSO data)
        {
            _data = data;
            RoundIndex = 0;
            Rounds = new List<RoundContext>(); 
            Currency = 0;
        }

        public bool TryIncrementRound()
        {
            if (RoundIndex >= _data.RoundsSequence.Length - 1)
                return false;

            RoundIndex += 1;
            return true;
        }

        public void LoadContext()
        {
            if (Round != null) Rounds.Add(Round);
            Round = CreateContext();
        }

        public bool TryPurchase(uint cost)
        {
            var newCount = (Currency - (int)cost);
            if (newCount < 0) return false;
            Currency = (uint)newCount;
            return true;
        }

        public void IncreaseCurrency(uint amount)
        {
            Currency += amount;
        }
        
        private RoundContext CreateContext()
        {
            var def = _data.RoundsSequence[RoundIndex];
            return new RoundContext(def);
        }
    }
}

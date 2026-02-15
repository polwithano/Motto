using System.Collections.Generic;
using Models.Rounds;
using Models.SO;
using UnityEngine;

namespace Models
{
    [System.Serializable]
    public class Run
    {
        [field: SerializeField] public int RoundIndex             { get; private set; }
        [field: SerializeField] public RoundContext Round         { get; private set; }
        [field: SerializeField] public List<RoundContext> Rounds  { get; private set; }
        
        [field: SerializeField] public uint SoftCurrency { get; private set; }
        [field: SerializeField] public uint HardCurrency { get; private set; }

        private RunDataSO _data;

        public Run(RunDataSO data)
        {
            _data = data;
            RoundIndex = 0;
            Rounds = new List<RoundContext>();
            
            SetCurrencyValue(0, CurrencyType.Soft);
            SetCurrencyValue(0, CurrencyType.Hard);
        }

        public bool TryIncrementRound()
        {
            if (RoundIndex >= _data.RoundsSequence.Length - 1)
                return false;

            RoundIndex += 1;
            return true;
        }
        
        public RoundType GetRoundType() => _data.RoundsSequence[RoundIndex].RoundType;

        public void LoadContext()
        {
            if (Round != null) Rounds.Add(Round);
            Round = CreateContext();
        }

        public bool TryPurchase(uint cost)
        {
            var newCount = (SoftCurrency - (int)cost);
            if (newCount < 0) return false;
            SoftCurrency = (uint)newCount;
            return true;
        }

        public void SetCurrencyValue(uint amount, CurrencyType type)
        {
            switch (type)
            {
                case CurrencyType.Soft:
                    SoftCurrency += amount;
                    break; 
                case CurrencyType.Hard:
                    HardCurrency += amount;
                    break;
            }
        }
        
        private RoundContext CreateContext()
        {
            var def = _data.RoundsSequence[RoundIndex];
            return new RoundContext(def);
        }
    }
}

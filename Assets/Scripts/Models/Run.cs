using System.Collections.Generic;
using Models.Rewards;
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
        [field: SerializeField] public RoundRewardResult CurrentRoundResult { get; private set; }

        private RunDataSO _data;

        public Run(RunDataSO data)
        {
            _data = data;
            RoundIndex = 0;
            Rounds = new List<RoundContext>();
            
            UpdateCurrencyValue(0, CurrencyType.Soft);
            UpdateCurrencyValue(0, CurrencyType.Hard);
        }

        public bool TryIncrementRound()
        {
            if (RoundIndex >= _data.RoundsSequence.Length - 1)
                return false;

            RoundIndex += 1;
            return true;
        }
        
        public RoundContext GetCurrentRoundContext() => Rounds[^1];
        public RoundType GetRoundType() => Rounds[RoundIndex].Definition.RoundType;

        public void LoadContext()
        {
            Round = CreateContext();
            if (Round != null) Rounds.Add(Round);
        }

        public bool TryPurchase(uint cost)
        {
            var newCount = (SoftCurrency - (int)cost);
            if (newCount < 0) return false;
            SoftCurrency = (uint)newCount;
            return true;
        }

        public void UpdateCurrencyValue(uint amount, CurrencyType type)
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
        
        public void SetRewardsResult(RoundRewardResult result) => CurrentRoundResult = result;
        
        private RoundContext CreateContext()
        {
            var def = _data.RoundsSequence[RoundIndex];
            return new RoundContext(def);
        }
    }
}

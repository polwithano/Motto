using System;
using System.Collections.Generic;
using Interfaces;
using UnityEngine;

namespace Models.Charms
{
    public enum EffectTarget { Score, Multiplier, GameState }
    public enum EffectTrigger {OnWordStart, OnWordEnd, OnLetter}
    
    public abstract class Charm : ScriptableObject, IEffectEmitter, IBuyable
    {
        [field: SerializeField] public string ID                             { get; set; }
        [field: SerializeField] public string CharmName                      { get; private set; } 
        [field: SerializeField] public string CharmDescription               { get; private set; } 
        [field: SerializeField] public Sprite CharmIcon                      { get; private set; }
        [field: SerializeField] public int PriorityOverride                  { get; private set; }
        [field: SerializeField] public EffectTrigger EffectTrigger           { get; private set; }
        [field: SerializeField] public List<ScoreEffect> ScoreEffects        { get; private set; } 

        #region Interface
        public bool IsInstance(string id) => ID == id;
        #endregion

        private void OnValidate()
        {
            if (string.IsNullOrEmpty(ID)) ID = Guid.NewGuid().ToString();
            ScoreEffects ??= new List<ScoreEffect>();
        }
        
        public abstract bool TryApplyEffect(RoundContext context, int? index = null);
        
        public virtual void ApplyEffect(ScoreLog log)
        {
            if (ScoreEffects != null && ScoreEffects.Count > 0)
            {
                foreach (var effect in ScoreEffects)
                {
                    var entry = new ScoreLogEntry(log.Logs.Count, this, effect);
                    log.AddEntry(entry);
                }
            }
        }

        public void ProcessPurchase()
        {
            throw new NotImplementedException();
        }
    }
}

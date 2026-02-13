#nullable enable
using System;
using System.Collections.Generic;
using Interfaces;
using Models.Rounds;
using UnityEngine;

namespace Models.Charms.Core
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
        [field: SerializeField] public uint DefaultValue { get; set; } = 1;
        public void SetPrice(uint price) => DefaultValue = price;
        #endregion

        private void OnValidate()
        {
            if (string.IsNullOrEmpty(ID)) ID = Guid.NewGuid().ToString();
        }
        
        public abstract bool TryApplyEffect(RoundContext context, int? index = null);
        public abstract bool WillPreviewEffect(string word, List<Tile> tiles = null, int? index = null);
        
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
    }
}

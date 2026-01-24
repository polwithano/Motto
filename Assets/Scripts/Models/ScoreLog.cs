using System;
using System.Collections.Generic;
using Interfaces;
using UnityEngine;

namespace Models
{
    [Serializable]
    public class ScoreLog
    {
        [field: SerializeField] public List<ScoreLogEntry> Logs     { get; private set; }
        [field: SerializeField] public string Word                  { get; private set; }

        public readonly List<Tile> Tiles;

        public int Score;
        public float Modifier;
        public float Result; 
        
        public ScoreLog(string word, List<Tile> tiles)
        {
            Word = word;
            Tiles = tiles;
            
            Logs = new List<ScoreLogEntry>();
        }
        
        public void AddEntry(ScoreLogEntry entry)
        {
            if (Logs.Count > 0)
            {
                var previous =  Logs[^1];
                entry.EntryScore = previous.EntryScore;
                entry.EntryModifier = previous.EntryModifier; 
            }
            entry.ComputeCurrentScoreAndModifier();
            Logs.Add(entry);
        }
    }

    [Serializable]
    public class ScoreLogEntry
    {
        public readonly int ID;
        public readonly IEffectEmitter Emitter;
        public readonly List<ScoreEffect> ScoreEffects;

        public int EntryScore = 0;
        public float EntryModifier = 0; 
        
        public ScoreLogEntry(int id, IEffectEmitter emitter, List<ScoreEffect> scoreEffects)
        {
            ID = id;
            Emitter = emitter;
            ScoreEffects = scoreEffects;
        }

        public ScoreLogEntry(int id, IEffectEmitter emitter, ScoreEffect scoreEffect)
        {
            ID = id;
            Emitter = emitter;
            ScoreEffects = new List<ScoreEffect> { scoreEffect };
        }

        public void ComputeCurrentScoreAndModifier()
        {
            foreach (var effect in ScoreEffects)
            {
                if (effect.Target == ScoreEffectTarget.Score)
                {
                    EntryScore += (int)effect.Value; 
                }
                if (effect.Target == ScoreEffectTarget.Modifier)
                {
                    EntryModifier += effect.Value;
                }
            }
        }
    }
    
    public enum ScoreEffectTarget {Score, Modifier}

    [Serializable]
    public class ScoreEffect
    {
        [field: SerializeField] public ScoreEffectTarget Target { get; private set; }
        [field: SerializeField] public float Value              { get; private set; }

        public ScoreEffect(ScoreEffectTarget target, float value)
        {
            Target = target;
            Value = value;
        }
    }
}
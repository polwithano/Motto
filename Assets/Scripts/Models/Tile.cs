using System;
using System.Collections.Generic;
using Interfaces;
using UnityEngine;

namespace Models
{
    [Serializable]
    public class Tile : IEffectEmitter, IBuyable
    {
        [field: SerializeField] public string ID           { get; set; }
        [field: SerializeField] public string Character    { get; private set; }
        [field: SerializeField] public int Points          { get; private set; }
        [field: SerializeField] public bool IsBlank        { get; private set; }
        
        [field: SerializeField] public TileModifierSO Modifier { get; private set; } 
        
        #region IEffectEmitter
        public bool IsInstance(string id) => ID == id;
        #endregion
        
        #region IBuyable
        public void ProcessPurchase()
        {
            throw new NotImplementedException();
        }
        #endregion
        
        public Tile(string character, int points, bool isBlank, TileModifierSO modifier = null)
        {
            ID = Guid.NewGuid().ToString();
            Character = character;
            Points = points;
            IsBlank = isBlank;
            Modifier = modifier;
        }
    }
}
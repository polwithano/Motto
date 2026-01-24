using System;
using TMPro;
using UnityEngine;

namespace Models
{
    [CreateAssetMenu(menuName = "Models/Tile Modifier", fileName = "TileDistribution")]
    public class TileModifierSO : ScriptableObject
    {
        [field: SerializeField] public string ID               { get; set; }
        
        [field: SerializeField] public TMP_FontAsset FontAsset { get; private set; }
        [field: SerializeField] public string Name             { get; private set; }
        [field: SerializeField] public float PriceModifier     { get; private set; } = 1f; 

        public void OnValidate()
        {
            if (string.IsNullOrEmpty(ID)) ID = Guid.NewGuid().ToString();
        }
    }
}
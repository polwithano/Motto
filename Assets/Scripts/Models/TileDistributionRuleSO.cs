using System.Collections.Generic;
using System.Linq;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Models
{
    [CreateAssetMenu(menuName = "Models/Tile Distribution", fileName = "TileDistribution")]
    public class TileDistributionRuleSO : ScriptableObject
    {
        [System.Serializable]
        public class CharacterRule
        {
            public string character;
            public bool isBlank;
            public int pointValue;
            public int countPerCharacter;
            public uint defaultValue; 
        }

        [Header("Letter Distribution (auto-generated A–Z + blank)")]
        [field: SerializeField] public List<CharacterRule> CharacterRules { get; private set; } = new();
        [Header("Tile Properties")]
        [field: SerializeField] public TileModifierSO DefaultTileModifier { get; private set; }

        [Header("Debug / Info")]
        [SerializeField, ReadOnlyInspector] private int totalTileCount; // read-only field (custom drawer below)

        /// <summary>
        /// Calculates the total number of tiles represented by all CharacterRules.
        /// </summary>
        public int CalculateTotalTiles()
        {
            totalTileCount = CharacterRules.Sum(r => Mathf.Max(r.countPerCharacter, 0));
            return totalTileCount;
        }

#if UNITY_EDITOR
        // Called automatically when the asset is first created
        private void OnEnable()
        {
            if (CharacterRules == null || CharacterRules.Count == 0)
                GenerateDefaultEntries();

            CalculateTotalTiles();
        }

        // Automatically refresh tile count and maintain correct ordering when edited
        private void OnValidate()
        {
            CalculateTotalTiles();

            // Keep sorted alphabetically, blank always last
            CharacterRules = CharacterRules
                .OrderBy(r => r.isBlank ? 999 : r.character[0])
                .ToList();
        }

        [ContextMenu("Regenerate A–Z + Blank")]
        private void GenerateDefaultEntries()
        {
            CharacterRules.Clear();

            // Add A–Z
            for (char c = 'A'; c <= 'Z'; c++)
            {
                CharacterRules.Add(new CharacterRule
                {
                    character = c.ToString(),
                    pointValue = 1,
                    countPerCharacter = 1,
                    isBlank = false,
                    defaultValue = 1
                });
            }

            // Add one blank tile
            CharacterRules.Add(new CharacterRule
            {
                character = "_",
                pointValue = 0,
                countPerCharacter = 2,
                isBlank = true,
                defaultValue = 1
            });

            Debug.Log("[TileDistributionSO] Generated default A–Z + blank entries.");
            EditorUtility.SetDirty(this);
        }
#endif
    }

    // Simple attribute & drawer to make total read-only
#if UNITY_EDITOR
    public class ReadOnlyInspectorAttribute : PropertyAttribute { }

    [CustomPropertyDrawer(typeof(ReadOnlyInspectorAttribute))]
    public class ReadOnlyInspectorDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            GUI.enabled = false;
            EditorGUI.PropertyField(position, property, label, true);
            GUI.enabled = true;
        }
    }
#endif
}

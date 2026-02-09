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
            public char character;
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
        [SerializeField, ReadOnlyInspector] private int totalTileCount;

        [SerializeField, ReadOnlyInspector] private int totalVowelCount;
        [SerializeField, ReadOnlyInspector] private int totalConsonantCount;

        [SerializeField, ReadOnlyInspector] private int totalVowelPoints;
        [SerializeField, ReadOnlyInspector] private int totalConsonantPoints;

        [SerializeField, ReadOnlyInspector] private int totalPoints;

        private static readonly HashSet<char> Vowels = new()
        {
            'A','E','I','O','U'
        };

        private void CalculateDebugStats()
        {
            totalTileCount = 0;
            totalVowelCount = 0;
            totalConsonantCount = 0;

            totalVowelPoints = 0;
            totalConsonantPoints = 0;
            totalPoints = 0;

            foreach (var rule in CharacterRules)
            {
                int count = Mathf.Max(rule.countPerCharacter, 0);
                int points = rule.pointValue * count;

                totalTileCount += count;
                totalPoints += points;

                if (rule.isBlank)
                    continue;

                if (Vowels.Contains(char.ToUpper(rule.character)))
                {
                    totalVowelCount += count;
                    totalVowelPoints += points;
                }
                else
                {
                    totalConsonantCount += count;
                    totalConsonantPoints += points;
                }
            }
        }

#if UNITY_EDITOR
        // Called automatically when the asset is first created
        private void OnEnable()
        {
            if (CharacterRules == null || CharacterRules.Count == 0)
                GenerateDefaultEntries();

            CalculateDebugStats();
        }

        // Automatically refresh tile count and maintain correct ordering when edited
        private void OnValidate()
        {
            CalculateDebugStats();

            // Keep sorted alphabetically, blank always last
            CharacterRules = CharacterRules
                .OrderBy(r => r.isBlank ? 999 : r.character)
                .ToList();
        }

        [ContextMenu("Regenerate A–Z + Blank")]
        private void GenerateDefaultEntries()
        {
            CharacterRules.Clear();

            // Add A–Z
            for (var c = 'A'; c <= 'Z'; c++)
            {
                CharacterRules.Add(new CharacterRule
                {
                    character = c,
                    pointValue = 1,
                    countPerCharacter = 1,
                    isBlank = false,
                    defaultValue = 1
                });
            }

            // Add one blank tile
            CharacterRules.Add(new CharacterRule
            {
                character = '_',
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
    
#if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(TileDistributionRuleSO.CharacterRule))]
    public class CharacterRuleDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var characterProp = property.FindPropertyRelative("character");
            var countProp = property.FindPropertyRelative("countPerCharacter");
            var isBlankProp = property.FindPropertyRelative("isBlank");
            var pointValueProp = property.FindPropertyRelative("pointValue");

            var c = (char)characterProp.intValue;

            var title = isBlankProp.boolValue
                ? $"_ (blank x{countProp.intValue}, {pointValueProp.intValue}pts)"
                : $"{c} (x{countProp.intValue}, {pointValueProp.intValue}pts)";

            EditorGUI.PropertyField(position, property, new GUIContent(title), true);
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUI.GetPropertyHeight(property, true);
        }
    }
#endif

}

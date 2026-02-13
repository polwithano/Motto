using System;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Models;
using Models.Charms;
using Models.Charms.Core;
using UnityEngine;

namespace UI.Helpers
{
    /// <summary>
    /// Utility class responsible for parsing and formatting charm description strings.
    /// 
    /// This parser allows charm descriptions (defined in ScriptableObjects) to contain
    /// dynamic tokens that are replaced at runtime using the charm's actual data
    /// (ScoreEffects). The output string is fully compatible with TextMeshPro
    /// (including color tags).
    ///
    /// ------------------------------------------------------------
    /// TOKEN SYNTAX
    /// ------------------------------------------------------------
    ///
    /// Tokens are defined using curly braces:
    ///
    ///     {s}    → Score value
    ///     {m}    → Modifier value
    ///
    /// Tokens can be extended with formatting options using ':' and '|':
    ///
    ///     {type:option1|option2|...}
    ///
    /// Where:
    ///     type    → 's' (score) or 'm' (modifier)
    ///     options → One or more formatting modifiers
    ///
    /// ------------------------------------------------------------
    /// AVAILABLE OPTIONS
    /// ------------------------------------------------------------
    ///
    /// +       : Displays a sign (+ / -) depending on the value.
    /// x       : Prefixes the value with 'x' (useful for multipliers).
    /// label   : Appends a textual label ("score" or "modifier").
    /// abs     : Displays the absolute value (removes sign).
    /// int     : Displays the value as an integer (rounded).
    ///
    /// Options can be combined using '|'.
    /// </summary>
    public static class CharmDescriptionParser
    {
        private static readonly Regex TokenRegex =
            new(@"\{(?<type>[sm])(?::(?<options>[^}]+))?\}");

        public static string Parse(string raw, Charm model)
        {
            if (string.IsNullOrEmpty(raw))
                return string.Empty;

            return TokenRegex.Replace(raw, match =>
            {
                var type = match.Groups["type"].Value;
                var options = match.Groups["options"].Value;

                return type switch
                {
                    "s" => BuildValue(
                        GetScoreValue(model),
                        CharmTextColors.Score,
                        "score",
                        options
                    ),
                    "m" => BuildValue(
                        GetModifierValue(model),
                        CharmTextColors.Modifier,
                        "modifier",
                        options
                    ),
                    _ => match.Value
                };
            });
        }
        
        private static string BuildValue(
            float value,
            string color,
            string label,
            string options)
        {
            var opts = string.IsNullOrEmpty(options)
                ? Array.Empty<string>()
                : options.Split('|');

            var showSign = opts.Contains("+");
            var showX = opts.Contains("x");
            var showLabel = opts.Contains("label");
            var abs = opts.Contains("abs");
            var asInt = opts.Contains("int");

            var v = abs ? Mathf.Abs(value) : value;

            var number = asInt
                ? ((int)v).ToString()
                : v.ToString(CultureInfo.InvariantCulture);

            var sign = showSign
                ? v > 0 ? "+" : v < 0 ? "-" : string.Empty
                : string.Empty;

            var prefix = showX ? "x" : string.Empty;
            var suffix = showLabel ? $" {label}" : string.Empty;

            return Colorize($"{sign}{prefix}{number}{suffix}", color);
        }
        
        private static float GetScoreValue(Charm charm)
        {
            return charm.ScoreEffects
                .Where(e => e.Target == ScoreEffectTarget.Score)
                .Sum(e => e.Value);
        }

        private static float GetModifierValue(Charm charm)
        {
            return charm.ScoreEffects
                .Where(e => e.Target == ScoreEffectTarget.Modifier)
                .Sum(e => e.Value);
        }
        
        private static string Colorize(string value, string hexColor)
        {
            return $"<color={hexColor}>{value}</color>";
        }
    }
}
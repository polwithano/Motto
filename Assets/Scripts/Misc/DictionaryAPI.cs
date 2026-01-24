using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Managers;
using UnityEngine;
using UnityEngine.Networking;

namespace Misc
{
    public static class DictionaryAPI
    {
        public static async Task<bool> CheckWordAsync(this string word)
        {
            if (string.IsNullOrEmpty(word) || word.Length == 1)
                return false;

            var country = GameManager.Instance.LanguageCode; 
            var url = $"https://api.dictionaryapi.dev/api/v2/entries/{country}/{word.ToLower()}";

            using (var www = UnityWebRequest.Get(url))
            {
                var op = www.SendWebRequest();
                while (!op.isDone)
                    await Task.Yield();

                if (www.result == UnityWebRequest.Result.ConnectionError ||
                    www.result == UnityWebRequest.Result.ProtocolError)
                {
                    return !www.downloadHandler.text.Contains("No Definitions Found");
                }

                return true;
            }
        }

        public static async Task<bool> CheckWordWithBlanksAsync(this string pattern)
        {
            if (!pattern.Contains("_"))
                return await pattern.CheckWordAsync();

            var blanks = pattern.Count(c => c == '_');
            if (blanks > 2)
            {
                Debug.LogWarning("Too many blanks, skipping exhaustive check.");
                return false;
            }

            var letters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ".ToCharArray();

            foreach (var combo in GenerateCombinations(pattern, letters))
            {
                if (await combo.CheckWordAsync())
                    return true;
            }

            return false;
        }

        private static IEnumerable<string> GenerateCombinations(string pattern, char[] letters)
        {
            var index = pattern.IndexOf('_');
            if (index == -1)
            {
                yield return pattern;
                yield break;
            }

            foreach (var letter in letters)
            {
                string newWord = pattern.Remove(index, 1).Insert(index, letter.ToString());
                foreach (var next in GenerateCombinations(newWord, letters))
                    yield return next;
            }
        }
    }
}

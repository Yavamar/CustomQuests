﻿using System.Collections.Generic;
using System.Linq;

namespace CustomQuest
{
    public static class DictionaryExt
    {
        public static IEnumerable<T> PartialMatch<T>(this Dictionary<string, T> dictionary, string partialKey)
        {
            // This, or use a RegEx or whatever.
            IEnumerable<string> fullMatchingKeys =
                dictionary.Keys.Where(currentKey => currentKey.Contains(partialKey));

            List<T> returnedValues = [];

            foreach (string currentKey in fullMatchingKeys)
            {
                returnedValues.Add(dictionary[currentKey]);
            }

            return returnedValues;
        }
    }
}

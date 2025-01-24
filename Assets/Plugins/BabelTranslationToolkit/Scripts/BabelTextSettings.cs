using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

[Serializable]
public class BabelTextSettings {
    public string prefix = "";
    public string suffixDelimiter = "";
    public bool ignorePrefixMatches;
    public bool ignoreAllCaps = true;
    public bool ignoreNoLetters = true;
    public bool caseSensitive = true;
    public int maxRecursionDepth = 8;
    public SearchRule[] customRules;


    private static readonly List<Type> excludedComponentTypes = new List<Type> {
        typeof(Rigidbody),
        typeof(Transform),
        typeof(BabelTranslator),
        typeof(BabelMarker),
        typeof(UnityEngine.EventSystems.StandaloneInputModule)
    };
    public const BindingFlags FIELD_SEARCH_FLAGS = BindingFlags.Public |
                           BindingFlags.NonPublic |
                           BindingFlags.Instance;

    public BabelTextSettings() {

    }

    public bool IsExcludedType(Component c) {
        return excludedComponentTypes.Contains(c.GetType());
    }

    public bool RequiresBaseLanguage() {
        return RequiresPrefix() | HasSuffix();
    }

    public bool HasPrefix() {
        return !string.IsNullOrEmpty(prefix);
    }

    public bool HasSuffix() {
        return !string.IsNullOrEmpty(suffixDelimiter);
    }

    public bool RequiresPrefix() {
        return !string.IsNullOrEmpty(prefix) && !ignorePrefixMatches;
    }

    public bool MatchesPrefix(string text) {
        if(!HasPrefix())
            return false;
        if(caseSensitive)
            return text.StartsWith(prefix);
        else
            return text.ToLower().StartsWith(prefix.ToLower());
    }

    public int MatchesSuffix(string text) {
        if(!HasSuffix())
            return -1;
        if(caseSensitive)
            return text.IndexOf(suffixDelimiter);
        else
            return text.ToLower().IndexOf(prefix.ToLower());
    }

    /// <summary>
    /// True if given key passes all filters
    /// </summary>
    public bool IsValidKeyString(string fieldType, string fieldName, string key) {
        if(string.IsNullOrEmpty(key) || key.Trim().Length == 0)
            return false;
        if(HasPrefix()) {
            if(MatchesPrefix(key) == ignorePrefixMatches)
                return false;
        }
        if(customRules != null) {
            bool requireFilters = true;
            bool excludeFilters = false;
            foreach(SearchRule rule in customRules) {
                bool filter = rule.Filter(fieldType, fieldName);

                switch(rule.type) {
                case SearchRule.Type.exclude: excludeFilters |= filter; break;
                case SearchRule.Type.require: requireFilters &= filter; break;
                default: Debug.LogError("Unkown Search rule type: " + rule.type); break;
                }
            }
            if(!requireFilters || excludeFilters)
                return false;
        }
        if(ignoreAllCaps || ignoreNoLetters) {
            bool allLetters = true;
            bool anyLetter = false;
            foreach(char c in key.ToCharArray()) {
                bool letter = char.IsLetter(c);
                allLetters &= letter;
                anyLetter |= letter;
            }
            if(ignoreAllCaps && allLetters && key.ToUpper() == key)
                return false;
            if(ignoreNoLetters && !anyLetter)
                return false;
        }

        return true;
    }

    [Serializable]
    public class SearchRule {
        public Type type = Type.exclude;
        public enum Type {
            exclude, require
        }

        public bool matchExact = true;
        public bool matchCase = true;

        public string className = "Class";
        public string variableName = "Variable";

        public bool Filter(string fieldClass, string fieldVariable) {
            string matchClass = className;
            string matchVariable = variableName;

            if(!matchCase) {
                fieldClass = fieldClass.ToLower();
                fieldVariable = fieldVariable.ToLower();
                matchClass = matchClass.ToLower();
                matchVariable = matchVariable.ToLower();
            }

            bool match = true;
            if(matchExact) {
                match &= fieldClass.Equals(matchClass);
                match &= fieldVariable.Equals(matchVariable);
            }
            else {
                match &= fieldClass.Contains(matchClass);
                match &= fieldVariable.Contains(matchVariable);
            }

            return match;
        }
    }
}

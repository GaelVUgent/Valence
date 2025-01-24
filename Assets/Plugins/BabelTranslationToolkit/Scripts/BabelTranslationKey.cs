using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class BabelTranslationKey {

    //settings
    private const char CSV_SEPERATOR = ',';

    //translation structure
    private string keyLanguage;
    private List<string> languages;
    private Dictionary<string, TranslationSet> dictionary;
    private Dictionary<string, string> inverse;
    public int translationCount { get { return dictionary.Count; } }

    //run-time translation variables
    private int selectedLanguage;

    /* =========================================================================================
     * =============================== Construction (loading) ==================================
     * =========================================================================================
     */

    /// <summary>
    /// Create blank translation key
    /// </summary>
    public BabelTranslationKey(string keyLanguage) {
        this.keyLanguage = keyLanguage;
        languages = new List<string>();
        dictionary = new Dictionary<string, TranslationSet>();
    }

    /// <summary>
    /// Create translation key based on a saved CSV format
    /// </summary>
    public BabelTranslationKey(TextAsset keySource) {

        dictionary = new Dictionary<string, TranslationSet>();
        
        string[] lines = keySource.text.Split('\n');
        for (int i = 0; i < lines.Length; i++)
            lines[i] = lines[i].Trim(' ', '\r');
        string[] languages = lines[0].Split(CSV_SEPERATOR);
        SetKeyLanguage(languages[0]);

        if(keyLanguage == "")
            throw new TranslationKeyParseException("File was empty!");

        this.languages = new List<string>(languages.Length - 1);
        for(int i = 1; i < languages.Length; i++)
            AddLanguage(languages[i]);

        for(int i = 1; i < lines.Length; i++)
            AddLineToDictionary(i, ParseLine(lines[i]));
    }

    private void AddLineToDictionary(int line, List<string> translations) {
        if(translations.Count <= 0)
            return;

        string key = translations[0];
        translations.RemoveAt(0);
        if(translations.Count > languages.Count)
            Debug.LogWarning("Found too many translations for \"" + key + "\" at line: " + (line + 1));
        if(dictionary.ContainsKey(key))
            Debug.LogWarning("Ignoring duplicate \"" + key + "\" at line: " + (line + 1));
        else
            dictionary.Add(key, new TranslationSet(translations));
    }

    private List<string> ParseLine(string line) {
        List<string> toReturn = new List<string>();
        StringBuilder nextString = new StringBuilder();
        bool foundQuote = false;
        bool foundSeperator = true;
        bool inValue = false;
        bool currentValueIsInQuotes = false;
        bool foundEncodedCharacter = false;
        List<char> ignoredChars = new List<char> { '\r' };

        char[] lineArray = line.ToCharArray();
        foreach(char c in lineArray) {
            if(ignoredChars.Contains(c))
                continue;
            if(!inValue) {
                if(c == '\"') {
                    currentValueIsInQuotes = true;
                    inValue = true;
                    foundQuote = false;
                    nextString = new StringBuilder();
                    continue;
                }
                else if(c == CSV_SEPERATOR) {
                    if(foundSeperator)
                        toReturn.Add("");
                    else
                        foundSeperator = true;
                }
                else {
                    currentValueIsInQuotes = false;
                    inValue = true;
                    nextString = new StringBuilder();
                }
            }
            if(inValue) {
                if(foundEncodedCharacter) {
                    if(c == 'n')
                        nextString.Append("\n");
                    else if(c == 's')
                        nextString.Append(" ");
                    else if(c == '\\' || c == '\"' || c == CSV_SEPERATOR)
                        nextString.Append(c);
                    else {
                        Debug.LogWarning("Unknown encoded character: \\" + c);
                        nextString.Append(c);
                    }
                    foundEncodedCharacter = false;
                }
                else if(c == '\\')
                    foundEncodedCharacter = true;
                else if(currentValueIsInQuotes) {
                    if(c == '\"') {
                        if(foundQuote) {
                            nextString.Append(c);
                            foundQuote = false;
                        }
                        else
                            foundQuote = true;
                    }
                    else if(foundQuote) {
                        if(c == '\\') {
                            foundEncodedCharacter = true;
                            foundQuote = false;
                        }
                        else {
                            inValue = false;
                            foundSeperator = false;
                            foundQuote = false;
                            toReturn.Add(nextString.ToString());
                        }
                    }
                    else
                        nextString.Append(c);
                }
                else {
                    if(c == CSV_SEPERATOR) {
                        inValue = false;
                        foundSeperator = true;
                        toReturn.Add(nextString.ToString());
                    }
                    else
                        nextString.Append(c);
                }
            }
        }
        if(inValue)
            toReturn.Add(nextString.ToString());
        return toReturn;
    }







    /* =========================================================================================
    * ====================================== Saving ===========================================
    * =========================================================================================
    */

    private void ApplyKeyChanges() {
        Dictionary<string, TranslationSet> source = dictionary;
        dictionary = new Dictionary<string, TranslationSet>();
        foreach(KeyValuePair<string, TranslationSet> translation in source) {
            string key = translation.Key;
            if(!string.IsNullOrWhiteSpace(translation.Value.newKey))
                key = translation.Value.newKey;
            dictionary[key] = translation.Value;
        }
    }

    /// <summary>
    /// Convert the data in this key to a csv string format that can 
    /// be used to reconstruct it later.
    /// </summary>
    public string Save() {
        ApplyKeyChanges();

        StringBuilder toReturn = new StringBuilder(keyLanguage);
        foreach(string language in languages)
            toReturn.Append(CSV_SEPERATOR + language);
        toReturn.Append('\n');
        foreach(KeyValuePair<string, TranslationSet> pair in dictionary) {
            AppendFormat(toReturn, pair.Key);
            foreach(string translation in pair.Value.translations) {
                toReturn.Append(CSV_SEPERATOR);
                AppendFormat(toReturn, translation);
            }
            toReturn.Append('\n');
        }
        return toReturn.ToString();
    }

    private void AppendFormat(StringBuilder builder, string input) {
        input = input.Replace("\\", "\\\\").Replace("\n", "\\n");
        if(string.IsNullOrEmpty(input) || input.Contains(CSV_SEPERATOR.ToString()) || input.Contains("\"")) {
            builder.Append('\"');
            builder.Append(input.Replace("\"", "\"\""));
            builder.Append('\"');
        }
        else
            builder.Append(input);
    }







    /* =========================================================================================
     * ==================== dynamic key editing (connections for editor) =======================
     * =========================================================================================
     */




    // languages

    public bool SetKeyLanguage(string key) {
        key = key.Replace(CSV_SEPERATOR.ToString(), "").Replace("\"", "").Replace("\\", "").Replace("\n", "");
        if(keyLanguage == key)
            return false;
        keyLanguage = key;
        return true;
    }

    public string GetKeyLanguage() {
        return keyLanguage;
    }

    public List<string> GetLanguages() {
        return languages;
    }

    public bool SetLanguage(int index, string language) {
        if(languages[index] == language)
            return false;
        languages[index] = language;
        return true;
    }

    public bool AddLanguage(string key) {
        if(string.IsNullOrEmpty(key) || key.Trim().Length == 0)
            return false;

        key = key.Replace(CSV_SEPERATOR.ToString(), "").Replace("\"", "").Replace("\\", "").Replace("\n", "");

        if(languages.Contains(key))
            return false;

        languages.Add(key);
        return true;
    }

    public void RemoveLanguage(int index) {
        languages.RemoveAt(index);
        foreach(KeyValuePair<string, TranslationSet> pair in dictionary) {
            if(index < pair.Value.translations.Count)
                pair.Value.translations.RemoveAt(index);
        }
    }




    // base language; prefix and suffix settings

    public void InsertBaseLanguage(BabelTextSettings settings) {

        //establish names for key language and base language
        string baseLanguage = keyLanguage;
        if(settings.RequiresPrefix()) {
            int pl = settings.prefix.Length;
            if(settings.MatchesPrefix(keyLanguage))
                baseLanguage = baseLanguage.Substring(pl);
            else
                keyLanguage = settings.prefix + keyLanguage;
        }
        if(baseLanguage == keyLanguage) {
            keyLanguage = "Input";
        }

        //do the insert
        languages.Insert(0, baseLanguage);
        foreach(KeyValuePair<string, TranslationSet> pair in dictionary)
            pair.Value.translations.Insert(0, "");
        RemakeBaseLanguage(settings);
    }

    public void RemakeBaseLanguage(BabelTextSettings settings) {
        int pl = settings.HasPrefix() ? settings.prefix.Length : 0;
        int baseLanguageIndex = 0;
        foreach(string key in GetKeys()) {
            string baseTranslation = key;
            if(settings.MatchesPrefix(key))
                baseTranslation = baseTranslation.Substring(pl);
            int suffix = settings.MatchesSuffix(baseTranslation);
            if(suffix >= 0)
                baseTranslation = baseTranslation.Substring(0, suffix);
            SetTranslation(key, baseLanguageIndex, baseTranslation);
        }
    }






    // key editing

    public bool AddKeys(params BabelTranslator.TranslationString[] keys) {
        bool toReturn = false;
        foreach(BabelTranslator.TranslationString key in keys) {
            if(string.IsNullOrEmpty(key.value) || key.value.Trim().Length == 0)
                continue;
            string processedKey = key.value.Replace("\\n", "\n");
            TranslationSet translation;
            bool exists = dictionary.TryGetValue(processedKey, out translation);
            if(exists)
                translation.sources.Add(key);
            else {
                toReturn = true;
                dictionary.Add(processedKey, new TranslationSet(key));
            }
        }
        return toReturn;
    }

    public bool AddManual(string key) {
        if(string.IsNullOrWhiteSpace(key))
            return false;
        string processedKey = key.Replace("\\n", "\n");
        TranslationSet translation;
        bool exists = dictionary.TryGetValue(processedKey, out translation);
        if(exists)
            return false;
        else {
            dictionary.Add(processedKey, new TranslationSet(key));
            return true;
        }
    }

    public bool RemoveKey(string key) {
        return dictionary.Remove(key);
    }

    public bool Clear() {
        if(dictionary.Count > 0) {
            dictionary.Clear();
            return true;
        }
        return false;
    }

    public List<string> GetKeys() {
        List<string> toReturn = new List<string>();
        foreach(KeyValuePair<string, TranslationSet> pair in dictionary)
            toReturn.Add(pair.Key);
        return toReturn;
    }

    public List<string> GetTranslations(string key) {
        return dictionary[key].translations;
    }

    public bool HasReferences(string key) {
        return dictionary[key].HasReferences();
    }

    public bool IsNew(string key) {
        return dictionary[key].IsNew();
    }

    public string GetLabel(string key) {
        return dictionary[key].GetLabel(key);
    }

    public bool SetTranslation(string key, int languageIndex, string translation) {
        List<string> translations = dictionary[key].translations;
        for(int i = translations.Count; i <= languageIndex; i++)
            translations.Add("");
        translation = translation.Replace("\\n", "\n");
        if(translations[languageIndex] != translation) {
            translations[languageIndex] = translation;
            return true;
        }
        return false;
    }

    /* =========================================================================================
     * ======================== Translation functionality (runtime) ============================
     * =========================================================================================
     */

    /// <summary>
    /// Change the output language of Translate()
    /// </summary>
    /// <param name="language">The language to translate to. Must match one of the languages in the translation key.</param>
    /// <returns>True if the given language was valid and could be selected</returns>
    public bool SelectLanguage(string language) {
        language = language.Trim();
        for(int i = 0; i < languages.Count; i++) {
            if (language.Equals(languages[i], StringComparison.OrdinalIgnoreCase)) {
                selectedLanguage = i;
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// Returns translation of the given input if it is stored in this translationKey.
    /// Returns the input itself otherwise.
    /// </summary>
    /// <param name="input">Input string to translate. This should match one of the key strings in the translation key.</param>
    /// <param name="allowCross">Allow translations from non-key languages in the translation key</param>
    public string Translate(string input, bool allowCross) {
        Translate(input, out string toReturn, allowCross);
        return toReturn;
    }

    /// <summary>
    /// Returns true if a translation of the given key string is available.
    /// </summary>
    /// <param name="input">Key string for translation, which should belong to the key language</param>
    /// <param name="translation">the input string translated if possible.</param>
    /// <param name="allowCross">Allow translations from non-key languages in the translation key</param>
    public bool Translate(string input, out string translation, bool allowCross) {
        return Translate(input, selectedLanguage, out translation, allowCross);
    }

    /// <summary>
    /// Returns true if a translation of the given key string is available.
    /// </summary>
    /// <param name="input">Key string for translation, which should belong to the key language</param>
    /// <param name="language">Index of the language in which to translate</param>
    /// <param name="translation">the input string translated if possible.</param>
    /// <param name="allowCross">Allow translations from non-key languages in the translation key</param>
    public bool Translate(string input, int language, out string translation, bool allowCross) {
        TranslationSet translations;
        if(string.IsNullOrEmpty(input)) {
            translation = "";
            return true;
        }
        input = input.Trim();

        //Check if a key translation exists
        if(dictionary.TryGetValue(input, out translations)) {
            if(translations.translations.Count > language) {
                translation = translations.translations[language];
                if(!string.IsNullOrEmpty(translation))
                    return true;
            }
        }

        //if cross translations are allowed, check if a cross translation exists
        else if(allowCross && Invert(input, out string key) && dictionary.TryGetValue(key, out translations)) {
            if(translations.translations.Count > language) {
                translation = translations.translations[language];
                if(!string.IsNullOrEmpty(translation))
                    return true;
            }
        }

        //no translation exists, return the input
        translation = input;
        Debug.LogWarning("Could not find translation for " + input + " in language " + languages[language]);
        return false;
    }

    /// <summary>
    /// Make an inverse dictionary to convert translations back to the key language
    /// </summary>
    private void MakeInverse() {
        inverse = new Dictionary<string, string>();
        string collission = null;
        foreach(KeyValuePair<string, TranslationSet> pair in dictionary) {
            foreach(string translation in pair.Value.translations) {
                if(string.IsNullOrEmpty(translation))
                    continue;
                if(inverse.TryGetValue(translation, out string collissionKey)){
                    if(collissionKey != pair.Key)
                        collission = translation;
                }
                else
                    inverse.Add(translation, pair.Key);
            }
        }
        Debug.LogWarning("One or more translations (\"" + collission + "\") occurs for different source texts, this may poorly affect the working of the cross translation functionality.");
    }

    /// <summary>
    /// Convert a translation back to the key language.
    /// Returns the input string if no inverted translation is available.
    /// </summary>
    public bool Invert(string translation, out string key) {
        if(inverse == null)
            MakeInverse();
        return inverse.TryGetValue(translation, out key);
    }

    /// <summary>
    /// Change occurences of a given key in the current scene and queue the 
    /// same change to be applied to this translation key upon the next save.
    /// </summary>
    public void ChangeKey(string key, string newKey) {
        dictionary[key].ChangeKey(newKey);
    }

    /// <summary>
    /// Returns string that is queued as the change for the given key.
    /// By default, the given key itself will be returned.
    /// </summary>
    public string GetKeyChange(string key) {
        string toReturn = dictionary[key].newKey;
        if(toReturn == null)
            return key;
        return toReturn;
    }

    public bool HasKey(string key) {
        return dictionary.ContainsKey(key);
    }

    public int GetSelectedLanguage() {
        return selectedLanguage;
    }




    /* =========================================================================================
     * ================================== Helper classes =======================================
     * =========================================================================================
     */

    /// <summary>
    /// Notify that something has gone wrong iwht parsing a csv saved key
    /// </summary>
    public class TranslationKeyParseException : Exception {
        public TranslationKeyParseException(string message) : base(message) { }
    }

    /// <summary>
    /// An entry (1 line) for the translation key.
    /// Apart from the translations themselves this set also 
    /// contains trace information to show the user where a certain 
    /// key comes from. This is only used in the editor.
    /// </summary>
    private class TranslationSet {
        public readonly List<string> translations;
        public string newKey;
        public readonly Source source;
        public readonly List<BabelTranslator.TranslationString> sources;
        public enum Source { Search, Parse, Manual }

        public TranslationSet(BabelTranslator.TranslationString source) {
            this.translations = new List<string>();
            this.source = Source.Search;
            this.sources = new List<BabelTranslator.TranslationString>() { source };
        }

        public TranslationSet(List<string> translations) {
            this.translations = translations;
            this.source = Source.Parse;
            this.sources = new List<BabelTranslator.TranslationString>();
        }

        public TranslationSet(string value) {
            this.translations = new List<string>();
            this.source = Source.Manual;
            this.sources = new List<BabelTranslator.TranslationString>();
        }

        public bool HasReferences() {
            return sources.Count > 0;
        }

        public bool IsNew() {
            return source != Source.Parse;
        }

        public void ChangeKey(string value) {
            newKey = value;
            foreach(BabelTranslator.TranslationString source in sources)
                source.Change(value);
        }

        public string GetLabel(string key) {
            List<string> sourceStrings = new List<string>();
            List<int> m = new List<int>();
            foreach(BabelTranslator.TranslationString source in sources) {
                string sourceString = source.GetSourceString();
                int index = sourceStrings.FindIndex((string v) => { return v == sourceString; });
                if(index < 0) {
                    sourceStrings.Add(sourceString);
                    m.Add(1);
                }
                else
                    m[index]++;
            }
            string toReturn = key;
            switch(source) {
            case Source.Search: toReturn += "\n> From scene search:"; break;
            case Source.Parse: toReturn += "\n> From existing key. " + 
                ((sourceStrings.Count > 0) ? "Found references:" : "No known references."); break;
            case Source.Manual: toReturn += "\n> Added manually."; break;
            }
            for(int i = 0; i < sourceStrings.Count; i++)
                toReturn += "\n" + sourceStrings[i] + ((m[i] > 1) ? (" (x" + m[i] + ")") : "");
            return toReturn;
        }
    }
}

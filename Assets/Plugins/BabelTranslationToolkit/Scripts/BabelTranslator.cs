using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class BabelTranslator : MonoBehaviour {
    
    //runtime translation
    public TextAsset translationKey;
    private BabelTranslationKey key;
    private List<StringField> sceneText;
    private static BabelTranslator _currentTranslator;
    public static BabelTranslator current {
        get {
            if(!_currentTranslator) {
                _currentTranslator = FindObjectOfType<BabelTranslator>();
                if(!_currentTranslator) {
                    Debug.LogError("No translator available!");
                    return null;
                }
            }

            if(!_currentTranslator.initialized) {
                _currentTranslator.Initialize();
                if(!_currentTranslator.initialized) {
                    Debug.LogError("Translator is not initialized!");
                    return null;
                }
            }
            return _currentTranslator;
        }
    }
    public bool initialized { get { return key != null; } }
    public static bool dynamicTranslations;
    public static string dynamicLanguage;
    public UnityMode unityMode;
    public enum UnityMode { Singleton, PerScene }
    private Thread asyncScanThread;
    public SceneMode sceneMode;
    public enum SceneMode { Automatic, AutomaticNoReset, Passive }
    private List<BabelMarker> markers;

    public BabelTextSettings textScanSettings;

    //functionality settings
    public string startingLanguage;
    public bool allowCrossTranslations = true;
    public bool allowMultiThreadSearch = false;

    //debugging settings
    public bool showDefaultGUI = false;
    public bool logWarnings = true;


    /* =========================================================================================
     * ==================================== Unity basis ========================================
     * =========================================================================================
     */

    private void Awake() {
        if(_currentTranslator && _currentTranslator != this) {
            switch(unityMode) {

            case UnityMode.PerScene:
            if(logWarnings)
                Debug.LogWarning("There are multiple Babel Translators in this scene, only one will be used!");
            break;

            case UnityMode.Singleton:
            Destroy(gameObject);
            break;

            }

        }
        else {
            _currentTranslator = this;
            switch(unityMode) {
            case UnityMode.Singleton:
            DontDestroyOnLoad(gameObject);
            break;
            }
        }
    }

    private void InitScene(Scene oldScene, LoadSceneMode mode) {
        StartCoroutine(InitDelayed());
    }

    private IEnumerator InitDelayed() {
        if(sceneMode != SceneMode.Passive)
            yield return ScanSceneForTextAsync();
        if(dynamicTranslations)
            Translate(dynamicLanguage);
    }

    private void OnEnable() {
        if(!initialized)
            Initialize();
    }

    private void OnDisable() {
        if(initialized)
            Restore();
        sceneText = null;
    }

    private void Initialize() {
        key = null;
        sceneText = null;
        if(translationKey == null) {
            if(logWarnings)
                Debug.LogWarning("No translation key provided!");
            return;
        }
        try {
            key = new BabelTranslationKey(translationKey);
        } catch(BabelTranslationKey.TranslationKeyParseException e) {
            Debug.LogError("Could not parse given translation key:\n" + e);
            return;
        }
        if(sceneMode != SceneMode.Passive)
            sceneText = ScanSceneForText();
        markers = new List<BabelMarker>();
        Translate(startingLanguage);
        SceneManager.sceneLoaded += InitScene;
    }

    private bool CheckInit(string message) {
        if(!initialized) {
            Debug.LogError(message);
            return true;
        }
        return false;
    }


    /* =========================================================================================
     * ===================================== Debugging =========================================
     * =========================================================================================
     */

    void OnGUI() {
        if(!showDefaultGUI || !initialized)
            return;
        GUILayout.Box("Scene translations");
        foreach(string language in GetLanguages()) {
            if(GUILayout.Button(language)) {
                if(sceneMode != SceneMode.Passive)
                    HardRefresh();
                TranslateScene(language);
            }
        }

    }

    /* =========================================================================================
     * ============================== User functions - General =================================
     * =========================================================================================
     */

    /// <summary>
    /// Return an array containing all valid languages of this translator, including the key language.
    /// </summary>
    public string[] GetLanguages() {
        if(CheckInit("Translator is not initialized!"))
            return null;

        string[] toReturn = new string[key.GetLanguages().Count + 1];
        toReturn[0] = key.GetKeyLanguage();
        int index = 1;
        foreach(string language in key.GetLanguages())
            toReturn[index++] = language;
        return toReturn;
    }

    /// <summary>
    /// Return an array containing all valid languages for the current translator, including the key langage
    /// </summary>
    public static string[] GetAllLanguages() {
        return current.GetLanguages();
    }

    /// <summary>
    /// Return an array containing the languages for the current translator, excluding the key language.
    /// Use this to provide the player with language options when the in-scene key text should never be shown.
    /// </summary>
    public static string[] GetTranslationLanguages() {
        return current.key.GetLanguages().ToArray();
    }

    /// <summary>
    /// Attempts to find and initialize the babel translator. Returns false if 
    /// this was not possible.
    /// </summary>
    public static bool IsTranslatorAvailableAndInitialized() {
        if(!_currentTranslator)
            _currentTranslator = FindObjectOfType<BabelTranslator>();
        if(!_currentTranslator)
            return false;
        if(!_currentTranslator.initialized)
            _currentTranslator.Initialize();
        return _currentTranslator.initialized;
    }


    /* =========================================================================================
     * ========================= User functions - Scene translations ===========================
     * =========================================================================================
     */

    /// <summary>
    /// Translate scene text to the given language
    /// </summary>
    public void Translate(string language) {
        dynamicLanguage = language;
        SetLanguage(language);

        if (CheckInit("Cannot translate while translator is not initialized!"))
            return;
        
        foreach(BabelMarker marker in markers)
            marker.value = Tr(marker.key);
        if(sceneMode == SceneMode.Passive)
            return;

        string translation;
        if(language.Equals(key.GetKeyLanguage(), StringComparison.OrdinalIgnoreCase)) {
            if(allowCrossTranslations) {
                foreach(StringField field in sceneText) {
                    if(key.Invert(field.key, out translation))
                        field.SetValue(translation);
                    else
                        field.Recover();
                }
            }
            else
                Restore();
        }
        else if(key.SelectLanguage(language)) {
            foreach(StringField field in sceneText) {
                if(key.Translate(field.key, out translation, allowCrossTranslations))
                    field.SetValue(translation);
            }
        }
        else {
            Debug.LogError("Current translation key has no entry for language \"" + language + "\"");
        }

        if(sceneMode == SceneMode.Automatic) {
            foreach(Canvas c in FindObjectsOfType<Canvas>()) {
                if(c.gameObject.activeSelf) {
                    c.gameObject.SetActive(false);
                    c.gameObject.SetActive(true);
                }
            }
        }
    }

    /// <summary>
    /// return scene text to its original untranslated state
    /// </summary>
    public void Restore() {
        if(CheckInit("Cannot translate while translator is not initialized!"))
            return;
        if(sceneMode != SceneMode.Passive)
            foreach(StringField field in sceneText)
                field.Recover();
    }

    /// <summary>
    /// translate all scene references to the given language.
    /// Note that newly instantiated objects are not automatically referenced, 
    /// you may have to call QuickRefresh() or HardRefresh()
    /// </summary>
    public static void TranslateScene(string language) {
        current.Translate(language);
    }

    /// <summary>
    /// Return scene references to the text content they had when they were first scanned
    /// </summary>
    public static void RestoreScene() {
        current.Restore();
    }

    /// <summary>
    /// Reload all scene references affected by TranslateScene().
    /// Clean up destroyed references and aggresively look for new references.
    /// Note that this refresh might take a few seconds for bigger scenes.
    /// </summary>
    public static void HardRefresh() {
        current.RefreshSceneText(false);
    }

    /// <summary>
    /// Reload scene references affected by TranslateScene().
    /// Clean up destroyed references and look for new UI text references.
    /// This refresh should be considerably faster than HardRefresh().
    /// </summary>
    public static void QuickRefresh() {
        current.RefreshSceneText(true);
    }



    /* =========================================================================================
     * ======================== User functions - Dynamic translations ==========================
     * =========================================================================================
     */



    /// <summary>
    /// Set dynamic translations to the default (key) language, so that Tr() 
    /// will simply return whatever string is provided as input. 
    /// </summary>
    public static void SetNoTranslate() {
        dynamicTranslations = false;
    }

    /// <summary>
    /// Change language output of dynamic translations via Tr(). 
    /// Note that translating the scene will automatically apply this method. 
    /// </summary>
    public static void SetLanguage(string language) {
        if(!IsTranslatorAvailableAndInitialized())
            return;
        if(language == current.key.GetKeyLanguage())
            dynamicTranslations = false;
        else if(current.key.SelectLanguage(language))
            dynamicTranslations = true;
        else {
            dynamicTranslations = false;
            Debug.LogError($"Unkown language \"{language}\" reporting available languages:");
            foreach (string l in current.key.GetLanguages())
                Debug.Log($"\"{l}\"");
        }
        if(dynamicTranslations)
            dynamicLanguage = language;
    }

    /// <summary>
    /// Dynamic translation; quickly translate the given string to whatever language was last set via SetLanguage(). 
    /// </summary>
    public static string Tr(string input) {
        if(dynamicTranslations) {
            if(current.key.SelectLanguage(dynamicLanguage))
                return current.key.Translate(input, current.allowCrossTranslations);
        }
        return input;
    }

    /// <summary>
    /// Dynamic translation; quickly translate the given string to whatever language was last set via SetLanguage(). 
    /// </summary>
    /// <param name="input">String to be translated, optional arguments in the form of {0}, {1}...</param>
    /// <param name="args">Arguments to insert in the translated string</param>
    /// <returns></returns>
    public static string Tr(string input, params object[] args) {
        string format = Tr(input);
        return string.Format(format, args);
    }
    
    /// <summary>
    /// Translate the given input string into any language without changing dynamic translation settings.
    /// </summary>
    /// <param name="input"></param>
    /// <param name="language"></param>
    public static string Trl(string input, int language) {
        string toReturn = input;
        current.key.Translate(input, language, out toReturn, false);
        return toReturn;
    }
    
    /// <summary>
    /// Apply dynamic translations to all string elements in the given dropdown menu
    /// </summary>
    public static void Tr(Dropdown dd) {
        List<string> options = new List<string>();
        foreach(Dropdown.OptionData option in dd.options)
            options.Add(Tr(option.text));
        dd.ClearOptions();
        dd.AddOptions(options);
    }
    
    /// <summary>
    /// Add a babelmarker to the registry to be translated
    /// </summary>
    public static int Register(BabelMarker marker) {
        if(!IsTranslatorAvailableAndInitialized())
            return -1;
        List<BabelMarker> markers = current.markers;
        markers.Add(marker);
        marker.value = Tr(marker.key);
        return markers.Count - 1;
    }
    
    /// <summary>
    /// Remove a babelmarker so it is no longer affected by translations
    /// </summary>
    public static void UnRegister(BabelMarker marker, int index) {
        if(!IsTranslatorAvailableAndInitialized())
            return;
        List<BabelMarker> markers = current.markers;
        if(markers.Count > index && markers[index] == marker) {
            //delete marker; shuffle last marker into its position and then remove last element
            markers[index] = markers[markers.Count - 1];
            markers[index].index = index;
            current.markers.RemoveAt(markers.Count - 1);
        }
    }


    /* =========================================================================================
     * ================================== Editor functions =====================================
     * =========================================================================================
     */



    /// <summary>
    /// Return list of strings this translator has access to in its current search configuration
    /// </summary>
    public TranslationString[] GetKeyStringsFromScene() {
        List<StringField> sceneText = ScanSceneForText();
        TranslationString[] toReturn = new TranslationString[sceneText.Count];
        for(int i = 0; i < sceneText.Count; i++) {
            toReturn[i] = new TranslationString();
            toReturn[i].value = sceneText[i].key;
            toReturn[i].sourceScene = SceneManager.GetActiveScene().name;
            toReturn[i].source = sceneText[i];
        }
        return toReturn;
    }




    /* =========================================================================================
     * ============================== Text scanning internals ==================================
     * =========================================================================================
     */

    private void RefreshSceneText(bool quick) {
        for(int i = 0; i < sceneText.Count; i++) {
            if(sceneText[i].obj == null)
                sceneText.RemoveAt(i);
        }
        if(quick) {
            Text[] uiText = FindObjectsOfType<Text>();
            FieldInfo textField = typeof(Text).GetField("m_Text", BabelTextSettings.FIELD_SEARCH_FLAGS);
            foreach(Text t in uiText) {
                StringField stringField = new StringField(new ValueField(textField, t));
                if(!sceneText.Contains(stringField))
                    sceneText.Add(stringField);
            }
        }
        else {
            List<StringField> newText = ScanSceneForText();
            foreach(StringField newField in newText) {
                if(!sceneText.Contains(newField))
                    sceneText.Add(newField);
            }
        }
    }

    private List<StringField> ScanSceneForText() {
        List<StringField> toReturn = new List<StringField>();
        
        List<GameObject> roots = GetActiveRoots();
        foreach (GameObject g in roots) {
            foreach(Component c in g.GetComponentsInChildren<Component>(true)) {
                if(c == null)
                    continue;
                if(textScanSettings.IsExcludedType(c))
                    continue;
                Component searchObject = c;
                Stack<object> recursionTrace = new Stack<object>();
                toReturn.AddRange(ScanObjectForText(c, recursionTrace, searchObject));
            }
        }

        return toReturn;
    }

    private IEnumerator ScanSceneForTextAsync() {
        sceneText = new List<StringField>();

        yield return null;
        List<GameObject> roots = GetActiveRoots();
        List<Component> scanComponents = new List<Component>();
        yield return null;

        foreach(GameObject g in roots) {
            if(g == null)
                continue;
            scanComponents.AddRange(g.GetComponentsInChildren<Component>(true));
            yield return null;
        }
#if UNITY_WEBGL
        yield return ScanComponentsForTextRoutine(scanComponents);
#else
        if(allowMultiThreadSearch) {
            asyncScanThread = new Thread(() => ScanComponentsForTextAsync(scanComponents));
            asyncScanThread.Start();

            while(asyncScanThread.IsAlive)
                yield return null;
        }
        else
            yield return ScanComponentsForTextRoutine(scanComponents);
#endif
    }

    private List<GameObject> GetActiveRoots() {
        List<GameObject> roots = new List<GameObject>();
        for (int i = 0; i < SceneManager.sceneCount; i++) {
            foreach (GameObject g in SceneManager.GetSceneAt(i).GetRootGameObjects()) {
                if (g.activeSelf)
                    roots.Add(g);
            }
        }
        return roots;
    }

    private void ScanComponentsForTextAsync(List<Component> scanObjects) {
        foreach(Component c in scanObjects) {
            if(textScanSettings.IsExcludedType(c))
                continue;
            Component searchObject = c;
            Stack<object> recursionTrace = new Stack<object>();
            sceneText.AddRange(ScanObjectForText(c, recursionTrace, searchObject));
        }
    }

    private IEnumerator ScanComponentsForTextRoutine(List<Component> scanObjects) {
        long timeStamp = DateTime.Now.Ticks;
        foreach(Component c in scanObjects) {
            long timePassed = DateTime.Now.Ticks - timeStamp;
            if(timePassed > 50000) { //about 1/3rd of a frame
                timeStamp = DateTime.Now.Ticks;
                yield return null;
            }
            if(textScanSettings.IsExcludedType(c))
                continue;
            Component searchObject = c;
            Stack<object> recursionTrace = new Stack<object>();
            sceneText.AddRange(ScanObjectForText(c, recursionTrace, searchObject));
        }
    }

    private List<StringField> ScanObjectForText(object o, Stack<object> recursionTrace, Component searchObject) {
        List<StringField> toReturn = new List<StringField>();

        //collect base level variables
        List<ValueField> fields = new List<ValueField>();
        foreach(FieldInfo field in o.GetType().GetFields(BabelTextSettings.FIELD_SEARCH_FLAGS))
            fields.Add(new ValueField(field, o));

        //unpack any (nested) arrays
        for(int i = 0; i < fields.Count; i++) {
            if(fields[i].field.FieldType.IsArray) {
                IList collection = (IList)fields[i].value;
                if(collection != null) {
                    foreach(object element in collection) {
                        if(element != null) {
                            foreach(FieldInfo field in element.GetType().GetFields(BabelTextSettings.FIELD_SEARCH_FLAGS))
                                fields.Add(new ValueField(field, element));
                        }
                    }
                }
            }
        }

        //extract any strings, continue recusrion
        foreach(ValueField field in fields) {
            if(field.value != null) {
                Type fieldType = field.field.FieldType;
                if(fieldType == typeof(string)) {
                    string stringValue = (string)field.value;
                    if(IsValidKeyString(field, stringValue))
                        toReturn.Add(new StringField(field));

                }
                
                else if(IsValidForRecursion(field, fieldType, recursionTrace)) {
                    recursionTrace.Push(o);
                    if(recursionTrace.Count < textScanSettings.maxRecursionDepth)
                        toReturn.AddRange(ScanObjectForText(field.value, recursionTrace, searchObject));
                    else if(logWarnings){
                        string valueInfo = field.obj == null ? "a null field" :
                            field.value + " belonging to class " + field.obj.GetType();
                        string message = "Translator text search stack overflow. " + 
                            "(Consider increasing recursion depth or turn of warning logging)\n" +
                            "Escaped recursion at " + valueInfo + " with trace:\n";
                        foreach(object traceObject in recursionTrace)
                            message += traceObject + "\n";
                        Debug.LogWarning(message + " in a " + searchObject.GetType() +
                            " component of object " + searchObject.name);
                    }
                    recursionTrace.Pop();
                }
            }
        }

        return toReturn;
    }

    private static bool IsValidForRecursion(ValueField field, Type fieldType, Stack<object> recursionTrace)
    {
        if(!fieldType.IsClass | fieldType == typeof(Component))
            return false;
        foreach(object o in recursionTrace)
            if(ReferenceEquals(o, field.value))
                return false;
        return true;
    }

    /// <summary>
    /// True if given key passes all filters
    /// </summary>
    private bool IsValidKeyString(ValueField field, string keyString) {
        string fieldType = field.obj.GetType().ToString();
        string fieldName = field.field.Name;
        if(!textScanSettings.IsValidKeyString(fieldType, fieldName, keyString))
            return false;
        if(key != null)
            return key.Translate(keyString, out string x, allowCrossTranslations);
        return true;
    }
    
    public static int GetCurrentLanguageIndex() {
        return current.key.GetSelectedLanguage();
    }


    /* =========================================================================================
     * =================================== Helper classes ======================================
     * =========================================================================================
     */

    /// <summary>
    /// Encodes a string to be translated along with information about where it came from
    /// </summary>
    public struct TranslationString {
        public string value;
        public string sourceScene;
        public StringField source;

        public bool HasSource() {
            return source.field != null;
        }

        public void Change(string value) {
            source.SetValue(value);
        }

        public string GetSourceString() {
            return sourceScene + "; " + source.GetSourceString();
        }
    }

    /// <summary>
    /// Encodes a general reflection field, which may be a valid string field.
    /// </summary>
    public struct ValueField {
        public readonly FieldInfo field;
        public readonly object obj;
        public readonly object value;

        public ValueField(FieldInfo field, object obj) {
            this.field = field;
            this.obj = obj;
            value = field.GetValue(obj);
        }
    }

    /// <summary>
    /// Encodes a reflection string field, which may be valid for translation.
    /// </summary>
    public struct StringField {
        public readonly FieldInfo field;
        public readonly object obj;
        public readonly string key;

        public StringField(ValueField field) {
            this.field = field.field;
            this.obj = field.obj;
            key = (string)field.value;
        }

        public void SetValue(string value) {
            if(obj != null)
                field.SetValue(obj, value);
        }

        public void Recover() {
            SetValue(key);
        }

        public override bool Equals(object obj) {
            if(obj == null || !(obj is StringField))
                return false;
            StringField other = (StringField)obj;
            if(other.obj != this.obj)
                return false;
            return other.field.Name == field.Name;
        }

        public override int GetHashCode() {
            int hash = obj.GetHashCode();
            return ((hash << 5) + hash + (hash >> 23)) ^ field.Name.GetHashCode();
        }

        public override string ToString() {
            if(obj == null)
                return "Null - StringField";
            return string.Join(";", obj.GetType(), field.Name, field.GetValue(obj));
        }

        public string GetSourceString() {
            string typeName = obj.GetType().ToString();
            string fieldName = field.Name;
            return typeName + "; " + fieldName;
        }
    }
}

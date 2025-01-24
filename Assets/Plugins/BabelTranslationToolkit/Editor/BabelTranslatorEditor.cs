using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

public class BabelTranslatorEditor : EditorWindow {

    //editor variables
    private const string WINDOW_TITLE = "Babel Translator Wizard";
    private TextAsset keyText;
    private BabelTranslationKey key;

    //gui variables
    private int windowWidth, windowHeight;
    private string newKeyLanguage = "ENG";
    private string newLanguage = "";
    private string newKey = "";
    private Vector2 scrollViewPosition;
    private Color backgroundColor;
    private bool dirty;
    private string editingKey;

    //gui layout settings
    private int SCROLL_WIDTH { get { return windowWidth - 2; } }
    private const int LEFT_WIDTH = 147;
    private const int RIGHT_SPACING = 10;
    private const int VERTICAL_SPACING = 10;
    private const int BUTTON_INDENT = 8;
    private const int FIELD_HEIGHT = 30;
    private const int KEY_LIST_VERTICAL = 245;
    private int RIGHT_WIDTH { get { return SCROLL_WIDTH - LEFT_WIDTH - 26; } }
    private int SCROLL_SPACING = 20;
    private static readonly Color NEGATIVE_SAVE_COLOR = new Color(1f, .85f, .75f);
    private static readonly Color POSITIVE_SAVE_COLOR = new Color(.7f, 1f, .7f);
    private static readonly Color DELETE_COLOR = new Color(1f, .6f, .5f);
    private static readonly Color NEW_COLOR = new Color(0f, .5f, 0f);
    private static readonly Color NOREF_COLOR = new Color(.4f, .0f, 0f);
    private static readonly Color EMPTY_COLOR = new Color(1f, .85f, 75f);

    //text scan settings
    private BabelTextSettings textScanSettings;


    /* =========================================================================================
     * ================================= Editor window basis ===================================
     * =========================================================================================
     */

    [MenuItem("Window/Babel Translation Wizard")]
    public static BabelTranslatorEditor TranslatorWizard() {
        var window = GetWindow<BabelTranslatorEditor>();
        window.titleContent = new GUIContent(WINDOW_TITLE);
        window.Focus();
        window.Repaint();
        AssemblyReloadEvents.beforeAssemblyReload += OnReload;
        return window;
    }

    private void SetParameters(BabelTranslator tr) {
        if(tr.translationKey != null)
            LoadKey(tr.translationKey);
        textScanSettings = tr.textScanSettings;
    }

    private static void OnReload() {
        GetWindow<BabelTranslatorEditor>().ExitSavePrompt();
    }

    private void OnEnable() {
        CheckTextSettings();
    }

    private void CheckTextSettings() {
        BabelTranslator tr = FindObjectOfType<BabelTranslator>();
        if(tr != null)
            SetParameters(tr);
        if(textScanSettings == null) {
            Debug.LogWarning("Using default text scan settings since none were available. To specify custom settings, use a Babel Translator object or go to a scene that already has one.");
            textScanSettings = new BabelTextSettings();
        }
    }

    private void OnDestroy() {
        ExitSavePrompt();
    }



    /* =========================================================================================
     * ========================================= GUI ===========================================
     * =========================================================================================
     */

    private void OnGUI() {
        windowWidth = (int)position.width;
        windowHeight = (int)position.height;
        FileGUI();

        if(keyText == null || key == null) {
            keyText = null;
            key = null;
            CreateKeyGUI();
        }
        else if(editingKey != null) {
            EditSingleKeyGUI();
        }
        else
            EditKeyGUI();
    }

    private void FileGUI() {
        backgroundColor = GUI.backgroundColor;
        GUILayout.Space(5);
        object newKey = EditorGUILayout.ObjectField("Translation key: ", keyText, typeof(TextAsset), false);
        GUILayout.BeginHorizontal();
        GUILayout.Space(LEFT_WIDTH + BUTTON_INDENT); 

        if(newKey != null) {
            if(dirty)
                GUI.backgroundColor = NEGATIVE_SAVE_COLOR;
            if(GUILayout.Button("Create new key"))
                newKey = null;
        }
        if(newKey != null && keyText != null) {
            if(dirty)
                GUI.backgroundColor = NEGATIVE_SAVE_COLOR;
            if(GUILayout.Button("Reload key")) {
                if(!dirty || EditorUtility.DisplayDialog(
                    "Discard changes",
                    "Reloading the translation key will discard your working changes. Are you sure you want to do this?",
                    "Reload key",
                    "Cancel")) {
                    key = new BabelTranslationKey(keyText);
                    dirty = false;
                }
            }

            if(dirty)
                GUI.backgroundColor = POSITIVE_SAVE_COLOR;
            if(GUILayout.Button("Save key"))
                SaveTranslationKey();
        }
        GUI.backgroundColor = backgroundColor;
        GUILayout.Space(18);
        GUILayout.EndHorizontal();

        DoFileSaveDialoguesIfChanged(newKey); 
    }

    private void DoFileSaveDialoguesIfChanged(object newKey) {
        if(newKey == null) {
            int saveResponse = SavePrompt();
            if(saveResponse == 0) {
                SaveTranslationKey();
                if(dirty)
                    return;
            }
            if(saveResponse == 1)
                return;
            keyText = null;
        }

        else if(newKey != (object)keyText) {
            int saveResponse = SavePrompt();
            if(saveResponse == 0) {
                SaveTranslationKey();
                if(dirty)
                    return;
            }
            if(saveResponse == 1)
                return;

            LoadKey(newKey);
        }
    }

    private void LoadKey(object newKey) {
        keyText = (TextAsset)newKey;

        try {
            //try to parse given text asset
            key = new BabelTranslationKey(keyText);
        }
        catch(BabelTranslationKey.TranslationKeyParseException e) {
            //Default to create gui if failed
            keyText = null;
            Debug.LogError(e);
        }
    }

    private void CreateKeyGUI() {
        EditorGUILayout.LabelField("Please assign a key to edit or create a new one.");
        GUILayout.Space(5);


        GUILayout.Label("Generate new translation key", EditorStyles.boldLabel);
        newKeyLanguage = EditorGUILayout.TextField("Key language: ", newKeyLanguage);
        if(GUILayout.Button("Create")) {
            string[] keyAssets;
            int index = 0;
            string assetName;
            do {
                assetName = GetIndexedKeyName(index++);
                keyAssets = AssetDatabase.FindAssets(assetName);
            }
            while(keyAssets.Length > 0);

            key = new BabelTranslationKey(newKeyLanguage);
            keyText = new TextAsset();
            AssetDatabase.CreateAsset(keyText, "Assets/" + assetName + ".csv");
            SaveTranslationKey();
        }
    }

    private void EditKeyGUI() {
        GUILayout.Label("");
        
        GUILayout.Label("String search utility", EditorStyles.boldLabel);
        GUILayout.BeginHorizontal();
        GUILayout.Space(LEFT_WIDTH + BUTTON_INDENT);
        if(GUILayout.Button("Collect text from current scene")) {
            CheckTextSettings();
            CollectTextFromScene();
        }
        if(GUILayout.Button("Collect text from all scenes")) {
            CheckTextSettings();
            CollectTextFromAllScenes();
        }
        GUILayout.Space(RIGHT_SPACING + BUTTON_INDENT);
        GUILayout.EndHorizontal();

        GUILayout.Space(VERTICAL_SPACING);
        KeyListHeaderGUI();
        KeyListGUI();
    }


    private void KeyListHeaderGUI() {

        GUILayout.Label("Translation key", EditorStyles.boldLabel);

        //AutoTranslateGUI();
        AddLanguageGUI();
        AddBaseLanguageGUI();
        LanguageHeaderGUI();

    }

    private void AutoTranslateGUI() {
        GUILayout.BeginHorizontal();
        GUILayout.Space(LEFT_WIDTH + BUTTON_INDENT);
        if(GUILayout.Button("Auto translate", GUILayout.Width(RIGHT_WIDTH))) {
            if(!EditorUtility.DisplayDialog("Confirm auto translation",
                "The translator will try to connect to the internet " +
                "to request automatic translations for empty entries in the translation key.",
                "ok", "cancel"))
                return;
            List<string> toTranslate = new List<string>() { "Hello" };
            BabelAutoTranslator t = new BabelAutoTranslator(toTranslate, "en", "nl");
        }
        GUILayout.EndHorizontal();
    }

    private void AddLanguageGUI() {
        GUILayout.BeginHorizontal();
        GUILayout.BeginVertical(GUILayout.Width(LEFT_WIDTH));
        GUILayout.Space(4);
        newLanguage = EditorGUILayout.TextField(newLanguage);
        GUILayout.EndVertical();
        GUILayout.BeginHorizontal(GUILayout.Width(RIGHT_WIDTH));
        if(GUILayout.Button("New language"))
            dirty |= key.AddLanguage(newLanguage);
        GUILayout.EndHorizontal();
        GUILayout.EndHorizontal();
    }

    private void AddBaseLanguageGUI() {
        CheckTextSettings();
        if(textScanSettings.RequiresBaseLanguage()) {
            GUILayout.BeginHorizontal();
            GUILayout.Space(LEFT_WIDTH + BUTTON_INDENT);
            if(GUILayout.Button("Insert base language")) {
                key.InsertBaseLanguage(textScanSettings);
                dirty = true;
            }
            if(GUILayout.Button("Remake base language")) {
                key.RemakeBaseLanguage(textScanSettings);
                dirty = true;
            }
            GUILayout.EndHorizontal();
        }
    }

    private void LanguageHeaderGUI() {
        GUILayout.BeginHorizontal();
        GUILayout.BeginVertical(GUILayout.Width(LEFT_WIDTH));
        GUILayout.Space(2);
        dirty |= key.SetKeyLanguage(EditorGUILayout.TextField(key.GetKeyLanguage()));
        GUI.backgroundColor = DELETE_COLOR;
        int n = key.translationCount;

        if(n > 0 && GUILayout.Button("X")) {
            if(EditorUtility.DisplayDialog("Confirm translation key clearing",
                "You are about to remove all the content of this translation key. " +
                "Are you sure you want to do this?",
                "Clear key", "Cancel")) {
                dirty |= key.Clear();
            }
        }
        GUI.backgroundColor = backgroundColor;
        GUILayout.EndVertical();

        List<string> languages = key.GetLanguages();
        GUILayout.BeginHorizontal(GUILayout.Width(RIGHT_WIDTH));
        for(int i = 0; i < languages.Count; i++)
            LanguageFieldGUI(i, languages[i]);
        GUILayout.EndHorizontal();
        GUILayout.EndHorizontal();
    }

    private void LanguageFieldGUI(int index, string language) {
        GUILayout.BeginVertical();
        dirty |= key.SetLanguage(index, EditorGUILayout.TextField(language));
        GUI.backgroundColor = DELETE_COLOR;
        if(GUILayout.Button("X")) {
            int n = key.translationCount;
            if(n <= 0 || EditorUtility.DisplayDialog("Confirm language deletion",
                "Are you sure you want to delete the language \"" + language +
                "\" from the translation key? " +
                "All translations entries for this language will be removed!",
                "Delete", "Cancel")) {

                key.RemoveLanguage(index);
                dirty = true;
            }
        }
        GUI.backgroundColor = backgroundColor;
        GUILayout.EndVertical();
    }

    private void KeyListGUI() {
        GUILayout.Space(5);
        GuiLine(1, Color.black);
        int height = windowHeight - KEY_LIST_VERTICAL;
        GUILayout.Space(0);
        scrollViewPosition = GUILayout.BeginScrollView(scrollViewPosition, GUILayout.Width(SCROLL_WIDTH), GUILayout.Height(height));
        List<string> keys = key.GetKeys();
        if(keys.Count == 0)
            GUILayout.Label("No translations available!");
        else {
            float scrollY = scrollViewPosition.y;
            for(int i = 0; i < keys.Count; i++){
                if((i + 1) * SCROLL_SPACING >= scrollY && i * SCROLL_SPACING <= scrollY + height)
                    KeyLineGUI(keys[i]);
                else
                    GUILayout.Space(SCROLL_SPACING);
            }
        }

        GUILayout.BeginHorizontal();
        GUILayout.BeginVertical(GUILayout.Width(LEFT_WIDTH));
        GUILayout.Space(4);
        newKey = EditorGUILayout.TextField(newKey);
        GUILayout.EndVertical();
        if(GUILayout.Button("New translation"))
            dirty |= key.AddManual(newKey);
        GUILayout.EndHorizontal();
        GUILayout.EndScrollView();
        GuiLine(1, Color.black);
    }

    private void KeyLineGUI(string keyString) {
        int n = key.GetLanguages().Count;

        GUILayout.BeginHorizontal();
        GUIStyle keyLabel = new GUIStyle(GUI.skin.button);
        keyLabel.normal.textColor = NOREF_COLOR;
        if(key.IsNew(keyString))
            keyLabel.normal.textColor = NEW_COLOR;
        else if(key.HasReferences(keyString))
            keyLabel.normal.textColor = Color.black;
        keyLabel.fixedWidth = LEFT_WIDTH;
        keyLabel.padding = new RectOffset(5, 3, 5, 0);
        keyLabel.clipping = TextClipping.Clip;
        keyLabel.alignment = TextAnchor.UpperLeft;

        GUIContent buttonContent = new GUIContent(keyString, key.GetLabel(keyString));
        if(GUILayout.Button(buttonContent, keyLabel))
            editingKey = keyString;

        GUI.backgroundColor = backgroundColor;
        List<string> translations = key.GetTranslations(keyString);
        for(int i = 0; i < n; i++)
            EditSingleTranslationGUI(translations, i, keyString);
        GUI.backgroundColor = backgroundColor;
        GUILayout.EndHorizontal();
    }

    private void EditSingleKeyGUI() {
        if(!key.HasKey(editingKey)) {
            editingKey = null;
            return;
        }
        //2 main panels
        GUILayout.BeginHorizontal();

        //access panel
        GUILayout.BeginVertical();

        //main label
        GUIStyle bold = new GUIStyle();
        bold.fontStyle = FontStyle.Bold;
        GUILayout.Label(editingKey, bold);

        //change key input
        GUILayout.BeginHorizontal();
        GUIContent changeKeyContent = new GUIContent("Change key", "This will change known occurences in the current scene and adapt the key value when the translation key is next saved.");
        GUILayout.Label(changeKeyContent);
        key.ChangeKey(editingKey, EditorGUILayout.TextField(key.GetKeyChange(editingKey)));
        GUILayout.EndHorizontal();

        //controls buttons
        GUILayout.BeginHorizontal();

        GUI.backgroundColor = DELETE_COLOR;
        if(GUILayout.Button("Delete")) {
            key.RemoveKey(editingKey);
            editingKey = null;
            return;
        }
        GUI.backgroundColor = backgroundColor;

        GUILayout.Space(50f);
        if(GUILayout.Button("OK")) {
            editingKey = null;
            return;
        }

        GUILayout.EndHorizontal();
        
        List<string> languages = key.GetLanguages();
        int n = languages.Count;
        List<string> translations = key.GetTranslations(editingKey);
        for(int i = 0; i < n; i++) {
            GUILayout.BeginHorizontal();
            GUILayout.Label(languages[i]);
            EditSingleTranslationGUI(translations, i, editingKey);
            GUILayout.EndHorizontal();
        }
        
        GUI.backgroundColor = backgroundColor;

        //end access panel
        GUILayout.EndVertical();

        //begin info panel
        GUILayout.BeginVertical();

        //info dispay
        GUILayout.Label(key.GetLabel(editingKey));
        
        //end info panel
        GUILayout.EndVertical();

        //done
        GUILayout.EndHorizontal();
    }

    private void EditSingleTranslationGUI(List<string> translations, int i, string keyString) {
        if(i < translations.Count) {
            if(translations[i].Contains("\n"))
                dirty |= key.SetTranslation(keyString, i, EditorGUILayout.TextField(translations[i], GUILayout.Height(FIELD_HEIGHT)));
            else {
                if(string.IsNullOrEmpty(translations[i]))
                    GUI.backgroundColor = EMPTY_COLOR;
                dirty |= key.SetTranslation(keyString, i, EditorGUILayout.TextField(translations[i]));
            }
            GUI.backgroundColor = backgroundColor;
        }
        else
            key.SetTranslation(keyString, i, EditorGUILayout.TextField(""));
    }



    /* =========================================================================================
     * ===================================== GUI Helpers =======================================
     * =========================================================================================
     */

    private int SavePrompt() {
        if(key == null || keyText == null)
            dirty = false;
        if(!dirty)
            return 2; //default to 'discard'
        int toReturn = EditorUtility.DisplayDialogComplex(
            "Save translation key",
            "Current translation key has unsaved changes, would you like to save them?",
            "Save",
            "Cancel",
            "Discard changes");
        dirty = toReturn == 1;
        return toReturn;
    }

    private void ExitSavePrompt() {
        if(dirty && EditorUtility.DisplayDialog(
            "Save translation key",
            "Current translation key has unsaved changes, would you like to save them?",
            "Save",
            "Discard changes"))
            SaveTranslationKey();
    }

    private void GuiLine(int thickness, Color color) {
        Rect rect = EditorGUILayout.GetControlRect(false, thickness);
        rect.height = thickness;
        EditorGUI.DrawRect(rect, color);
    }

    private void SaveTranslationKey() {
        string keyPath = AssetDatabase.GetAssetPath(keyText);
        try {
            File.WriteAllText(keyPath, key.Save());
        }
        catch(Exception e) {
            EditorUtility.DisplayDialog("Saving error",
                "Translation key could not be saved due to an exception:\n" + e.GetType() + 
                "\nDetails were printed to the editor console.", 
                "OK");
            Debug.LogError(e);
            dirty = true;
            return;
        }
        EditorUtility.SetDirty(keyText);
        AssetDatabase.Refresh();
        keyText = AssetDatabase.LoadAssetAtPath<TextAsset>(keyPath);
        dirty = false;
    }

    private string GetIndexedKeyName(int index) {
        if(index <= 0)
            return "TranslationKey";
        else
            return "TranslationKey_" + index;
    }





    /* =========================================================================================
     * =========================== Text collection functionality ===============================
     * =========================================================================================
     */


    private void CollectTextFromAllScenes() {
        List<string> sceneFiles = new List<string>();
        foreach(EditorBuildSettingsScene scene in EditorBuildSettings.scenes)
            sceneFiles.Add(scene.path);

        int nboScenes = sceneFiles.Count;
        bool sceneDirty = SceneManager.GetActiveScene().isDirty;

        if(sceneDirty) {
            int response = EditorUtility.DisplayDialogComplex("Confirm text collection",
            "The translator wizard is about to collect text data from " +
            nboScenes + " different scenes. Would you like to save changes to the current " +
            "scene before continuing?",
            "Save and continue", "Discard changes", "Cancel");
            if(response == 2)
                return;
            else if(response == 0)
                EditorSceneManager.SaveScene(SceneManager.GetActiveScene());
        }
        else if(!EditorUtility.DisplayDialog("Confirm text collection",
            "The translator wizard is about to collect text data from " +
            nboScenes + " different scenes. ",
            "Continue", "Cancel"))
            return;

        string startScene = SceneManager.GetActiveScene().name;
        string startSceneFile = null;

        for(int i = 0; i < sceneFiles.Count; i++) {
            string sceneName = Path.GetFileNameWithoutExtension(sceneFiles[i]);
            if(sceneName.Equals(startScene, StringComparison.OrdinalIgnoreCase))
                startSceneFile = sceneFiles[i];
            string title = "Searching for text fields";
            float progress = (float)i / nboScenes;
            string info = "Loading text from " + sceneName;
            if(EditorUtility.DisplayCancelableProgressBar(title, info, progress)) {
                EditorUtility.ClearProgressBar();
                return;
            }
            LoadSceneAndCollectText(sceneFiles[i]);
        }

        EditorUtility.ClearProgressBar();
        EditorSceneManager.OpenScene(startSceneFile, OpenSceneMode.Single);
    }

    private void LoadSceneAndCollectText(string sceneFile) {
        try {
            string sceneName = Path.GetFileNameWithoutExtension(sceneFile);
            EditorSceneManager.OpenScene(sceneFile, OpenSceneMode.Single);
        }catch(Exception) {
            EditorUtility.ClearProgressBar();
            return;
        }
        CollectTextFromScene();
        EditorSceneManager.SaveScene(SceneManager.GetActiveScene());
    }

    private void CollectTextFromScene() {
        BabelTranslator translator = FindObjectOfType<BabelTranslator>();
        GameObject g = null;

        if(!translator) {
            g = new GameObject("Translator");
            translator = g.AddComponent<BabelTranslator>();
            translator.textScanSettings = textScanSettings;
        }
        
        if(string.IsNullOrEmpty(translator.startingLanguage))
            translator.startingLanguage = key.GetKeyLanguage();
        if(translator.translationKey == null)
            translator.translationKey = keyText;

        int count = key.translationCount;
        key.AddKeys(translator.GetKeyStringsFromScene());
        dirty |= key.translationCount > count;

        if(g != null)
            DestroyImmediate(g);
    }


}
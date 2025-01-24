using UnityEngine;
using UnityEngine.SceneManagement;

public class BTK_Example_MenuScript : MonoBehaviour {

    public void LaunchGame() {
        SceneManager.LoadScene(1);
    }

    public void Translate(string language) {
        BabelTranslator.TranslateScene(language);
    }

    public void Translate(int language) {
        string languageString = BabelTranslator.GetAllLanguages()[language];
        BabelTranslator.QuickRefresh();
        BabelTranslator.TranslateScene(languageString);
    }
}

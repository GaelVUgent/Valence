using UnityEngine;
using UnityEngine.UI;

using static BabelTranslator;

public class BTL_Example_GameScript : MonoBehaviour {

    public Text text;

    public void UpdateText(bool state) {
        if(state)
            text.text = "Correct!";
        else
            text.text = "False!";
    }

    public void UpdateTextTranslated(bool state) {
        if(state)
            text.text = Tr("Correct!");
        else
            text.text = Tr("False!");
    }
}

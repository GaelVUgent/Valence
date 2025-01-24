using UnityEngine;
using UnityEngine.UI;

using static BabelTranslator;

public class BTT_Example_GameScript : MonoBehaviour {

    public Text text;

    public void UpdateText(bool state) {
        if(state)
            text.text = Tr("Correct!");
        else
            text.text = Tr("False!");
    }

    public void UpdateTextTranslated(bool state) {
        if(state)
            text.text = Tr("Correct!");
        else
            text.text = Tr("False!");
    }
}

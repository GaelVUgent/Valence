using MICT.eDNA.Controllers;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public abstract class WizardEvent : MonoBehaviour
{
    //Text die initieel op de button moet staan
    public string _Name = "Click Me!";
    //Wrapper voor de unity button class. Gebruik WizardEventButton.Text om de text aan te passen en WizardEventButton.ButtonColor om de kleur van de knop aan te passen.
    [HideInInspector] public WizardEventButton _Button = null;
    //Wether the button clicks should be added to the Scene Controls Graph and a bool to indicate wether the value is recording
    [SerializeField] private bool _addToDefaultGraph = true;
    private bool _isInteracting = false;
    //Variabele om te bepalen of we dit event wegschrijven naar de output
    [SerializeField] private bool _writeToOutput = true;
    public bool WriteToOutput { get { return _writeToOutput; } }

    //In de Startfunctie doen we een call naar de WizardEventHandler om onze button aan te maken en daarna zorgen we dat OnClick wordt uitgevoerd telkes er 
    //op de knop wordt gedrukt.
    //Indien je in een overervende class een Startfunctie wil gebruiken, begin deze dan met 'base.Start();' anders zal deze Startfunctie niet worden aangeroepen.
    protected void Start()
    {
        if (_Button == null)
            _Button = WizardEventHandler.INSTANCE.RegisterWizardEvent(this);
        _Button._OnButtonClick += OnClick;
        if (_addToDefaultGraph)
        {
            OutputController.INSTANCE.AddSceneControlsGraphValue(this, _isInteracting);
            _Button._OnButtonClick += AddValueToSceneControlsGraph;
        }
    }

    private void AddValueToSceneControlsGraph()
    {
        _isInteracting = !_isInteracting;
        OutputController.INSTANCE.AddSceneControlsGraphValue(this, _isInteracting);
    }

    //abstracte functie voor wanneer de speler op de knop drukt. Overervende class implementeert deze als volgt: public override void OnClick(){}
    public abstract void OnClick();
}

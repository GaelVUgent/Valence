using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WizardEventHandler : MonoBehaviour
{
    [SerializeField] private WizardEventButton _buttonPrefab;
    [SerializeField] private Transform _buttonParent;

    public static WizardEventHandler INSTANCE { get; private set; } = null;
    public static List<WizardEvent> _RegisteredEvents = new List<WizardEvent>();

    private void Awake()
    {
        INSTANCE = this;
    }

    public WizardEventButton RegisterWizardEvent(WizardEvent we)
    {
        //Handle output registering
        if (we.WriteToOutput)
        {
            if (!_RegisteredEvents.Contains(we))
            {
                _RegisteredEvents.Add(we);
                //TODO: readd this
                //OutputService.AddWizardEvent(we);
            }
        }
        //Create Button
        WizardEventButton btn = Instantiate(_buttonPrefab, _buttonParent);
        btn.Text = we._Name;
        return btn;
    }

    public WizardEventButton RegisterWizardEvent(WizardEvent we, WizardEventButton customButton)
    {
        //Handle output registering
        if (we.WriteToOutput)
        {
            if (!_RegisteredEvents.Contains(we))
            {
                //TODO: readd this
                _RegisteredEvents.Add(we);
                //OutputService.AddWizardEvent(we);
            }
        }
        //Create Button
        WizardEventButton btn = Instantiate(customButton, _buttonParent);
        return btn;
    }
}

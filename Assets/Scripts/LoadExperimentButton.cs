using MICT.eDNA.Managers;
using UnityEngine;

public class LoadExperimentButton : MonoBehaviour
{

    public int environmentScene;
    public int experimentScene;
    public KeyCode hotkey = KeyCode.None;

    private void Update()
    {
        if (hotkey != KeyCode.None && Input.GetKeyDown(hotkey))
            Load();
    }

    public void Load()
    {
        Global.ReturnToMenu();
        if (environmentScene >= 0)
            Global.LoadScene(environmentScene);
        Global.LoadScene(experimentScene);

        if (ServiceLocator.UserService.CurrentUser != null)
        {
            ServiceLocator.UserService.OverrideParticipantNumber(ServiceLocator.UserService.CurrentUser.ParticipantNumber + 1);
        }
        ServiceLocator.DataService.StartOutput(true);
    }
}

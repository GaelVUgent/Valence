using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// provides basic controls for the app. Main menu should be in the starting 
/// scene. the object remains loaded, but should be disabled when moving to 
/// an assembly scene. this is handled by the Global script.
/// </summary>
public class MainMenu : MonoBehaviour
{

    public InputField participantNumberInput;
    public Text hrmStatusText;

    public void Quit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.ExitPlaymode();
#else
        Application.Quit();
#endif
    }

    private void Update()
    {
        //control scene buttons via keyboard for easier debugging while wearing a VR set
        if (Input.GetKeyDown(KeyCode.Escape))
            Global.ReturnToMenu();
        if (Input.GetKeyDown(KeyCode.H))
            FindHRM();
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += SceneManager_sceneLoaded;
        participantNumberInput.text = Global.participantNumber.ToString();
        if (MICT.eDNA.Managers.ServiceLocator.UserService != null)
        {
            if (MICT.eDNA.Managers.ServiceLocator.UserService.CurrentUser != null)
            {
                Global.participantNumber = MICT.eDNA.Managers.ServiceLocator.UserService.CurrentUser.ParticipantNumber;
                participantNumberInput.text = Global.participantNumber.ToString();
            }
        }
    }

    

    private void OnDisable()
    {
        ApplyInput();
        SceneManager.sceneLoaded -= SceneManager_sceneLoaded;
    }

    /// <summary>
    /// Propagate values of UI input elements
    /// </summary>
    public void ApplyInput()
    {
        Global.UpdateParticipantNumber(int.Parse(participantNumberInput.text));
    }

    /// <summary>
    /// Politely ask windows to open the folder containing saved experiment data
    /// </summary>
    public void OpenDataFolder()
    {
        System.Diagnostics.Process p = new System.Diagnostics.Process();
        string path = ExperimentData.GetDataFolder().Replace('/', '\\');
        string explorer = "explorer.exe";
        p.StartInfo = new System.Diagnostics.ProcessStartInfo(explorer, path);
        p.Start();

    }

    /// <summary>
    /// Connect HRM to display text in the main menu,
    /// to help user see when it is running.
    /// </summary>
    public void FindHRM()
    {
        FindObjectOfType<HRM>().Connect(hrmStatusText);
    }

    private void SceneManager_sceneLoaded(Scene arg0, LoadSceneMode arg1)
    {
        Global.participantNumber = MICT.eDNA.Managers.ServiceLocator.UserService.CurrentUser.ParticipantNumber;
        participantNumberInput.text = Global.participantNumber.ToString();
    }
}

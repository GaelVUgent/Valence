using MICT.eDNA.Managers;
using MICT.eDNA.Models;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Script to handle shared settings and functionality troughout all scenes.
/// This structure has been rendered somewhat obsolute due to the additive 
/// scene loading, but it's preserved this way, just in case this changes in the future.
/// </summary>
public class Global : MonoBehaviour
{

    //singleton structure
    public static Global global;

    //experiment flow data
    public ExperimentFlow flow;
    public TextAsset flowSource;

    //scene connections
    public GameObject mainMenuRig;
    private static List<int> loadedScenes;
    private ServiceLoader serviceLoader;

    //settings
    public static int participantNumber;

    private void Awake()
    {
        if (global == null)
        {
            global = this;
            DontDestroyOnLoad(gameObject);
            SceneManager.sceneLoaded += OnSceneLoad;
        }
        else if (global != this)
        {
            Destroy(gameObject);
            return;
        }

        //set defaults
        participantNumber = -1;

        serviceLoader = GetComponent<ServiceLoader>();
        serviceLoader.OnServicesLoaded += OnServiceLoad;
    }

    private void Start()
    {
        loadedScenes = new List<int>();
        LoadFlowFile();
    }

    private void OnServiceLoad(object sender, System.EventArgs e)
    {
        ServiceLocator.DataService.OnDataLoaded += OnDataLoaded;
        //needed because data-event could already've be dispatched before this method was even called
        if (ServiceLocator.DataService.Data != null)
        {
            OnDataLoaded(this, null);
        }
        if (participantNumber > 0)
            ServiceLocator.UserService.OverrideParticipantNumber(participantNumber);

        // ServiceLocator.NetworkService.Connect();
        ServiceLocator.UserService.AssignUserRole(UserRole.DefaultVR);
    }

    private void OnDataLoaded(object sender, System.EventArgs e)
    {
        ServiceLocator.ExperienceService.SetData(ServiceLocator.DataService.Data, true);
        Debug.Log("eDNA data ready");
    }

    public static void UpdateParticipantNumber(int participantNumber)
    {
        Global.participantNumber = participantNumber;
        if (participantNumber > 0)
            ServiceLocator.UserService?.OverrideParticipantNumber(participantNumber);
    }

    public void LoadFlowFile()
    {
        if (flowSource == null)
            return;
        flow = JsonUtility.FromJson<ExperimentFlow>(flowSource.text);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (!InMainMenu())
                ReturnToMenu();
        }
    }

    private bool InMainMenu()
    {
        return loadedScenes.Count == 0;
    }

    public static void ReturnToMenu()
    {
        UnloadAdditiveScenes();
        global.mainMenuRig.SetActive(true);
    }

    public static void LoadScene(int buildIndex)
    {
        if (!loadedScenes.Contains(buildIndex))
            SceneManager.LoadSceneAsync(buildIndex, LoadSceneMode.Additive);
        global.mainMenuRig.SetActive(false);
    }

    private static void UnloadAdditiveScenes()
    {
        foreach (int s in loadedScenes)
            SceneManager.UnloadSceneAsync(s);
        loadedScenes.Clear();
    }

    private void OnSceneLoad(Scene scene, LoadSceneMode mode)
    {
        if (scene.buildIndex > 0 & mode == LoadSceneMode.Additive)
            loadedScenes?.Add(scene.buildIndex);
    }
}



#if UNITY_EDITOR
[UnityEditor.CustomEditor(typeof(Global))]
public class GlobalEditor : UnityEditor.Editor
{

    private const string GAME_DATA_PATH = "Assets/";

    public override void OnInspectorGUI()
    {
        Global g = (Global)target;

        DrawDefaultInspector();

        //editor button to export flow data after manually editing it
        if (GUILayout.Button("Export flow data"))
        {
            string text = JsonUtility.ToJson(g.flow);
            string path = GAME_DATA_PATH + "Other/ExperimentFlow.json";
            File.WriteAllText(path, text);
        }


        //reload changed flow file to continue editing it
        if (GUILayout.Button("Import flow data"))
        {
            g.LoadFlowFile();
        }
    }
}
#endif

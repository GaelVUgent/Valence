using MICT.eDNA.Models;
using Newtonsoft.Json;
using System;
using System.IO;
using UnityEngine;

public class OutputService
{
    private  string _fileName = "data";
    private  string _folderName = "Output";
    private  string _folderPath;
    private  string _excelExtension = ".csv";
    private  string _startDateTime;
    private  string _filePath { get { return $"{_folderPath}/{_fileName}_{_startDateTime}{_excelExtension}"; } }
    private  string _jsonFilePath { get { return $"{_folderPath}/{_fileName}_{_startDateTime}.json"; } }
    public  SingleOutputFrame Output = new SingleOutputFrame();
    private static  StreamWriter _writer;
    private static StreamWriter _jSONWriter;
    public static float NULLEYEFLOATVALUE = -100;
    public static Vector3 NULLEYEVECTOR3VALUE = new Vector3(-100, -100, -100);


    public void Initialize()
    {
        _folderPath = $"{Application.streamingAssetsPath}/{_folderName}";
        _startDateTime = DateTime.Now.ToString(@"yyyy-MM-dd-HH-mm");
        //Check if folder exists
        if (!Directory.Exists(_folderPath))
            Directory.CreateDirectory(_folderPath);
        //Create File
        _writer = File.CreateText(_filePath);
        //Write first line
        _writer.Write($"Absolute Time;Relative Time;;Experience Data;;;;;;;;;;;Interactions;;;HCCI interactions;;;;PhysioData;;;;;;;;Wizard interactions;{Environment.NewLine}");
        //Write second line
        _writer.Write($";;" +
            $";Participant Number;Block ID;Block Name;Trial ID;Trial Name;Active Conditions (ID);Active Conditions (Name);Action ID;Action Name;Action Description;;" +
            $"Looking at;Grasping;Interacting with;" +
            $"User to Object;User to User;User to Content;User to Context;" +
            $"Pupil Diameter Left;Pupil Diameter Right;Pupil Coordinate Left;Pupil Coordinate Right; Eye Openess Left;Eye Openess Right;Heart Rate;");

        //Create JSON file
        _jSONWriter = File.CreateText(_jsonFilePath);
    }

    public void AddWizardEvent(WizardEvent we)
    {
        /*File.AppendAllText(FilePath,*/
        _writer.Write($";{we._Name}");
    }

    [Obsolete("Not used, see BaseDataService for implementation")]
    public void WriteOutput()
    {
        //TODO: who added the return?
        //this code causes problems on Windows and is no longer required
        //return;

        if (_writer == null)
            return;

        var conditionIds = Output.CurrentActiveConditionIds?.Length > 0 ? Output.CurrentActiveConditionIds[0].ToString() : "";
        var conditionNames = Output.CurrentActiveConditionNames?.Length > 0 ? Output.CurrentActiveConditionNames[0] : ""; ;

        if (Output.CurrentActiveConditionIds?.Length > 1)
        {
            for (int i = 1; i < Output.CurrentActiveConditionIds?.Length; i++)
            {
                conditionIds += ", " + Output.CurrentActiveConditionIds[i];
            }
        }
        if (Output.CurrentActiveConditionNames?.Length > 1)
        {
            for (int i = 1; i < Output.CurrentActiveConditionNames?.Length; i++)
            {
                conditionNames += ", " + Output.CurrentActiveConditionNames[i];
            }
        }

        string line = string.Concat(Environment.NewLine,
            Output.TimeNow.ToString(@"HH\:mm\:ss\:fff"), ";",
            Output._Time.ToString(@"hh\:mm\:ss\:fff"), ";;",
            Output.ParticipantNumber, ";",
            Output.ParticipantName, ";",
            Output.CurrentBlockId, ";",
            Output.CurrentBlockName, ";",
            Output.CurrentTrialId, ";",
            Output.CurrentTrialName, ";",
            conditionIds, ";",
            conditionNames, ";",
            Output.CurrentActionId, ";",
            Output.CurrentActionName, ";",
            Output.CurrentActionDescription, ";;",
            Output._LookingAt, ";",
            Output._Grasping, ";",
            Output._InteractingWith, ";",
            Output._UserToObject, ";",
            Output._UserToUser, ";",
            Output._UserToContent, ";",
            Output._UserToContext, ";",
            Output._PupilDiameterLeft, ";",
            Output._PupilDiameterRight, ";",
            Output._PupilCoordinateLeft, ";",
            Output._PupilCoordinateRight, ";",
            Output._EyeOpenessLeft, ";",
            Output._EyeOpenessRight, ";",
            Output._HeartRate, ";"
            );

        for (int i = 0; i < WizardEventHandler._RegisteredEvents.Count; i++)
        {
            line = string.Concat(line, ";", Output._SceneControlInteractionsDictionary[WizardEventHandler._RegisteredEvents[i]._Name]);
        }

        //File.AppendAllText(FilePath, line);
        _writer.Write(line);

        //JSON
        _jSONWriter.Write(JsonConvert.SerializeObject(Output, Formatting.None, new SingleOutputFrameConverter()));
        _jSONWriter.Write(Environment.NewLine);
    }

    public static void CloseStream()
    {
        try
        {
            _writer.Close();
            _jSONWriter.Close();
        }
        catch (Exception e)
        {
            Debug.LogError("Error trying to close stream to save save output: " + e.Message);
        }
    }

    public  void ReOpenStream()
    {
        _writer?.Close();
        _writer = new StreamWriter(_filePath);

        _jSONWriter?.Close();
        _jSONWriter = new StreamWriter(_jsonFilePath);
    }
}


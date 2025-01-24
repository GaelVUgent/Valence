using MICT.eDNA.Models;
using MICT.eDNA.Interfaces;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Net;
using UnityEngine;
using UnityEngine.Networking;
using System.Collections.Generic;
using MICT.eDNA.Managers;
using MICT.eDNA.Controllers;

namespace MICT.eDNA.Services
{
    public abstract class BaseDataService : IDataService
    {
        public event EventHandler OnDataLoaded;

        protected Experiment _data;
        public Experiment Data
        {
            get
            {
                return _data;
            }
            set
            {
                if (_data == value)
                    return;
                _data = value;
                _data.LinkIds();
            }
        }

        protected Experience _dataStructure;
        public Experience DataStructure
        {
            get
            {
                return _dataStructure;
            }
            set
            {
                if (_dataStructure == value)
                    return;
                _dataStructure = value;
                AddStructureToOutput(_dataStructure);
                _dataStructure.LinkIds();
            }
        }

        //TODO: api later used to post to website where data will be shown
        protected virtual string _api
        {
            get
            {
                return "http://10.0.2.2:60962/api/";
            }
        }

        protected const string _authorization = "b221b435-de79-4335-9540-ba6478ed4783";
        protected const string _dataFolderUri = "data";
        protected string _dummyFolderPath = "dummy/", _dummyDataFileName = "data.json", _structureFolderPath = "dummy/", _structureDataFileName = "data_structure.json";

        protected bool _useStreamingAssetsFolder = true;
        protected MonoBehaviour _monoBehaviour;

        //OUTPUT
        private string _fileName = "data";
        private string _folderName = "Output";
        private string _folderPath;
        private string _excelExtension = ".csv";
        private string _startDateTime;
        private int _participantNumber;
        private string _filePath { get { return $"{_folderPath}/{_participantNumber.ToString("D3")}_{_startDateTime}{_excelExtension}"; } }
        protected string _jsonFilePath { get { return $"{_folderPath}/{_participantNumber.ToString("D3")}_{_startDateTime}.json"; } }
        public SingleOutputFrame CurrentOutputFrame = new SingleOutputFrame();
        protected static StreamWriter _writer;
        protected static StreamWriter _jSONWriter;
        protected bool _hasAddedStructureToWriter = false, _hasAddedFirstLineToOutputFrames = false;
        protected bool _hasWriterBeenInitialised = false;

        //TODO: remove automaticallyStartWritingOutput parameter
        public virtual void Init(MonoBehaviour behaviour = null, bool useStreamingAssetsFolder = true, bool automaticallyStartWritingOutput = false)
        {
            _monoBehaviour = behaviour;
            _useStreamingAssetsFolder = useStreamingAssetsFolder;
            //check if we have
            FetchData();
        }

        public virtual void StartOutput(bool startNewFile = false)
        {

            InitOutput(startNewFile);
        }

        protected virtual void InitOutput(bool startNewFile = false)
        {
            bool moveExistingFile = false;
            string oldFilePath = _filePath;
            string oldFilePathJson = _jsonFilePath;

            if (_participantNumber == 0 && File.Exists(_filePath))
            {
                //file already exists as default empty thing
                moveExistingFile = true;
            }

            _participantNumber = ServiceLocator.UserService?.CurrentUser?.ParticipantNumber ?? 0;
            _folderPath = $"{Application.streamingAssetsPath}/{_folderName}";
            _startDateTime = DateTime.Now.ToString(@"yyyy-MM-dd-HH-mm");
            //Check if folder exists
            if (!Directory.Exists(_folderPath))
                Directory.CreateDirectory(_folderPath);
            //Create File
            if (moveExistingFile)
            {
                if (File.Exists(_filePath)) {
                    _startDateTime += DateTime.Now.ToString(@"yyyy-MM-dd-HH-mm-ss");
                }
                CloseStream(false);
                File.Move(oldFilePath, _filePath);
                File.Move(oldFilePathJson, _jsonFilePath);
                _writer = File.AppendText(_filePath);
                _jSONWriter = File.AppendText(_jsonFilePath);
            }
            //if (!moveExistingFile)
            //{
            if (!File.Exists(_filePath))
            {
                _writer = File.CreateText(_filePath);
                //Write first line
                _writer.Write($"Absolute Time;Relative Time;;Experience Data;;;;;;;;;;;;;;Interactions;;;HCCI interactions;;;;PhysioData;;;;;;;;;Wizard interactions;{Environment.NewLine}");
                //Write second line
                _writer.Write($";;" +
                    $";Participant Number;Participant Name;Experiment ID;Experiment Name;Block ID;Block Name;Trial ID;Trial Name;Active Conditions (ID);Active Conditions (Name);Action ID;Action Name;Action Description;;" +
                    $"Looking at;Grasping;Interacting with;" +
                    $"User to Object;User to User;User to Content;User to Context;" +
                    $"Pupil Diameter Left;Pupil Diameter Right;Pupil Coordinate Left;Pupil Coordinate Right; Eye Openess Left;Eye Openess Right;Heart Rate;REBA;");

                //Create JSON file
                _jSONWriter = File.CreateText(_jsonFilePath);
            }
            else
            {


                if (_writer == null)
                    _writer = File.AppendText(_filePath);
                if (_jSONWriter == null)
                    _jSONWriter = File.AppendText(_jsonFilePath);
            }
            // }

        }

        public virtual void WriteOutputLine(SingleOutputFrame outputLine)
        {
            if (_writer == null)
                return;

            var conditionIds = outputLine.CurrentActiveConditionIds?.Length > 0 ? outputLine.CurrentActiveConditionIds[0].ToString() : "";
            var conditionNames = outputLine.CurrentActiveConditionNames?.Length > 0 ? outputLine.CurrentActiveConditionNames[0] : ""; ;

            if (outputLine.CurrentActiveConditionIds?.Length > 1)
            {
                for (int i = 1; i < outputLine.CurrentActiveConditionIds?.Length; i++)
                {
                    conditionIds += ", " + outputLine.CurrentActiveConditionIds[i];
                }
            }
            if (outputLine.CurrentActiveConditionNames?.Length > 1)
            {
                for (int i = 1; i < outputLine.CurrentActiveConditionNames?.Length; i++)
                {
                    conditionNames += ", " + outputLine.CurrentActiveConditionNames[i];
                }
            }

            string line = string.Concat(Environment.NewLine,
                outputLine.TimeNow.ToString(@"HH\:mm\:ss\:fff"), ";",
                outputLine._Time.ToString(@"hh\:mm\:ss\:fff"), ";;",
                outputLine.ParticipantNumber, ";",
                outputLine.ParticipantName, ";",
                outputLine.ExperimentNumber, ";",
                outputLine.ExperimentName, ";",
                outputLine.CurrentBlockId, ";",
                outputLine.CurrentBlockName, ";",
                outputLine.CurrentTrialId, ";",
                outputLine.CurrentTrialName, ";",
                conditionIds, ";",
                conditionNames, ";",
                outputLine.CurrentActionId, ";",
                outputLine.CurrentActionName, ";",
                outputLine.CurrentActionDescription, ";;",
                outputLine._LookingAt, ";",
                outputLine._Grasping, ";",
                outputLine._InteractingWith, ";",
                outputLine._UserToObject, ";",
                outputLine._UserToUser, ";",
                outputLine._UserToContent, ";",
                outputLine._UserToContext, ";",
                outputLine._PupilDiameterLeft, ";",
                outputLine._PupilDiameterRight, ";",
                outputLine._PupilCoordinateLeft, ";",
                outputLine._PupilCoordinateRight, ";",
                outputLine._EyeOpenessLeft, ";",
                outputLine._EyeOpenessRight, ";",
                outputLine._HeartRate, ";",
                outputLine.RebaScore, ";"
                );

            for (int i = 0; i < WizardEventHandler._RegisteredEvents.Count; i++)
            {
                line = string.Concat(line, ";", outputLine._SceneControlInteractionsDictionary[WizardEventHandler._RegisteredEvents[i]._Name]);
            }
            if (_writer.BaseStream != null)
            {
                _writer.Write(line);
            }
            else {
                Debug.LogWarning("Trying to write to file but file has been shutdown.");
            }

            //JSON
            if (_hasAddedStructureToWriter)
            {
                //fix trailing comma. gives error in django api
                if (!_hasAddedFirstLineToOutputFrames)
                {
                    _hasAddedFirstLineToOutputFrames = true;
                }
                else {
                    _jSONWriter.Write(",");
                }
                _jSONWriter.Write(JsonConvert.SerializeObject(outputLine, Formatting.None, new SingleOutputFrameConverter()));
                
            }
        }

        public virtual void CloseStream(bool finishFile = true)
        {
            try
            {
                if (_writer.BaseStream != null)
                {
                    _writer.Close();                   
                }
                if (_jSONWriter.BaseStream != null)
                {
                    if (finishFile)
                    {
                        Debug.Log("closing stream");
                        
                        _jSONWriter.WriteLine("],");
                        if (Data != null) {
                            Data.UpdateName();
                        }
                        _jSONWriter.Write("\"ExperimentName\": \"" + (Data?.Name ?? "Untitled") + "\",");
                        _jSONWriter.Write("\"ExperienceId\": \"" + (DataStructure?.Id.ToString() ?? "-1") + "\"}");
                        //resetting for possible new file
                        _hasAddedFirstLineToOutputFrames = false;
                        _hasAddedStructureToWriter = false;
                    }
                    _jSONWriter.Close();
                }
            }
            catch (Exception e)
            {
                Debug.LogError("Error trying to close stream to save output: " + e.Message);
            }
        }

        public void SetCurrentOutputFrame(SingleOutputFrame frame)
        {
            CurrentOutputFrame = frame;
            WriteOutputLine(frame);
        }

        public void SetEndOutput(Output data)
        {
            File.WriteAllText(Path.Combine(Application.streamingAssetsPath, _folderName, $"data_end_{System.DateTime.Now.ToString("yyyyMMddHHmmss")}.json"), JsonConvert.SerializeObject(data, new JsonSerializerSettings() { ReferenceLoopHandling = ReferenceLoopHandling.Ignore }));
        }

        public virtual void SaveDataToLocal(Experiment data, bool writeToPersistent = true, string folderPath = "", string filename = "")
        {
            if (string.IsNullOrEmpty(folderPath))
            {
                folderPath = _dummyFolderPath;              
            }
            if (string.IsNullOrEmpty(filename))
            {
                filename = _dummyDataFileName;                
            }
            var jsonData = JsonConvert.SerializeObject(data, new JsonSerializerSettings() { ReferenceLoopHandling = ReferenceLoopHandling.Serialize });
            if (writeToPersistent)
                WriteTextToPersistentData(jsonData, folderPath, filename);
            else
                WriteTextToStreamingData(jsonData, folderPath, filename);
        }

        public virtual void SaveDataStructureToLocal(Experience data, bool writeToPersistent = true, string folderPath = "", string filename = "")
        {
            if (string.IsNullOrEmpty(folderPath))
            {
                folderPath = _structureFolderPath;
            }
            if (string.IsNullOrEmpty(filename))
            {
                filename = _structureDataFileName;
            }
            var jsonData = JsonConvert.SerializeObject(data, new JsonSerializerSettings() { ReferenceLoopHandling = ReferenceLoopHandling.Serialize, Formatting = Formatting.Indented, Culture = System.Globalization.CultureInfo.InvariantCulture });
            if (writeToPersistent)
                WriteTextToPersistentData(jsonData, folderPath, filename);
            else
                WriteTextToStreamingData(jsonData, folderPath, filename);
        }

        public Models.Action GetActionFromSelector(ActionSelector selector)
        {
            return Data?.AllActions?.SingleOrDefault(x => x.ConfigId == selector.ConfigId);
        }

        public Condition GetConditionFromSelector(ConditionSelector selector)
        {
            return DataStructure?.AllConditions?.SingleOrDefault(x => x.ConfigId == selector.ConfigId);
        }

        public Trial GetTrialFromSelector(TrialSelector selector)
        {
            var allTrials = new List<Trial>();
            if (Data?.Blocks != null)
            {
                foreach (var block in Data?.Blocks)
                {
                    if (block.Trials != null)
                    {
                        foreach (var trial in block.Trials)
                        {
                            allTrials.Add(trial);
                        }
                    }
                }
            }
            return allTrials?.FirstOrDefault(x => x.ConfigId == selector.ConfigId);
        }

        public Block GetBlockFromSelector(BlockSelector selector)
        {
            return Data?.Blocks?.FirstOrDefault(x => x.ConfigId == selector.ConfigId);
        }

        public ActionConfiguration GetActionConfigurationFromSelector(ActionSelector selector)
        {
            return DataStructure?.AllActions?.SingleOrDefault(x => x.ConfigId == selector.ConfigId);
        }

        public TrialConfiguration GetTrialConfigurationFromSelector(TrialSelector selector)
        {
            return DataStructure?.AllTrials?.FirstOrDefault(x => x.ConfigId == selector.ConfigId);
        }

        public BlockConfiguration GetBlockConfigurationFromSelector(BlockSelector selector)
        {
            return DataStructure?.Blocks?.FirstOrDefault(x => x.ConfigId == selector.ConfigId);
        }
        public virtual void FetchData()
        {
            FetchLocalData();
        }

        protected virtual void FetchLocalData()
        {
            if (DataStructure == null)
            {
                var result = "";
                var text = "";
                if (!_useStreamingAssetsFolder)
                {
                    text = FetchTextFromPersistentData(_structureFolderPath, _structureDataFileName, error =>
                    {
                        if (error as FileNotFoundException != null)
                        {
                            //if there is no localdata, check that we might have a copy in the streaming assets folder e.g. first build on android
                            result = CopyDataFromStreamingAssetsToPersistentData();
                        }
                    });
                }
                else
                {
                    text = FetchTextFromStreamingAssetsFolder(_structureFolderPath, _structureDataFileName);
                }

                if (!string.IsNullOrEmpty(text))
                {
                    result = text;
                }
                InterpretDataStructure(result);
            }
        }

        protected virtual void InterpretDataStructure(string json)
        {
            var data = InterpretData<Experience>(json);
            DataStructure = data;
            if (DataStructure != null)
            {
                Data = new Experiment(DataStructure);
                SendDataEvent();
            }
        }

        private string CopyDataFromStreamingAssetsToPersistentData()
        {
            //if there is no localdata, check that we might have a copy in the streaming assets folder e.g. first build on android
            string text = FetchTextFromStreamingAssetsFolder(_structureFolderPath, _structureDataFileName);
            if (!string.IsNullOrEmpty(text))
            {
                WriteTextToPersistentData(text, _structureFolderPath, _structureDataFileName);
            }
            return text;
        }

        protected void SendDataEvent()
        {
            OnDataLoaded?.Invoke(this, null);
        }

        protected void AddStructureToOutput(Experience dataStructure)
        {

            if (!Application.isPlaying)
                return;

            if (_jSONWriter == null)
            {
                StartOutput();
            }
            if (!_hasAddedStructureToWriter && _jSONWriter?.BaseStream != null)
            {
                _jSONWriter.Write("{\"StartDate\": \"" + _startDateTime + "\",");
               // _jSONWriter.Write(Environment.NewLine);
                _jSONWriter.Write("\"Experience\": ");
                _jSONWriter.Write(JsonConvert.SerializeObject(dataStructure, Formatting.None, new JsonSerializerSettings() { ReferenceLoopHandling = ReferenceLoopHandling.Serialize }));
                //_jSONWriter.Write(Environment.NewLine);
                _jSONWriter.Write(",\"OutputFrames\": [");
                _hasAddedStructureToWriter = true;
            }
        }

        protected virtual void DownloadedActions(DownloadHandler result)
        {
            try
            {
                Data = JsonConvert.DeserializeObject<Experiment>(result.text);
            }
            catch
            {
                //TODO: errorhandling
            }
        }

        protected virtual IEnumerator RunGetText(string url, System.Action<DownloadHandler> callback = null)
        {
            try
            {
                using (UnityWebRequest www = UnityWebRequest.Get(url))
                {
                    // could also use "US-ASCII" or "ISO-8859-1" encoding
                    string encoding = "UTF-8";
                    string base64 = System.Convert.ToBase64String(
                        System.Text.Encoding.GetEncoding(encoding)
                            .GetBytes(_authorization)
                    );
                    www.SetRequestHeader("Authorization", "Basic " + base64);
                    www.SendWebRequest();

                    while (!www.isDone)
                    {
                        continue;
                        //yield return null;
                    }

#if UNITY_2020_1_OR_NEWER
                    if (www.result == UnityWebRequest.Result.ConnectionError || www.result == UnityWebRequest.Result.DataProcessingError || www.result == UnityWebRequest.Result.ProtocolError)
#else
                    if (www.isNetworkError || www.isHttpError)
#endif
                    {
                        Debug.LogError($"Posting to {www.url} failed due to {www.error}");
                        throw new HttpListenerException(0, www.error);
                    }
                    else
                    {
                        callback?.Invoke(www.downloadHandler);
                    }
                }
            }
            catch (HttpListenerException e)
            {
                Debug.LogError($"Getting from {url} failed due to httpListenerException: {e.Message}");

            }
            catch (Exception e)
            {
                Debug.LogError($"Posting to {url} failed due to {e.Message}");
            }
            yield return null;
        }

        protected IEnumerator RunPostText(string url, string data, System.Action<DownloadHandler> callback = null)
        {
            try
            {
                var formData = System.Text.Encoding.UTF8.GetBytes(data);
                byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(data);

                using (UnityWebRequest www = UnityWebRequest.Post(url, ""))
                {
                    // could also use "US-ASCII" or "ISO-8859-1" encoding
                    string encoding = "UTF-8";

                    string base64 = System.Convert.ToBase64String(
                        System.Text.Encoding.GetEncoding(encoding)
                            .GetBytes(_authorization)
                    );
                    www.uploadHandler = new UploadHandlerRaw(formData);
                    www.SetRequestHeader("Content-Type", "application/json;charset=utf-8");
                    www.SendWebRequest();

                    while (!www.isDone)
                    {
                        continue;
                    }

#if UNITY_2020_1_OR_NEWER
                    if (www.result == UnityWebRequest.Result.ConnectionError || www.result == UnityWebRequest.Result.DataProcessingError || www.result == UnityWebRequest.Result.ProtocolError)
#else
                    if (www.isNetworkError || www.isHttpError)
#endif              
                    {
                        Debug.LogError($"Posting to {www.url} failed due to {www.error}");
                        throw new HttpListenerException(0, www.error);
                    }
                    else
                    {
                        callback?.Invoke(www.downloadHandler);
                        yield break;
                    }
                }
            }
            catch (HttpListenerException e)
            {
                Debug.LogError($"Posting to {url} failed due to httpListenerException {e.Message}");
                callback?.Invoke(null);

            }
            catch (Exception e)
            {
                Debug.LogError($"Posting to {url} failed due to {e.Message}");
                callback?.Invoke(null);
            }
            //yield return null;
        }


        private T LoadDataFromLocalFile<T>(string localUri, string filename)
        {
            try
            {
                string text = "";
                var folder = Path.Combine(Application.persistentDataPath, localUri);
                var filePath = Path.Combine(folder, filename);
                FileInfo fileInfo = new FileInfo(folder);

                if (Directory.Exists(fileInfo.DirectoryName) && File.Exists(filePath))
                {
                    text = File.ReadAllText(filePath);
                    return LoadDataFromJson<T>(text);
                }

            }
            catch (FileNotFoundException e)
            {
                Debug.LogErrorFormat($"[LoadDataFromLocalFile] File not found for {localUri}: {e.Message}");
                throw;
            }
            catch (InsufficientMemoryException e)
            {
                Debug.LogErrorFormat($"[LoadDataFromLocalFile] Not enough memory to save in {localUri}: {e.Message}");
            }
            catch (IOException e)
            {
                Debug.LogErrorFormat($"[LoadDataFromLocalFile] Writing/reading/Removing failed in {localUri}: {e.Message}");
            }
            catch (Exception e)
            {
                Debug.LogErrorFormat($"[LoadDataFromLocalFile] Error in {localUri} {e.Message}");
            }
            return default(T);
        }

        private T LoadDataFromJson<T>(string json)
        {
            return InterpretData<T>(json);
        }

        protected static void WriteTextTo(string data, string folderUri, string localUri, string filename)
        {
            try
            {
                var folder = Path.Combine(folderUri, localUri);
                var filePath = Path.Combine(folder, filename);
                FileInfo fileInfo = new FileInfo(folder + "\\");

                if (!Directory.Exists(fileInfo.DirectoryName))
                {
                    Directory.CreateDirectory(fileInfo.DirectoryName);
                }
                RemoveFileFromPersistentData(localUri, filename, true);
                File.WriteAllText(filePath, data);
            }
            catch (FileNotFoundException e)
            {
                Debug.LogErrorFormat($"[WriteTextTo] File not found for {localUri}: {e.Message}");
                throw;
            }
            catch (InsufficientMemoryException e)
            {
                Debug.LogErrorFormat($"[WriteTextTo] Not enough memory to save in {localUri}: {e.Message}");
            }
            catch (IOException e)
            {
                Debug.LogErrorFormat($"[WriteTextTo] Writing/reading/Removing failed in {localUri}: {e.Message}");
            }
            catch (Exception e)
            {
                Debug.LogErrorFormat($"[WriteTextTo] Error in {localUri}: {e.Message}");
            }
        }

        public static void WriteTextToPersistentData(string data, string localUri, string filename)
        {
            WriteTextTo(data, Application.persistentDataPath, localUri, filename);
        }

        public static void WriteTextToStreamingData(string data, string localUri, string filename)
        {
            WriteTextTo(data, Application.streamingAssetsPath, localUri, filename);
        }

        protected void WriteFileToPersistentData(byte[] bytes, string localUri, string filename)
        {
            try
            {
                var folder = Path.Combine(Application.persistentDataPath, localUri);
                var filePath = Path.Combine(folder, filename);
                FileInfo fileInfo = new FileInfo(folder);

                if (!Directory.Exists(fileInfo.DirectoryName))
                {
                    Directory.CreateDirectory(fileInfo.DirectoryName);
                }
                RemoveFileFromPersistentData(localUri, filename, true);
                if (File.Exists(filePath))
                    File.Delete(filePath);
                File.WriteAllBytes(filePath, bytes);
            }
            catch (FileNotFoundException e)
            {
                Debug.LogErrorFormat($"[WriteFileToPersistentData] File not found for {localUri}: {e.Message}");
                throw;
            }
            catch (InsufficientMemoryException e)
            {
                Debug.LogErrorFormat($"[WriteFileToPersistentData] Not enough memory to save in {localUri}: {e.Message}");
            }
            catch (IOException e)
            {
                Debug.LogErrorFormat($"[WriteFileToPersistentData] Writing/reading/Removing failed in {localUri}: {e.Message}");
            }
            catch (Exception e)
            {
                Debug.LogErrorFormat($"[WriteFileToPersistentData] Error in {localUri}: {e.Message}");
            }
        }

        protected bool MoveFile(string oldLocalLocation, string newLocalLocation)
        {
            try
            {
                var oldFile = Path.Combine(Application.persistentDataPath, oldLocalLocation);
                var newFile = Path.Combine(Application.persistentDataPath, newLocalLocation);
                FileInfo fileInfoOldFile = new FileInfo(oldFile);
                FileInfo fileInfoNewFile = new FileInfo(newFile);

                if (!Directory.Exists(fileInfoOldFile.DirectoryName) || !File.Exists(oldFile))
                {
                    return false;
                }
                else
                {
                    if (!Directory.Exists(fileInfoNewFile.DirectoryName))
                    {
                        Directory.CreateDirectory(fileInfoNewFile.DirectoryName);
                    }
                    File.Move(oldFile, newFile);
                    return true;
                }
            }
            catch (FileNotFoundException e)
            {
                Debug.LogErrorFormat($"File not found in old({oldLocalLocation}) or new({newLocalLocation}) uri: {e.Message}");
                throw;
            }
            catch (InsufficientMemoryException e)
            {
                Debug.LogErrorFormat($"Not enough memory to save in new({newLocalLocation}) uri: {e.Message}");
            }
            catch (IOException e)
            {
                Debug.LogErrorFormat($"Writing/reading/Removing failed in old({oldLocalLocation}) or new({newLocalLocation}) uri: {e.Message}");
            }
            catch (Exception e)
            {
                Debug.LogErrorFormat($"{e.Message}");
            }
            return false;
        }

        protected string FetchTextFromPersistentData(string localUri, string filename, Action<Exception> errorHandler = null)
        {
            return FetchTextFromFolder(Application.persistentDataPath, localUri, filename, errorHandler);
        }

        protected string FetchTextFromStreamingAssetsFolder(string localUri, string filename, Action<Exception> errorHandler = null)
        {
            return FetchTextFromFolder(Application.streamingAssetsPath, localUri, filename, errorHandler);
        }

        protected string FetchTextFromFolder(string rootfolder, string localUri, string filename, Action<Exception> errorHandler = null)
        {
            string text = "";
            try
            {
                var folder = Path.Combine(rootfolder, localUri);
                var filePath = Path.Combine(folder, filename);
                FileInfo fileInfo = new FileInfo(folder);

                if (!Directory.Exists(fileInfo.DirectoryName) || !File.Exists(filePath))
                {
                    errorHandler?.Invoke(new FileNotFoundException());
                    return text;
                }
                text = File.ReadAllText(filePath);
            }
            catch (Exception e)
            {
                errorHandler?.Invoke(e);
            }
            return text;
        }

        public static void RemoveFileFromPersistentData(string localUri, string filename, bool skipDirectoryCheck = false)
        {
            try
            {
                var folder = Path.Combine(Application.persistentDataPath, localUri);
                var filePath = Path.Combine(folder, filename);

                bool directoryExists = true;
                if (!skipDirectoryCheck)
                {
                    FileInfo fileInfo = new FileInfo(folder);
                    if (!Directory.Exists(fileInfo.DirectoryName))
                    {
                        directoryExists = false;
                    }
                }
                if (directoryExists && File.Exists(filePath))
                {
                    File.Delete(filePath);
                }
            }
            catch (FileNotFoundException e)
            {
                Debug.LogErrorFormat($"File not found for {localUri}: {e.Message}");
                throw;
            }
            catch (InsufficientMemoryException e)
            {
                Debug.LogErrorFormat($"Not enough memory to save: {e.Message}");
            }
            catch (IOException e)
            {
                Debug.LogErrorFormat($"Writing/reading/Removing failed: {e.Message}");
            }
            catch (Exception e)
            {
                Debug.LogErrorFormat($"{e.Message}");
            }
        }

        protected IEnumerator RunDownloadTexture(string filepath, Action<Texture2D> completedDownloadHandler = null)
        {
            if (!File.Exists(filepath))
            {
                Debug.LogError(string.Format("[BaseDataService:RunDownloadTexture] Texture not found at {0}!", filepath));
            }
            else
            {
                using (UnityWebRequest www = new UnityWebRequest("file://" + filepath))
                {
                    www.downloadHandler = new DownloadHandlerTexture();

                    yield return www.SendWebRequest();

#if UNITY_2020_1_OR_NEWER
                    if (www.result == UnityWebRequest.Result.ConnectionError || www.result == UnityWebRequest.Result.DataProcessingError || www.result == UnityWebRequest.Result.ProtocolError)
#else
                    if (www.isNetworkError || www.isHttpError)
#endif
                    {
                        Debug.LogErrorFormat("[BaseDataService:RunDownloadTexture]  Network error when trying to download texture from {0}!", filepath);
                        throw new HttpListenerException(0, www.error);
                    }
                    else
                    {
                        Texture2D t = DownloadHandlerTexture.GetContent(www);
                        if (t == null || t.width < 10)
                        {
                            Debug.LogError(string.Format("[BaseDataService:RunDownloadTexture]  Downloaded texture is null so deleting file {0}!", filepath));
                            File.Delete(filepath);
                            completedDownloadHandler?.Invoke(null);
                            www.Abort();
                            www.Dispose();
                        }
                        else
                        {
                            completedDownloadHandler?.Invoke(t);
                        }
                    }
                }
            }
        }

        public static T InterpretData<T>(string json)
        {
            try
            {
                return JsonConvert.DeserializeObject<T>(json, new JsonSerializerSettings() { ReferenceLoopHandling = ReferenceLoopHandling.Serialize });
            }
            catch(SystemException e)
            {
                Debug.LogError($"Data from ${json} not readable as {typeof(T).Name}. {e.Message}");
                return default(T);
            }
        }
    }
}

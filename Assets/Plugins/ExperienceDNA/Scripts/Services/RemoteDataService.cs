using MICT.eDNA.Managers;
using MICT.eDNA.Models;
using Newtonsoft.Json;
using System.IO;
using UnityEngine;

namespace MICT.eDNA.Services
{
    public class RemoteDataService : BaseDataService
    {
        public RemoteDataService(MonoBehaviour behaviour = null, bool useStreamingAssetsFolder = true, string dataFileName = "data.json", bool automaticallyStartWritingOutput = true)
        {
            if (ServiceLocator.DataService != null)
            {
                Debug.LogWarning("Editor tried creating new DataService but was blocked 1.");
                return;
            }
            _structureDataFileName = dataFileName;
            Init(behaviour, useStreamingAssetsFolder, automaticallyStartWritingOutput);
        }

        protected override string _api
        {
            get
            {
                return "http://127.0.0.1:8000/api/";
            }
        }

        public override void CloseStream(bool finishFile = true)
        {
            base.CloseStream(finishFile);

            if (finishFile)
            {
                //send output file to API

                var text = File.ReadAllText(_jsonFilePath);
                if (_monoBehaviour.gameObject.activeSelf)
                {
                    _monoBehaviour.StartCoroutine(RunPostText(_api + "parse/", text, (callback) =>
                    {
                        try
                        {
                            var experiment = InterpretData<Experiment>(callback?.text);
                            Debug.Log($"Sent to api succesfully!");
                            if (DataStructure.Id != experiment.GetExperienceId())
                            {
                                Debug.Log($"Experience id used in this experiment does not match backend. Locally, it was {DataStructure.Id} but remotely it was {experiment.GetExperienceId()}");
                                GetModelWithIdFromDatabase<Experience>(experiment.Id, cb =>
                                {
                                    var backendExperience = cb;
                                    SaveDataStructureToLocal(backendExperience, false);
                                    Debug.Log("Backend experience has been requested from backend and saved to the data_structure.json file");
                                });
                            }
                        }
                        catch (System.Exception e)
                        {
                            Debug.LogError("Sending to API failed. " + e.Message);
                        }
                    }));
                }
            }
        }

        private void GetModelWithIdFromDatabase<T>(int id, System.Action<T> callback)
        {
            try
            {
                string url = typeof(T).ToString().ToLower();
                _monoBehaviour.StartCoroutine(RunGetText(_api + url + "s/" + id + "/", (cb) =>
                {
                    try
                    {
                        T returnedObject = InterpretData<T>(cb?.text);
                        Debug.Log($"Got object from api ({_api + url + "s/" + id + "/"}) succesfully!");
                        callback?.Invoke(returnedObject);

                    }
                    catch (System.Exception e)
                    {
                        Debug.LogError($"Getting object with id {id} from api ({_api + url + "s / " + id + " / "}) failed. {e.Message}");
                        callback?.Invoke(default(T));
                    }
                }));
            }
            catch (System.Exception e)
            {
                Debug.LogError("Sending to API failed. " + e.Message);
                callback?.Invoke(default(T));
            }
        }

        private void SaveModelToDatabase<T>(T model, System.Action<T> callback = null)
        {
            try
            {
                string url = model.GetType().Name.ToString().ToLower();
                string text = JsonConvert.SerializeObject(model);
                _monoBehaviour.StartCoroutine(RunPostText(_api + url + "s/", text, (cb) =>
                {
                    try
                    {
                        Debug.Log($"Sent object to api ({_api + url + "s/"}) succesfully!");
                        T returnedObject = InterpretData<T>(cb?.text);
                        callback?.Invoke(returnedObject);

                    }
                    catch (System.Exception e)
                    {
                        Debug.LogError("Posting to API failed. " + e.Message);
                        callback?.Invoke(default(T));
                    }
                }));
            }
            catch (System.Exception f)
            {
                Debug.LogError("Sending to API failed. " + f.Message);
                callback?.Invoke(default(T));
            }
        }

        protected override void InterpretDataStructure(string json)
        {
            var data = InterpretData<Experience>(json);

            //check if experience id == -1. if so, this experience hasnt been added to the backend yet so do that first
            if (data.Id == -1)
            {
                UpdateRemoteData(data);
                return;
            }

            DataStructure = data;
            if (DataStructure != null)
            {
                Data = new Experiment(DataStructure);
                SendDataEvent();
            }
        }

        private void UpdateRemoteData(Experience data)
        {
            //post structure file to backend
            SaveModelToDatabase<Experience>(data, callback =>
            {
                if (callback == null)
                {
                    Debug.LogError("Callback to save experience is returning null. Replacing RemoteDataService with LocalDataService as offline fallback");
                    ServiceLocator.ReplaceService(new LocalDataService(_monoBehaviour, _useStreamingAssetsFolder, _structureDataFileName));
                    return;
                }    

                SaveDataStructureToLocal(callback, false);
                //save the database response locally as override to data structure. This will only override the (Database)Ids (since these are directly linked to the database)
                DataStructure = callback;
                if (DataStructure != null)
                {
                    Data = new Experiment(DataStructure);
                    SendDataEvent();
                }
            });
        }
    }
}

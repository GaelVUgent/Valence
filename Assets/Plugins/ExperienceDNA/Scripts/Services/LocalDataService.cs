using MICT.eDNA.Managers;
using MICT.eDNA.Models;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace MICT.eDNA.Services
{
    public class LocalDataService : BaseDataService
    {
        private const string _experimentIdKey = "ExperimentId";
        private int _experimentId = -1;

        public LocalDataService(MonoBehaviour behaviour = null, bool useStreamingAssetsFolder = true, string dataFileName = "data.json", bool automaticallyStartWritingOutput = true)
        {
            if (ServiceLocator.DataService != null)
            {
                Debug.LogWarning("[LocalDataService] Editor tried creating new DataService but was blocked 1.");
                return;
            }
            _structureDataFileName = dataFileName;
            Init(behaviour, useStreamingAssetsFolder, automaticallyStartWritingOutput);
        }

        //used in editor
        public LocalDataService(string currentSceneName = "")
        {

            if (Application.isPlaying && ServiceLocator.DataService != null)
            {
                Debug.LogWarning("[LocalDataService] Editor tried creating new DataService but was blocked 2.");
                return;
            }

            string filename = "data_structure.json";
            if (!string.IsNullOrEmpty(currentSceneName))
            {
                //check if there's a file though
                filename = $"data_structure{currentSceneName}.json";
                if (!File.Exists(Path.Combine(Application.streamingAssetsPath, _dummyFolderPath, filename)))
                {
                    filename = "data_structure.json";
                }
            }
            _structureDataFileName = filename;
            Init(null, true);
        }

        protected override string _api
        {
            get
            {
                return "http://10.0.2.2:60962/api/";
            }
        }

        public override void Init(MonoBehaviour behaviour, bool useStreamingAssetsFolder = true, bool automaticallyStartWritingOutput = false)
        {
            if (PlayerPrefs.GetInt(_experimentIdKey) > -1)
            {
               _experimentId = PlayerPrefs.GetInt(_experimentIdKey) + 1;
                PlayerPrefs.SetInt(_experimentIdKey, _experimentId);
                PlayerPrefs.Save();
            }
            base.Init(behaviour, useStreamingAssetsFolder, automaticallyStartWritingOutput);
            if (Data == null || Data.Blocks.Count == 0)
            {
                CreateDummyData();
            }
        }

        //only used in ExperienceCreatorScene
        public Experiment GetLocalData(bool fromStructureFile = true)
        {
            if (fromStructureFile)
            {
                FetchData();
            }
            else
            {
                FetchExistingExperimentData();
            }
            return Data;
        }

        //only used in ExperienceCreatorScene
        public Experience GetLocalDataStructure()
        {
            FetchData();
            return DataStructure;
        }

        private void CreateDummyData()
        {
            Data = new Experiment()
            {
                AllActions = new HashSet<Models.Action>()
                    {
                        new Models.Action(){
                            Name = "Step 1 Introduction",
                            ConfigId = 0,
                            Configuration = new ActionConfiguration(){
                                ConfigId = 0,
                                IsPhysical = false,
                                 Name = "Step 1 Introduction"
                            }
                        },
                        new Models.Action(){
                            Name = "Step 2 Introduction physical trigger",
                            ConfigId = 1,
                            Configuration = new ActionConfiguration(){
                                ConfigId = 1,
                                IsPhysical = true,
                                 Name = "Step 1 Introduction"
                            }
                        },
                    },
                Blocks = new List<Block>()
                {
                    new Block()
                    {
                        Trials = new List<Trial>()
                        {
                            new Trial()
                            {
                                Name = "Introduction",
                                Description = "Introduction description.",
                                LinkedActions = new HashSet<int>(){ 0,1}

                            }
                        }
                    }
                }
            };
            SendDataEvent();
        }

        //only used in ExperienceCreatorScene
        protected void FetchExistingExperimentData(bool overrideExistingExperiment = true)
        {
            if (overrideExistingExperiment || Data == null)
            {
                var result = "";
                var text = "";
                if (!_useStreamingAssetsFolder)
                {
                    text = FetchTextFromPersistentData(_dummyFolderPath, _dummyDataFileName, error =>
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
                    text = FetchTextFromStreamingAssetsFolder(_dummyFolderPath, _dummyDataFileName);
                }

                if (!string.IsNullOrEmpty(text))
                {
                    result = text;
                }
                InterpretData(result);
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

        protected override void InterpretDataStructure(string json)
        {
            var data = InterpretData<Experience>(json);
            DataStructure = data;
            if (DataStructure != null)
            {
                Data = new Experiment(DataStructure);
                Data.Id = _experimentId;
                SendDataEvent();
            }
        }

        [Obsolete("Using experience structure now when importing. See InterpretDataStructure")]
        private void InterpretData(string json)
        {
            var data = InterpretData<Experiment>(json);
            Data = data;
            DataStructure = new Experience(Data);
            /*if (DataStructure == null)
            {
                DataStructure = new Experience(Data);
            }*/
            if (Data != null)
            {
                SendDataEvent();
            }
        }
    }
}

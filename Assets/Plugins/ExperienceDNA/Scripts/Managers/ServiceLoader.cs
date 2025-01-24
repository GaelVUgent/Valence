using MICT.eDNA.Interfaces;
using MICT.eDNA.Models;
using MICT.eDNA.Services;
using Photon.Pun;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;

namespace MICT.eDNA.Managers
{
    [DefaultExecutionOrder(-10)]
    public class ServiceLoader : MonoBehaviour
    {
        public event EventHandler OnServicesLoaded;
        [SerializeField]
        private bool _useExperienceService = true;
        [SerializeField]
        private bool _sendToDatabase = false;
        [SerializeField]
        private bool _useUserService;
        [SerializeField]
        private bool _isVRProject;
        [SerializeField]
        private bool _usingStreamingAssetsLocation = true;
        
        [SerializeField, Obsolete, Tooltip("This boolean will be removed soon.")]
        private bool _saveDataToFileOnStart = true;
        [SerializeField, Tooltip("This is what will be used in game. HOWEVER!!! In the editor drop down fields, the data will always come from 'data.json' and not the file specified here. If ids are kept the same, this should not be a problem."), FormerlySerializedAs("DataFileNameToUse")]
        private string _dataFileNameToUse = "data_structure.json";
        [Header("Possible Photon settings")]
        [SerializeField]
        private bool _isMultiplayer;
        [Tooltip("Do all users first need to be collected in a lobby? Only used with Multiplayer VR."), SerializeField]
        private bool _useWaitingLobby = false;
        [Tooltip("Scene name that needs to be loaded when Photon has been loaded. Only used with Multiplayer VR."), SerializeField]
        private string _mainSceneName = "HospitalScene";
        [Tooltip("The Photon App ID used. Only used with Multiplayer VR."), SerializeField]
        private string _photonAppId = "12f5a393-6d96-4d6c-a774-dafa274221a4";
        private BaseStateService _stateService;
        private ExperienceService _experienceService;
        private UserService _userService;
        private IDataService _dataService;
        private PhotonNetworkService _networkService;

        private void Awake()
        {
            //force persistent data on android/ios
            #if (UNITY_ANDROID || UNITY_IOS) && !UNITY_EDITOR
            _usingStreamingAssetsLocation = false;
            #endif
            if (_useExperienceService) {
                _experienceService = new ExperienceService(this);
            }
            if (_useUserService) {
                _userService = new UserService();
            }
            if (_isVRProject) {
                if (_isMultiplayer) {
                    _stateService = new MultiplayerVRStateService();
                    _networkService = gameObject.AddComponent<PhotonNetworkService>();
                    _networkService.Init(_mainSceneName, _photonAppId, _useWaitingLobby);
                    
                    ServiceLocator.AddService(_networkService);

                    if (_useExperienceService) {
                        _networkService.OnOtherEntered += NetworkService_OnOtherEntered;
                    }
                } else { 
                    //TODO
                }
            }

            if (_sendToDatabase) {
                _dataService = new RemoteDataService(this, _usingStreamingAssetsLocation, _dataFileNameToUse, _saveDataToFileOnStart);
            } else {
                _dataService = new LocalDataService(this, _usingStreamingAssetsLocation, _dataFileNameToUse, _saveDataToFileOnStart);
            }
            
            ServiceLocator.AddService(_stateService);
            ServiceLocator.AddService(_experienceService);
            ServiceLocator.AddService(_userService);
            ServiceLocator.AddService(_dataService);

            OnServicesLoaded?.Invoke(this,null);
        }

        private void OnDestroy()
        {
            if (_useExperienceService && _isMultiplayer)
            {
                _networkService.OnOtherEntered -= NetworkService_OnOtherEntered;
            }
            //TODO: send destroys to services if multithread
            _experienceService = null;
            _stateService = null;
            _userService = null;
            ServiceLocator.DataService.CloseStream();     
            _dataService = null;
            if (_networkService != null) {
                
                Destroy(_networkService);
            }
            _networkService = null;
        }

        public void SendCollectedDataToServer() {
            if (_sendToDatabase) {
                ServiceLocator.DataService.CloseStream();

            } else {
                //TODO: make workaround laterrr
                Debug.LogError("Cant change to RemoteDataService now");
            }
        }

        private void NetworkService_OnOtherEntered(object sender, Photon.Realtime.Player e)
        {
            //Make sure that new players who enter are on the same ExperienceService page
            if (PhotonNetwork.IsMasterClient)
            {
                if (ServiceLocator.ExperienceService.CurrentBlock != null)
                {
                    ServiceLocator.NetworkService?.SendNetworkCall<ServiceLoader>(this, "CurrentBlock", ServiceLocator.ExperienceService.CurrentBlock.ConfigId);
                }
                if (ServiceLocator.ExperienceService.CurrentTrial != null)
                {
                    ServiceLocator.NetworkService?.SendNetworkCall<ServiceLoader>(this, "CurrentTrial", ServiceLocator.ExperienceService.CurrentTrial.ConfigId);
                }
            }

        }


    } 
}

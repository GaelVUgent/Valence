using MICT.eDNA.Interfaces;
using MICT.eDNA.Managers;
using MICT.eDNA.Models;
using Photon.Pun;
using Photon.Realtime;
using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MICT.eDNA.Services
{
    public class PhotonNetworkService : MonoBehaviourPunCallbacks, INetworkService, IPunObservable
    {
        public event EventHandler OnEntered;
        public event EventHandler OnExited;
        public event EventHandler<Player> OnOtherEntered;
        public event EventHandler<Player> OnOtherExited;

        public const string EXPERIMENTID_PROP_KEY = "id";
        public const string GROUPNAME_PROP_KEY = "nm";
        public const string EXPERIMENTNAME_PROP_KEY = "ex";

        private NetworkState _currentState = NetworkState.Unknown;
        public NetworkState CurrentState
        {
            get
            {
                return _currentState;
            }
            protected set
            {
                _currentState = value;
            }
        }
        private string _gameVersion;
        private bool _shouldLogDebugInfo = true;
        private bool _isConnecting = false;
        private string _mainSceneName = "HospitalScene", _lobbySceneName = "NetworkLobby";
        private string _appId = "12f5a393-6d96-4d6c-a774-dafa274221a4";
        private PhotonEventConverter _converter;
        private string _experimentName, _playerName, _groupName;
        private ExitGames.Client.Photon.Hashtable _roomProperties;
        private bool _isInMainScene = false;
        private bool _shouldUseLobby = false;
        private string _experimentNameFormat = "[{0}-{1}] {2} (Connected at {3})";
        private string _timeFormat = "dd-MM HH-mm";

        public void Init(string mainSceneName, string appId, bool useLobby = false) {
            _mainSceneName = mainSceneName;
            _appId = appId;
            _shouldUseLobby = useLobby;
        }
        public override void OnEnable()
        {
            _gameVersion = Application.version;
            _isConnecting = true;
            PhotonNetwork.AutomaticallySyncScene = true;
            _converter = gameObject.AddComponent<PhotonEventConverter>();
            SceneManager.sceneLoaded += SceneManager_sceneLoaded;

            base.OnEnable();
        }

        public override void OnDisable()
        {
            SceneManager.sceneLoaded -= SceneManager_sceneLoaded;
            base.OnDisable();

        }

        #region Public Methods
        public void Connect(string playerName = "", string groupName = "") {
            var experimentName = string.Format(_experimentNameFormat, ServiceLocator.DataService?.Data?.Id.ToString("D3"), ServiceLocator.DataService?.DataStructure?.Name ?? "", groupName ?? "", DateTime.Now.ToString(_timeFormat));
            if (!string.IsNullOrEmpty(groupName))
            {
                ServiceLocator.ExperienceService?.OverwriteExperimentName(experimentName);
                _experimentName = experimentName;
            }
            else {
                _experimentName = ServiceLocator.DataService?.Data?.Name ?? "";
            }

            if (!string.IsNullOrEmpty(groupName))
            {
                _groupName = groupName;
            }
            else
            {
                _groupName = _experimentName;
            }

            if (!string.IsNullOrEmpty(playerName))
            {
                if(ServiceLocator.UserService?.CurrentUser != null)
                    ServiceLocator.UserService.CurrentUser.Name = playerName;
                _playerName = playerName;
            }
            else
            {
                _playerName = ServiceLocator.UserService?.CurrentUser.Name ?? "";
            }

            _roomProperties = new ExitGames.Client.Photon.Hashtable() { { EXPERIMENTID_PROP_KEY, ServiceLocator.DataService?.Data?.Id ?? -1 }, { GROUPNAME_PROP_KEY, _groupName }, { EXPERIMENTNAME_PROP_KEY, _experimentName } };

            if (!PhotonNetwork.IsConnected)
            {               
                PhotonNetwork.PhotonServerSettings.AppSettings.AppIdRealtime = _appId;
                PhotonNetwork.ConnectUsingSettings();
                PhotonNetwork.GameVersion = _gameVersion;
                PhotonNetwork.NickName = _playerName;
            }
            else
            {
                PhotonNetwork.JoinRandomRoom();
            }
        }

        public string GetGroupName() {
            return _groupName;
        }

        public void Leave()
        {
            PhotonNetwork.LeaveRoom();
        }

        public void SendNetworkCall<T>(T intrface, string methodName, object variable = null)
        {
            _converter.SendNetworkCallToAllButMe<T>(intrface, methodName, variable);
        }
        #endregion

        #region Private Methods
        private void LogInfo(string info, bool isError = false)
        {
            if (_shouldLogDebugInfo)
            {
                if (isError)
                {
                    Debug.LogError(info);
                }
                else
                {
                    Debug.Log(info);
                }
            }
        }
        #endregion

        #region Photon Callbacks
        public override void OnConnectedToMaster()
        {
            if (_isConnecting)
            {
                PhotonNetwork.JoinRandomOrCreateRoom();
            }
        }

        public override void OnJoinedRoom()
        {
            CurrentState = NetworkState.EnteredNetwork;
            LogInfo(string.Format("{0}: OnJoinedRoom() called by PUN. Now this client is in the Lobby/Room.", name));
            PhotonNetwork.NickName = _playerName;
            OnEntered?.Invoke(this, null);
            if (_shouldUseLobby)
            {
                if (PhotonNetwork.IsMasterClient)
                {
                    PhotonNetwork.LoadLevel(_lobbySceneName);
                }
            }
            else {
                LoadRoom();
            }
        }

        public virtual void LoadRoom() { 

            if (PhotonNetwork.IsMasterClient)
            {
                PhotonNetwork.CurrentRoom.SetCustomProperties(_roomProperties);
                LogInfo(string.Format("PhotonNetwork : Loading Level HospitalScene for {0} players", PhotonNetwork.CurrentRoom.PlayerCount));
                PhotonNetwork.LoadLevel(_mainSceneName);               
            }            
        }

        private void SceneManager_sceneLoaded(Scene arg0, LoadSceneMode arg1)
        {
            _isInMainScene = arg0.name == _mainSceneName;
            if (arg0.name == _lobbySceneName || arg0.name == _mainSceneName) {
                if (!PhotonNetwork.IsMasterClient)
                {
                    //we're joining a room that already exists, where the master has already decided what the room name (i.e the experiment name) will be. As slaves, we need to get the master name and use it in our own files.
                    try
                    {
                        _experimentName = PhotonNetwork.CurrentRoom.CustomProperties[EXPERIMENTNAME_PROP_KEY] as string;
                        _groupName = PhotonNetwork.CurrentRoom.CustomProperties[GROUPNAME_PROP_KEY] as string;
                        ServiceLocator.ExperienceService?.OverwriteExperimentName(_experimentName);
                    }
                    catch (SystemException e)
                    {
                        Debug.LogWarningFormat("Changing experiment name after connecting to Photon room as slave failed. {0}", e.Message);
                    }
                }
            }
        }

        public override void OnLeftRoom()
        {
            CurrentState = NetworkState.ExitedNetwork;
            //PhotonNetwork.LoadLevel("01 - Lobby");
            LogInfo("OnLeftRoom() called by PUN. Now this client is in the lobby.");
            //PhotonNetwork.DestroyPlayerObjects(PhotonNetwork.LocalPlayer);

            OnExited?.Invoke(this, null);
        }

        public override void OnPlayerEnteredRoom(Player other)
        {           
            LogInfo(string.Format("Other player ({0}) entered room. AreWeMasterClient {1}", other.ActorNumber, PhotonNetwork.IsMasterClient)); // called before OnPlayerLeftRoom
            if (PhotonNetwork.IsMasterClient && _isInMainScene)
            {
                PhotonNetwork.CurrentRoom.CustomProperties[EXPERIMENTNAME_PROP_KEY] = _experimentName;
                PhotonNetwork.CurrentRoom.CustomProperties[GROUPNAME_PROP_KEY] = _groupName;
            }
            OnOtherEntered?.Invoke(this, other);
            base.OnPlayerEnteredRoom(other);
        }

        public override void OnPlayerLeftRoom(Player other)
        {            
            LogInfo(string.Format("Other player ({0}) left room. AreWeMasterClient {1}", other.NickName, PhotonNetwork.IsMasterClient)); // called before OnPlayerLeftRoom
            OnOtherExited?.Invoke(this, other);
            base.OnPlayerLeftRoom(other);
        }

        public override void OnDisconnected(DisconnectCause cause)
        {
            CurrentState = NetworkState.ExitedNetwork;
            LogInfo(string.Format("OnDisconnected() was called by PUN with reason {0}", cause), false);
            _isConnecting = false;
        }

        public override void OnJoinRandomFailed(short returnCode, string message)
        {
            LogInfo("OnJoinRandomFailed() was called by PUN. No random room available, so we create one.\nCalling: PhotonNetwork.CreateRoom");
            PhotonNetwork.CreateRoom(null);
        }

        public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
        {
        }
        #endregion
    }
}
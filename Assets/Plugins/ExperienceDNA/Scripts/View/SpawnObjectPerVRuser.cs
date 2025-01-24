using MICT.eDNA.Managers;
using MICT.eDNA.Models;
using Photon.Pun;
using System.Collections;
using UnityEngine;

namespace MICT.eDNA.View
{
    public class SpawnObjectPerVRuser : BaseSpawnObject
    {
        [SerializeField]
        protected GameObject _prefabForSecondUser;
        [SerializeField]
        private Transform _spawnPointForSecondUser;
        [SerializeField]
        private bool _destroyOnRoleChange = false;
        private UserRole _role = UserRole.DefaultVR;
        private bool _isUsingAlternative = false;
        
        protected NetworkState _networkChange = NetworkState.EnteredNetwork;

        new IEnumerator Start()
        {
            yield return new WaitForSeconds(0.1f);
            if (_networkChange == NetworkState.EnteredNetwork)
            {
                ServiceLocator.NetworkService.OnEntered += NetworkService_OnChange;
            }
            else if (_networkChange == NetworkState.ExitedNetwork)
            {
                ServiceLocator.NetworkService.OnExited += NetworkService_OnChange;
            }
            if (ServiceLocator.NetworkService.CurrentState == _networkChange)
            {
                NetworkService_OnChange(this, null);
            }
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            if (_networkChange == NetworkState.EnteredNetwork)
            {
                ServiceLocator.NetworkService.OnEntered -= NetworkService_OnChange;
            }
            else if (_networkChange == NetworkState.ExitedNetwork)
            {
                ServiceLocator.NetworkService.OnExited -= NetworkService_OnChange;
            }
        }

        //called in editor via ActionForSpawner1 gameObject 
        public void ResetPositionForUser1() {
            if (_isUsingAlternative || _createdInstance == null)
                return;
            _createdInstance.transform.position = !_isUsingAlternative ? _spawnPoint.position : _spawnPointForSecondUser.position;
            _createdInstance.transform.rotation = !_isUsingAlternative ? _spawnPoint.rotation : _spawnPointForSecondUser.rotation;
        }

        //called in editor via ActionForSpawner2 gameObject 
        public void ResetPositionForUser2()
        {
            if (!_isUsingAlternative || _createdInstance == null)
                return;
            _createdInstance.transform.position = !_isUsingAlternative ? _spawnPoint.position : _spawnPointForSecondUser.position;
            _createdInstance.transform.rotation = !_isUsingAlternative ? _spawnPoint.rotation : _spawnPointForSecondUser.rotation;
        }

        private void NetworkService_OnChange(object sender, System.EventArgs e)
        {
            if (ServiceLocator.UserService?.CurrentUser?.Role == _role)
            {
                bool hasCreated = false;

                foreach (var otherPlayer in PhotonNetwork.PlayerListOthers)
                {
                    //Debug.Log($"[SpawnObjectPerVRuser] Photon user with actornumber: {otherPlayer.ActorNumber} and userId {otherPlayer.UserId}");
                    var photonGameObject = (otherPlayer.TagObject as GameObject);
                    if (photonGameObject != null && photonGameObject.tag == "User1")
                    {
                        _isUsingAlternative = true;
                        CreateInstance(ref _prefabForSecondUser);
                        hasCreated = true;
                        break;
                    }
                    else if (photonGameObject != null && photonGameObject.tag == "User2")
                    {
                        CreateInstance(ref _prefab);
                        hasCreated = true;
                        break;
                    }
                }
                if (!hasCreated)
                {
                    CreateInstance(ref _prefab);
                }
            }
        }

        protected override bool CreateInstance(ref GameObject createdInstance)
        {
            if (createdInstance == _prefabForSecondUser)
            {
                return CreateInstance(ref _createdInstance, ref _spawnPointForSecondUser, ref _prefabForSecondUser);
            }
            else
            {
                return base.CreateInstance(ref _createdInstance);
            }
        }
    }
}

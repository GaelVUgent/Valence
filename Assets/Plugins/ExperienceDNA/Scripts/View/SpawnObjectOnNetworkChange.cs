using MICT.eDNA.Managers;
using MICT.eDNA.Models;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

namespace MICT.eDNA.View
{
    public class SpawnObjectOnNetworkChange : BaseSpawnObject
    {
        [SerializeField]
        protected NetworkState _networkChange;
        [SerializeField, FormerlySerializedAs("_destroyOnRoleChange")]
        protected bool _destroyOnChange = false;

        protected override void Start()
        {
            base.Start();
            ServiceLocator.NetworkService.OnEntered += NetworkService_OnEntered;
            ServiceLocator.NetworkService.OnExited += NetworkService_OnExited;
            if (ServiceLocator.NetworkService.CurrentState == _networkChange)
            {
                NetworkService_OnEntered(this, null);
            }
        }

        protected override void OnDestroy()
        {
            ServiceLocator.NetworkService.OnEntered -= NetworkService_OnEntered;
            ServiceLocator.NetworkService.OnExited -= NetworkService_OnExited;
            base.OnDestroy();
        }

        private void NetworkService_OnEntered(object sender, System.EventArgs e)
        {
            var _text = GameObject.FindGameObjectWithTag("DebugText")?.GetComponent<TMP_Text>();
            _text?.SetText(_text.text + " \nTrying to spawn");

            if (_networkChange == NetworkState.EnteredNetwork)
            {
                CreateInstance(ref _createdInstance);
            }
            else if (_destroyOnChange)
            {
                DestroyInstance(ref _createdInstance);
            }
        }

        private void NetworkService_OnExited(object sender, System.EventArgs e)
        {
            if (_networkChange == NetworkState.ExitedNetwork)
            {
                CreateInstance(ref _createdInstance);
            }
            else if (_destroyOnChange)
            {
                DestroyInstance(ref _createdInstance);
            }
        }
    }
}

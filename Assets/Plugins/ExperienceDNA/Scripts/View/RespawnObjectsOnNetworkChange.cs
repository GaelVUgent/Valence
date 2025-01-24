using MICT.eDNA.Managers;
using MICT.eDNA.Models;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace MICT.eDNA.View
{
    public class RespawnObjectsOnNetworkChange : BaseSpawnObject
    {
        public event EventHandler<List<GameObject>> OnObjectsRespawned;
        [SerializeField]
        protected NetworkState _networkChange;
        [SerializeField, FormerlySerializedAs("_destroyOnRoleChange")]
        protected bool _destroyOnChange = false;
        [SerializeField]
        protected GameObject[] _gameObjects;
        protected List<GameObject> _createdInstances;
        [SerializeField]
        protected bool _hideOriginalsIfNotRespawned = true;

        protected override void Start()
        {
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

        protected virtual void CreateInstances()
        {
            if (_createdInstances != null || _gameObjects == null)
                return;

            _createdInstances = new List<GameObject>();
            int i = 0;
            foreach (var go in _gameObjects)
            {
                if (_gameObjects[i] == null)
                    continue;
                GameObject createdInstance = default(GameObject);
                var didspawn = CreateInstance(ref createdInstance, ref _spawnPoint, ref _gameObjects[i]);
                if (didspawn || _hideOriginalsIfNotRespawned)
                {
                    go.SetActive(false);
                }

                go.name += " (Original)";
                if (createdInstance != null)
                {
                    createdInstance.SetActive(true);
                    _createdInstances.Add(createdInstance);
                }
                i++;
            }
            if (_createdInstances.Count > 0)
            {
                OnObjectsRespawned?.Invoke(this, _createdInstances);
            }
        }

        protected virtual void DestroyInstances()
        {
            if (_createdInstances != null)
            {
                foreach (var go in _gameObjects)
                {
                    go.SetActive(true);
                }
                foreach (var go in _createdInstances)
                {
                    Destroy(go);
                }
                _createdInstances.Clear();
                _createdInstances = null;
            }
            base.OnDestroy();
        }

        private void NetworkService_OnEntered(object sender, System.EventArgs e)
        {
            if (_networkChange == NetworkState.EnteredNetwork)
            {
                CreateInstances();
            }
            else if (_destroyOnChange)
            {
                DestroyInstances();
            }
        }

        private void NetworkService_OnExited(object sender, System.EventArgs e)
        {
            if (_networkChange == NetworkState.ExitedNetwork)
            {
                CreateInstances();
            }
            else if (_destroyOnChange)
            {
                DestroyInstances();
            }
        }
    }
}

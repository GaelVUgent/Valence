using Photon.Pun;
using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

namespace MICT.eDNA.View
{
    public abstract class BaseSpawnObject : MonoBehaviour
    {
        [FormerlySerializedAs("ObjectsRespawned")]
        public UnityAction ObjectSpawned;
        public event EventHandler<GameObject> OnObjectSpawned;
        [SerializeField]
        protected GameObject _prefab;
        [SerializeField]
        protected Transform _parent;
        [SerializeField]
        protected Transform _spawnPoint;
        [SerializeField, FormerlySerializedAs("_respawnOverPhotonNetwork")]
        protected bool _spawnOverPhotonNetwork = false;
        [SerializeField, FormerlySerializedAs("_onlyRespawnForMaster")]
        protected bool _onlySpawnForMaster = false;
        [SerializeField, FormerlySerializedAs("_onlyRespawnForLocal")]
        protected bool _onlySpawnForLocal = false;
        protected GameObject _createdInstance;

        protected virtual void Start()
        {
            if (_spawnOverPhotonNetwork && _prefab.GetComponent<PhotonView>() == null)
            {
                throw new System.Exception("If you want to spawn a prefab via Photon, please make sure the prefab is in the Resources folder, disabled and with PhotonView.");
            }
        }

        protected virtual void OnDestroy()
        {
            DestroyInstance(ref _createdInstance);
        }

        public GameObject GetSpawnedObject()
        {
            return _createdInstance;
        }

        protected virtual bool CreateInstance(ref GameObject createdInstance) {
            return CreateInstance(ref createdInstance, ref _spawnPoint, ref _prefab);
        }

        protected bool CreateInstance(ref GameObject createdInstance, ref Transform spawnPoint, ref GameObject prefab)
        {
            var spawn = prefab.transform;
            if (spawnPoint != null)
                spawn = spawnPoint;
            if (!_spawnOverPhotonNetwork)
            {
                createdInstance = GameObject.Instantiate(prefab, _parent);
                createdInstance.transform.position = spawnPoint.position;
                createdInstance.transform.rotation = spawnPoint.rotation;

            }
            else
            {
                if (_onlySpawnForMaster && !PhotonNetwork.IsMasterClient)
                    return false;
                createdInstance = PhotonNetwork.Instantiate(prefab.name, spawn.position, spawn.rotation);
                createdInstance.transform.SetParent(_parent);
            }
            OnObjectSpawned?.Invoke(this, createdInstance);
            return true;
        }

        protected virtual void DestroyInstance(ref GameObject createdInstance)
        {
            if (createdInstance != null)
            {
                if (_spawnOverPhotonNetwork)
                {
                    PhotonNetwork.Destroy(createdInstance.GetComponent<PhotonView>());
                }
                Destroy(createdInstance);
                createdInstance = null;
            }
        }
    }
}

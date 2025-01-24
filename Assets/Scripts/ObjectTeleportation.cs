using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Mict.Widgets
{
    public class ObjectTeleportation : MonoBehaviour
    {
        public bool IsObjectAlreadyInScene = false;
        public float DelayPerPosition = 1f;
        public float BeginDelay = 5f;
        public int AmountOfTimesToLoop = 5;

        public GameObject Prefab;
        public List<Transform> ListOfPositions = new List<Transform>();
        private int _currentIndexPositions = 0;
        private int _amountOfLoops = 0;
        private bool _shouldStopTeleporting = false;
        private GameObject _createdInstance;

        public UnityStringEvent OnObjectTeleported;
        public UnityEvent OnFinishedTeleporting;

        IEnumerator Start()
        {
            yield return new WaitForSeconds(BeginDelay);
            if (!IsObjectAlreadyInScene)
            {
                CreatePrefabInScene();
            }
            else
            {
                _createdInstance = Prefab;
            }
            yield return RunDoLoop();
        }

        private void OnDestroy()
        {
            if (!IsObjectAlreadyInScene)
            {
                Destroy(_createdInstance);
            }
        }

        private void CreatePrefabInScene()
        {
            if (ListOfPositions.Count == 0)
            {
                Debug.LogError("No positions filled in! Aborting teleportation.");
                _shouldStopTeleporting = true;
                return;
            }
            _createdInstance = GameObject.Instantiate(Prefab, ListOfPositions[0]);
            _createdInstance.transform.localPosition = Vector3.zero;
        }

        private IEnumerator RunDoLoop()
        {
            if (!_shouldStopTeleporting)
            {
                GoToNextPosition();
                yield return new WaitForSeconds(DelayPerPosition);
                yield return RunDoLoop();
            }
            else
            {
                OnFinishedTeleporting?.Invoke();
                yield return null;
            }
        }

        private void GoToNextPosition()
        {
            _currentIndexPositions++;
            if (_currentIndexPositions >= ListOfPositions.Count)
            {
                _amountOfLoops++;

                _currentIndexPositions = 0;
                if (_amountOfLoops >= AmountOfTimesToLoop)
                {
                    _shouldStopTeleporting = true;
                }
            }
            _createdInstance.transform.position = ListOfPositions[_currentIndexPositions].position;
            OnObjectTeleported?.Invoke(ListOfPositions[_currentIndexPositions].gameObject.name);
        }
    }
}

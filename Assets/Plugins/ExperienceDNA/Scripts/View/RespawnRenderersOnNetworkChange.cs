using System.Collections.Generic;
using UnityEngine;

namespace MICT.eDNA.View
{
    public class RespawnRenderersOnNetworkChange : RespawnObjectsOnNetworkChange
    {
        protected override void CreateInstances()
        {
            if (_createdInstances != null || _gameObjects == null)
                return;

            _createdInstances = new List<GameObject>();

            foreach (var go in _gameObjects)
            {
                _createdInstances = new List<GameObject>();
                var createdInstance = new GameObject(go.name + " Photon Duplicate");
                var filter = go.GetComponent<MeshFilter>();
                var rend = go.GetComponent<MeshRenderer>();

                CopyComponent<MeshFilter>(filter, createdInstance);
                CopyComponent<MeshRenderer>(rend, createdInstance);

                rend.enabled = false;

                createdInstance.transform.SetParent(go.transform);
                createdInstance.transform.position = Vector3.zero;

                _createdInstances.Add(createdInstance);
            }         
        }

        protected override void DestroyInstances()
        {
            if (_createdInstances != null)
            {
                foreach (var go in _gameObjects)
                {
                    var rend = go.GetComponent<MeshRenderer>();
                    rend.enabled = true;
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

        T CopyComponent<T>(T original, GameObject destination) where T : Component
        {
            System.Type type = original.GetType();
            Component copy = destination.AddComponent(type);
            System.Reflection.FieldInfo[] fields = type.GetFields();
            foreach (System.Reflection.FieldInfo field in fields)
            {
                field.SetValue(copy, field.GetValue(original));
            }
            return copy as T;
        }
    }
}

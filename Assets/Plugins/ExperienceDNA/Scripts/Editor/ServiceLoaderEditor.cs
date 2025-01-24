using MICT.eDNA.Managers;
using UnityEditor;
using UnityEngine;

namespace MICT.eDNA.Editors
{
    [CustomEditor(typeof(ServiceLoader))]
    public class ServiceLoaderEditor : Editor
    {
        private ServiceLoader _target;
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            _target = (ServiceLoader)target;
            if (GUILayout.Button("Send Data to Backend (click on this before stopping experiment)"))
            {
                _target?.SendCollectedDataToServer();
            }
        }
    } 
}

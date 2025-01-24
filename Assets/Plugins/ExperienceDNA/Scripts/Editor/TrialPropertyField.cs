﻿using MICT.eDNA.Managers;
using MICT.eDNA.Models;
using MICT.eDNA.Services;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(TrialSelector))]
[CanEditMultipleObjects]
public class TrialPropertyField : PropertyDrawer
{
    private static BaseDataService _localDataController;
    private string[] _empty = new string[] { "-" };
    [JsonIgnore]
    private static HashSet<TrialConfiguration> _data;

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        if (property.hasMultipleDifferentValues)
        {
            EditorGUI.Popup(position, property.displayName, 0, _empty);
            return;
        }
        if (_localDataController == null)
        {
            if (ServiceLocator.DataService == null)
                _localDataController = new LocalDataService(UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene().name);
            else
                _localDataController = ServiceLocator.DataService as LocalDataService;
        }
        if (_data == null) {
            _localDataController.FetchData();
            _data = _localDataController.DataStructure?.AllTrials;           
        }
        
        int id = property.FindPropertyRelative("_value").intValue;

        EditorGUI.BeginChangeCheck();
        if (_data?.Count > 0)
        {
            int index = Mathf.Max(_data.ToList().IndexOf(_data.FirstOrDefault(r => r.ConfigId == id)), -1);
            if (index == -1)
            {
                var newValue = EditorGUI.Popup(position, property.displayName, -1, _data.Select(r => r.Name).ToArray());
                if (newValue != index)
                {
                    index = newValue;
                }
                else
                {
                    EditorGUI.EndChangeCheck();
                    return;
                }
            }
            int selectedIndex = selectedIndex = EditorGUI.Popup(position, property.displayName, index, _data.Select(r => r.Name).ToArray());
            var confId = _data.ToList()[selectedIndex].ConfigId;
            property.FindPropertyRelative("_value").intValue = confId;
        }
        else
        {
            property.FindPropertyRelative("_value").intValue = EditorGUI.IntField(position, $"Unable to fetch trials ({property.displayName})", id);
        }
        EditorGUI.EndChangeCheck();
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;
using UnityEngine.UI;
using MICT.eDNA.Controllers;

public class HCCIInteractionGraph : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Image _image;
    [SerializeField] private TextMeshProUGUI _titleText;

    public string Name { get { return _titleText.text; } }

    private DataPeriod _currentLookingAtDataPeriod;
    private DataPeriod _currentGraspingDataPeriod;
    private DataPeriod _currentInteractingWithDataPeriod;
    private List<DataPeriod> _lookingAtDataPeriods = new List<DataPeriod>();
    private List<DataPeriod> _graspingDataPeriods = new List<DataPeriod>();
    private List<DataPeriod> _interactingWithDataPeriods = new List<DataPeriod>();
    private DateTime _startTime = DateTime.Now;
    private double TotalTimeMilliseconds { get { return (_startTime - DateTime.Now).TotalMilliseconds; } }
    private HCCIInteractionObject _hcciObject;

    public void Initialize(string name, HCCIInteractionObject obj)
    {
        _titleText.text = name;
        _hcciObject = obj;
        //if(gameObject.activeInHierarchy)
        //    StartCoroutine(RefreshCanvas(1.0f));
    }
    private IEnumerator RefreshCanvas(float deltaTime)
    {
        while (true)
        {
            yield return new WaitForSeconds(deltaTime);
            RecalculateGraph();
        }
    }

    public void AddValue(OutputController.InteractionType type, bool interacting)
    {
        switch (type)
        {
            case OutputController.InteractionType.LookingAt:
                if (interacting)
                {
                    if (_currentLookingAtDataPeriod == null)
                    {
                        _currentLookingAtDataPeriod = new DataPeriod(0.1f);
                        _lookingAtDataPeriods.Add(_currentLookingAtDataPeriod);
                    }
                    else
                        _currentLookingAtDataPeriod.StopDataPeriod();
                }
                else
                {
                    if(_currentLookingAtDataPeriod != null)
                    {
                        _currentLookingAtDataPeriod.StopDataPeriod();
                        _currentLookingAtDataPeriod = null;
                    }
                }
                break;
            case OutputController.InteractionType.Grasping:
                if (interacting)
                {
                    if (_currentGraspingDataPeriod == null)
                    {
                        _currentGraspingDataPeriod = new DataPeriod(0.5f);
                        _graspingDataPeriods.Add(_currentGraspingDataPeriod);
                    }
                    else
                        _currentGraspingDataPeriod.StopDataPeriod();
                }
                else
                {
                    if (_currentGraspingDataPeriod != null)
                    {
                        _currentGraspingDataPeriod.StopDataPeriod();
                        _currentGraspingDataPeriod = null;
                    }
                }
                break;
            case OutputController.InteractionType.InteractingWith:
                if (interacting)
                {
                    if (_currentInteractingWithDataPeriod == null)
                    {
                        _currentInteractingWithDataPeriod = new DataPeriod(0.9f);
                        _interactingWithDataPeriods.Add(_currentInteractingWithDataPeriod);
                    }
                    else
                        _currentInteractingWithDataPeriod.StopDataPeriod();
                }
                else
                {
                    if (_currentInteractingWithDataPeriod != null)
                    {
                        _currentInteractingWithDataPeriod.StopDataPeriod();
                        _currentInteractingWithDataPeriod = null;
                    }
                }
                break;
            default:
                break;
        }
    }
    [Obsolete("All WoZ graph stuff will be removed soon")]
    private void RecalculateGraph()
    {
        Texture2D tex = new Texture2D(Mathf.FloorToInt(_image.rectTransform.rect.width), 5);
       /* GUIChartEditor.BeginChart(new Rect(0, 0, Mathf.FloorToInt(_image.rectTransform.rect.width), 5), Color.white,
            GUIChartEditorOptions.ChartBounds(0, 1, 0, 1),
            GUIChartEditorOptions.SetOrigin(ChartOrigins.BottomLeft),
            GUIChartEditorOptions.ShowAxes(Color.grey),
            GUIChartEditorOptions.ShowGrid(float.MaxValue, float.MaxValue, Color.grey, false),
            GUIChartEditorOptions.DrawToTexture(tex, FilterMode.Point, TextureCompression.None));*/
        Vector2[] pointsLookingAt = new Vector2[2];
        for (int i = 0; i < _lookingAtDataPeriods.Count; i++)
        {
            pointsLookingAt[0] = new Vector2((float)((_startTime - _lookingAtDataPeriods[i]._StartTime).TotalMilliseconds / TotalTimeMilliseconds), _lookingAtDataPeriods[i]._Value);
            pointsLookingAt[1] = new Vector2((float)((_startTime - _lookingAtDataPeriods[i]._EndTime).TotalMilliseconds / TotalTimeMilliseconds), _lookingAtDataPeriods[i]._Value);
            //GUIChartEditor.PushLineChart(pointsLookingAt, Color.red);
        }
        Vector2[] pointsGrasping = new Vector2[2];
        for (int i = 0; i < _graspingDataPeriods.Count; i++)
        {
            pointsGrasping[0] = new Vector2((float)((_startTime - _graspingDataPeriods[i]._StartTime).TotalMilliseconds / TotalTimeMilliseconds), _graspingDataPeriods[i]._Value);
            pointsGrasping[1] = new Vector2((float)((_startTime - _graspingDataPeriods[i]._EndTime).TotalMilliseconds / TotalTimeMilliseconds), _graspingDataPeriods[i]._Value);
            //GUIChartEditor.PushLineChart(pointsGrasping, Color.magenta);
        }
        Vector2[] pointsInteractingWith = new Vector2[2];
        for (int i = 0; i < _interactingWithDataPeriods.Count; i++)
        {
            pointsInteractingWith[0] = new Vector2((float)((_startTime - _interactingWithDataPeriods[i]._StartTime).TotalMilliseconds / TotalTimeMilliseconds), _interactingWithDataPeriods[i]._Value);
            pointsInteractingWith[1] = new Vector2((float)((_startTime - _interactingWithDataPeriods[i]._EndTime).TotalMilliseconds / TotalTimeMilliseconds), _interactingWithDataPeriods[i]._Value);
            //GUIChartEditor.PushLineChart(pointsInteractingWith, Color.blue);
        }
        //GUIChartEditor.EndChart();
        
        _image.sprite = Sprite.Create(tex, new Rect(0.0f, 0.0f, tex.width, tex.height), new Vector2(0.5f, 0.5f));
    }
}

public class DataPeriod
{
    public DateTime _StartTime;
    public float _Value;
    public DateTime _EndTime;

    public DataPeriod(float value)
    {
        _StartTime = DateTime.Now;
        _Value = value;
        _EndTime = DateTime.Now;
    }

    public void StopDataPeriod()
    {
        _EndTime = DateTime.Now;
    }
}



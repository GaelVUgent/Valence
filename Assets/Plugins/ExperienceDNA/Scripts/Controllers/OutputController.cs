using MICT.eDNA.Interfaces;
using MICT.eDNA.Managers;
using MICT.eDNA.Models;
using MICT.eDNA.View;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using ViveSR.anipal.Eye;

namespace MICT.eDNA.Controllers
{
    public class OutputController : MonoBehaviour
    {

        public enum InteractionType
        {
            LookingAt,
            Grasping,
            InteractingWith
        }

        public static OutputController INSTANCE = null;

        public bool Pauzed { get; private set; } = false;
        [SerializeField]
        private bool _savePerFrame = true;
        [SerializeField]
        private float _fixedTimeDeltaForOutput = 0.01f;
        [Header("References")]
        [SerializeField] private EyeDataCapturer _eyeDataCapturer = null;
        [SerializeField] private RenderTexture _renderTexture;

        [SerializeField] private Transform _userToObjectParent;
        [SerializeField] private Transform _userToContextParent;
        [SerializeField] private Transform _userToContentParent;
        private Dictionary<Collider, HCCIInteractionObject> _hcciInteractionObjectDic = new Dictionary<Collider, HCCIInteractionObject>();
        private HCCIInteractionObject _currentLookAtObject = null;
        private List<HCCIInteractionObject> _currentGraspingObjects = new List<HCCIInteractionObject>();
        private DateTime _startTime = DateTime.Now;

        [SerializeField] private HCCIInteractionGraph _hcciGraphTemplate;

        private IExperienceService _experienceService;
        private IUserService _userService;
        private IDataService _dataService;
        public HRM Hrm;

        private SingleOutputFrame _currentOutputFrame;
        private Output _output;
        private TextMesh _eyeDataDebugText;

        private const float NULLEYEFLOATVALUE = -100;
        private Vector3 NULLEYEVECTOR3VALUE
        {
            get { return new Vector3(NULLEYEFLOATVALUE, NULLEYEFLOATVALUE, NULLEYEFLOATVALUE); }
        }

        private int _rebaScore = -1;

        private void Awake()
        {
            if (!INSTANCE)
            {
                INSTANCE = this;
                _output = new Output();
                _output.OutputFrames = new List<SingleOutputFrame>();
            }
            else
            {
                return;
            }
        }

        private void Start()
        {
            _experienceService = ServiceLocator.ExperienceService;
            _userService = ServiceLocator.UserService;
            _dataService = ServiceLocator.DataService;
            _dataService?.StartOutput();
            Hrm?.Connect(Hrm?.GetComponentInChildren<Text>());
            //start coroutine for fixed delta
            if (!_savePerFrame)
            {
                StartCoroutine(RunCreateFrame(_fixedTimeDeltaForOutput));
            }
        }

        public static void UpdateRebaScore(int reba) {
            if (INSTANCE == null)
                return;
            INSTANCE._rebaScore = reba;
        }

        public static void StartWriting() {
            INSTANCE.Pauzed = false;
        }
        public static void ClearCurrentData() {
            
            INSTANCE?.Clear();
        }

        private void Clear() {
            Pauzed = true;
            _dataService.CloseStream();
            _hcciInteractionObjectDic?.Clear();
            _currentGraspingObjects?.Clear();
            _currentLookAtObject = null;
        }

        public HCCIInteractionGraph AddHCCIGraph(HCCIInteractionObject obj)
        {
            Transform parent = transform;
            switch (obj.Type)
            {
                case HCCIInteractionType.UserToObject:
                    parent = _userToObjectParent;
                    break;
                case HCCIInteractionType.UserToContext:
                    parent = _userToContextParent;
                    break;
                case HCCIInteractionType.UserToContent:
                    parent = _userToContentParent;
                    break;
                    //TODO: dont use contextparent?
                case HCCIInteractionType.UserToUser:
                    parent = _userToContextParent;
                    break;
                default:
                    break;
            }
            HCCIInteractionGraph graph = Instantiate(_hcciGraphTemplate, parent);
            graph.Initialize(obj.Name, obj);
            for (int i = 0; i < obj.Colliders.Count; i++)
            {
                _hcciInteractionObjectDic.Add(obj.Colliders[i], obj);
            }
            return graph;
        }

        public void SetEyeDataCapturer(EyeDataCapturer eye, TextMesh text = null)
        {
            _eyeDataCapturer = eye;
            _eyeDataDebugText = text;
        }

        private void LateUpdate()
        {
            if (Pauzed || !_savePerFrame)
                return;
            //Handle Output in LateUpdate so all variables are already updated
            WriteFrameOutput();
        }

        void OnDestroy()
        {
            _dataService.SetEndOutput(_output);
            _dataService.CloseStream();
        }

        public void CheckLookAtCollider(Collider col)
        {
            if (col == null)
            {
                _currentLookAtObject?.Graph?.AddValue(InteractionType.LookingAt, false);
                _currentLookAtObject = null;

                if (_eyeDataDebugText != null)
                    _eyeDataDebugText.text = "";
            }
            else if (_hcciInteractionObjectDic.ContainsKey(col))
            {
                if (_hcciInteractionObjectDic[col] != _currentLookAtObject)
                {
                    //started looking at
                    if (_currentLookAtObject != null)
                    {
                        _currentLookAtObject.Graph.AddValue(InteractionType.LookingAt, false);
                        _currentLookAtObject?.LookAtEnded();
                    }
                    _currentLookAtObject = _hcciInteractionObjectDic[col];
                    _currentLookAtObject?.LookAtStarted();
                    _currentLookAtObject.Graph.AddValue(InteractionType.LookingAt, true);
                    _currentLookAtObject.HeatMapTime = _currentLookAtObject.HeatMapTime + Time.fixedDeltaTime;
                }
                else
                {
                    //is still looking at
                    _hcciInteractionObjectDic[col].Graph.AddValue(InteractionType.LookingAt, true);
                    _currentLookAtObject.HeatMapTime = _currentLookAtObject.HeatMapTime + Time.fixedDeltaTime;
                }

                //Debug.Log("Looking at: " + _currentLookAtObject.Name);
                if (_eyeDataDebugText != null)
                    _eyeDataDebugText.text = "Looking at: " + _currentLookAtObject.Name;
            }
        }

        public void CheckGraspCollider(Collider col, bool grabbed)
        {
            if (_hcciInteractionObjectDic.ContainsKey(col))
            {
                _hcciInteractionObjectDic[col].Graph.AddValue(InteractionType.Grasping, grabbed);
                if (grabbed)
                    _currentGraspingObjects.Add(_hcciInteractionObjectDic[col]);
                else
                    _currentGraspingObjects.Remove(_hcciInteractionObjectDic[col]);
            }
        }

        //Buttons
        private void PauzeClicked()
        {
            Pauzed = !Pauzed;

        }

        public void SetEyeDataFromViveForFrame(ref SingleOutputFrame _currentOutputFrame, ViveSR.anipal.Eye.VerboseData data)
        {
            _currentOutputFrame._PupilDiameterLeft = data.left.GetValidity(SingleEyeDataValidity.SINGLE_EYE_DATA_PUPIL_DIAMETER_VALIDITY) ? _eyeDataCapturer.eyeData.verbose_data.left.pupil_diameter_mm : NULLEYEFLOATVALUE;
            _currentOutputFrame._PupilCoordinateLeft = data.left.GetValidity(SingleEyeDataValidity.SINGLE_EYE_DATA_GAZE_DIRECTION_VALIDITY) ? _eyeDataCapturer.eyeData.verbose_data.left.gaze_direction_normalized : NULLEYEVECTOR3VALUE;
            _currentOutputFrame._EyeOpenessLeft = data.left.GetValidity(SingleEyeDataValidity.SINGLE_EYE_DATA_EYE_OPENNESS_VALIDITY) ? _eyeDataCapturer.eyeData.verbose_data.left.eye_openness : NULLEYEFLOATVALUE;

            _currentOutputFrame._PupilDiameterRight = data.right.GetValidity(SingleEyeDataValidity.SINGLE_EYE_DATA_PUPIL_DIAMETER_VALIDITY) ? _eyeDataCapturer.eyeData.verbose_data.right.pupil_diameter_mm : NULLEYEFLOATVALUE;
            _currentOutputFrame._PupilCoordinateRight = data.right.GetValidity(SingleEyeDataValidity.SINGLE_EYE_DATA_GAZE_DIRECTION_VALIDITY) ? _eyeDataCapturer.eyeData.verbose_data.right.gaze_direction_normalized : NULLEYEVECTOR3VALUE;
            _currentOutputFrame._EyeOpenessRight = data.right.GetValidity(SingleEyeDataValidity.SINGLE_EYE_DATA_EYE_OPENNESS_VALIDITY) ? _eyeDataCapturer.eyeData.verbose_data.right.eye_openness : NULLEYEFLOATVALUE;

        }

        public void SetHeartrateFromHRMForFrame(ref SingleOutputFrame _currentOutputFrame, HRM hrm)
        {
            _currentOutputFrame._HeartRate = hrm.Bpm.ToString(CultureInfo.InvariantCulture);
        }

        public void SetExperimentDataForFrame(ref SingleOutputFrame _currentOutputFrame, IExperienceService experienceService)
        {
            if (_output.Experience == null && experienceService.CurrentExperience != null) {
                _output.Experience = experienceService.CurrentExperience;
            }
            _currentOutputFrame.CurrentBlockId = experienceService.CurrentBlock?.ConfigId.ToString();
            _currentOutputFrame.CurrentTrialId = experienceService.CurrentTrial?.ConfigId.ToString();
            _currentOutputFrame.CurrentActionId = experienceService.CurrentAction != null ? experienceService.CurrentAction.ConfigId.ToString() : "";
            _currentOutputFrame.CurrentActiveConditionIds = (experienceService.CurrentActiveConditions != null && experienceService.CurrentActiveConditions?.Count > 0) ? experienceService.CurrentActiveConditions.Select(x => x.ConfigId).ToArray() : null;
            _currentOutputFrame.CurrentBlockName = experienceService.CurrentBlock?.Name ?? "";
            _currentOutputFrame.CurrentTrialName = experienceService.CurrentTrial?.Name ?? "";
            _currentOutputFrame.CurrentActionName = (experienceService.CurrentAction != null && experienceService.CurrentAction.ConfigId > -1) ? experienceService.CurrentAction.Name : "";
            _currentOutputFrame.CurrentActionDescription = (experienceService.CurrentAction != null && experienceService.CurrentAction.ConfigId > -1) ? experienceService.CurrentAction.Description : "";
            _currentOutputFrame.CurrentActiveConditionNames = (experienceService.CurrentActiveConditions != null && experienceService.CurrentActiveConditions?.Count > 0) ? experienceService.CurrentActiveConditions.Select(x => x.Name).ToArray() : null;

        }

        public void SetOtherDataForFrame(ref SingleOutputFrame _currentOutputFrame)
        {
            _currentOutputFrame.RebaScore = _rebaScore;
            
        }

        public void InitialiseFrame(ref SingleOutputFrame _currentOutputFrame)
        {
            _currentOutputFrame = new SingleOutputFrame();
            _currentOutputFrame._Time = DateTime.Now - _startTime;
            _currentOutputFrame.TimeNow = DateTime.Now;
        }

        public void SetUserDataForFrame(ref SingleOutputFrame _currentOutputFrame, IUserService userService)
        {
            _currentOutputFrame.ParticipantNumber = userService?.CurrentUser?.ParticipantNumber ?? -1;
            _currentOutputFrame.ParticipantName = userService?.CurrentUser?.FullName ?? "";
        }

        public void SetDataForFrame(ref SingleOutputFrame _currentOutputFrame, IDataService dataService)
        {
            _currentOutputFrame.ExperimentNumber = dataService?.Data.Id ?? -1;
            _currentOutputFrame.ExperimentName = dataService?.Data?.Name ?? "";
        }

        public void SetInteractionsForFrame(ref SingleOutputFrame _currentOutputFrame, List<HCCIInteractionObject> currentGraspingObjects, HCCIInteractionObject currentLookAtObject)
        {
            //Interactions
            _currentOutputFrame._LookingAt = currentLookAtObject != null ? currentLookAtObject.Name : string.Empty;
            
            if (currentGraspingObjects != null && currentGraspingObjects.Count > 0)
            {
                string graspingObjectsString = string.Empty;
                for (int i = 0; i < currentGraspingObjects.Count; i++)
                    graspingObjectsString = $"{graspingObjectsString},{currentGraspingObjects[i]?.Name}";
                _currentOutputFrame._Grasping = graspingObjectsString;
            }
            //Interacting With not implemented yet

            //HCCI interactions
            List<HCCIInteractionObject> currentInteractingObjects = new List<HCCIInteractionObject>(currentGraspingObjects) { currentLookAtObject };
            for (int i = 0; i < currentInteractingObjects.Count; i++)
            {
                if (currentInteractingObjects[i] == null)
                    continue;
                switch (currentInteractingObjects[i].Type)
                {
                    case HCCIInteractionType.UserToObject:
                        _currentOutputFrame._UserToObject = true;
                        break;
                    case HCCIInteractionType.UserToContext:
                        _currentOutputFrame._UserToContext = true;
                        break;
                    case HCCIInteractionType.UserToContent:
                        _currentOutputFrame._UserToContent = true;
                        break;
                    case HCCIInteractionType.UserToUser:
                        _currentOutputFrame._UserToUser = true;
                        break;
                    default:
                        break;
                }
            }
        }

        private IEnumerator RunCreateFrame(float timeDelta)
        {
            float timer = 0;
            while (timer < timeDelta)
            {
                timer += Time.deltaTime;
                yield return null;
            }
            WriteFrameOutput();
            yield return RunCreateFrame(_fixedTimeDeltaForOutput);
        }

        //Ouput
        private void WriteFrameOutput()
        {
            InitialiseFrame(ref _currentOutputFrame);
            SetUserDataForFrame(ref _currentOutputFrame, _userService);
            SetDataForFrame(ref _currentOutputFrame, _dataService);
            SetInteractionsForFrame(ref _currentOutputFrame, _currentGraspingObjects, _currentLookAtObject);
            //Physiodata
            if (_eyeDataCapturer != null)
            {
                //Physiodata
                SetEyeDataFromViveForFrame(ref _currentOutputFrame, _eyeDataCapturer.eyeData.verbose_data);
            }
            if (Hrm != null)
            {
                SetHeartrateFromHRMForFrame(ref _currentOutputFrame, Hrm);
            }
            SetExperimentDataForFrame(ref _currentOutputFrame, _experienceService);
            SetOtherDataForFrame(ref _currentOutputFrame);
            _dataService.SetCurrentOutputFrame(_currentOutputFrame);
        }


        //Scene Controls Graph
        public void AddSceneControlsGraphValue(WizardEvent wizardEvent, bool interacting)
        {
            // _sceneControlsInteractionGraph.AddValue(wizardEvent, interacting);
        }
        public void AddCustomSceneControlsGraphValue(WizardEvent wizardEvent, float value)
        {
            // _sceneControlsInteractionGraph.AddCustomValue(wizardEvent, value);
        }




        /* ----------------------------------------------------------------------
         * ----------------------- Data export additions ------------------------
         * ----------------------------------------------------------------------
         */

        public string GetCurrentViewedObjectName()
        {
            if (_currentLookAtObject != null)
                return _currentLookAtObject.Name;
            return "";
        }

        public EyeDataCapturer eyeData { get { return _eyeDataCapturer; } }
    }

}

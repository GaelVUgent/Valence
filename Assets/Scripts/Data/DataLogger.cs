using Mict.Widgets;
using MICT.eDNA.Controllers;
using MICT.eDNA.Managers;
using MICT.eDNA.Models;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using static ExperimentData.AssemblySequence;

public class DataLogger : MonoBehaviour
{

    //connections for collecting frame data
    public AssemblyTask task;
    public OutputController OutputController;
    public HRM hrm;
    public float meterScale;

    public Camera vrCamera;
    public Transform leftHand;
    public Transform rightHand;

    public Text hrText;
    public Text saveText;
    public Text lookAtText;
    public Text judgeText;
    public Text statusText;
    public TimedMessageWidget TimedMessages;

    private static DataLogger loggerInstance;
    private static ExperimentData data;
    public int savedCharCount { set; get; }
    private string _currentLabel = "";

    private static Condition[] conditions;



    //maintenance

    private void Awake()
    {
        Calibrator c = FindObjectOfType<Calibrator>();
        OutputController = FindObjectOfType<OutputController>();
        hrm = FindObjectOfType<HRM>();
        if (c == null | OutputController == null)
        {
            Debug.LogError("Data logger could not initialize without global references");
            enabled = false;
            return;
        }
        vrCamera = c.vrCamera;
        leftHand = c.leftHandLogPoint;
        rightHand = c.rightHandLogPoint;
        loggerInstance = this;
        savedCharCount = 0;

        if (TimedMessages != null)
        {
            //if (TimedMessages.ShowFirstMessageOnAwake)
            //    ChangeFrameLabel(TimedMessages.GetCurrentMessage());
            TimedMessages.OnNewMessageShown += ChangeFrameLabel;

        }

        conditions = new Condition[3];
        conditions[0] = MakeCondition("PartWithRobotArm", 0);
        conditions[1] = MakeCondition("PartInHand", 1);
        conditions[2] = MakeCondition("PartAttached", 2);
    }

    private Condition MakeCondition(string name, int id)
    {
        Condition condition = new Condition();
        condition.Name = name;
        condition.ConfigId = id;
        return condition;
    }

    private void OnEnable()
    {
        if (data == null)
            data = new ExperimentData(Global.participantNumber);
    }

    private void OnDisable()
    {
        if (data != null)
            data.Save(this, true);
        data = null;
    }

    private void OnDestroy()
    {
        if (TimedMessages != null)
            TimedMessages.OnNewMessageShown -= ChangeFrameLabel;
    }
    public void ChangeFrameLabel(string text)
    {
        _currentLabel = text;
        print(text);
    }

    /// <summary>
    /// Update the save text display to show when and how much data has been saved.
    /// </summary>
    private void SetSaveText(int charCount)
    {
        if (charCount == 0)
            saveText.text = "No data saved";

        float b = charCount;
        float kb = charCount / 1024f;
        float mb = kb / 1024f;
        if (mb < 1f)
        {
            saveText.text = "Saved " + Mathf.RoundToInt(kb) + " KB";
            return;
        }
        saveText.text = "Saved " + Mathf.RoundToInt(mb) + " MB";

    }

    private void Update()
    {
        UpdateDisplay();
        if (data == null)
            return;

        //capture frame data
        Frame frame = data.MakeFrame();
        frame.label = _currentLabel;
        frame.lookAtObject = OutputController.GetCurrentViewedObjectName();
        frame.frameTime = Time.deltaTime;
        frame.heartRate = hrm.Bpm;
        //frame.instructie = current instruction

        CaptureEyeData(frame);
        CaptureInputData(frame);

        data.AddHRV(hrm.PollHRI());
    }

    private void CaptureEyeData(Frame frame)
    {
        //if eye capturer is not available the rest of the data will be blank.
        EyeDataCapturer eyeDataCapturer = OutputController.eyeData;
        if (eyeDataCapturer == null)
            return;

        frame.pupilDilationLeft = eyeDataCapturer.eyeData.verbose_data.left.pupil_diameter_mm;
        frame.pupilDilationRight = eyeDataCapturer.eyeData.verbose_data.right.pupil_diameter_mm;
        frame.eyeOpenessLeft = eyeDataCapturer.eyeData.verbose_data.left.eye_openness;
        frame.eyeOpenessRight = eyeDataCapturer.eyeData.verbose_data.right.eye_openness;
        frame.gazeRay = StDir(eyeDataCapturer.eyeData.verbose_data.left.gaze_direction_normalized);
        frame.eyeCoordinatesLeft = St(eyeDataCapturer.eyeData.verbose_data.left.pupil_position_in_sensor_area);
        frame.eyeCoordinatesRight = St(eyeDataCapturer.eyeData.verbose_data.right.pupil_position_in_sensor_area);
    }

    private void CaptureInputData(Frame frame)
    {
        Vector3 headPos = vrCamera.transform.position;
        frame.headsetLocation = StPos(headPos);
        frame.headsetRotation = StRot(vrCamera.transform.rotation);
        frame.gloveLocationLeft = StPos(leftHand.position);
        frame.gloveLocationRight = StPos(rightHand.position);
        frame.gloveRotationLeft = StRot(leftHand.rotation);
        frame.gloveRotationRight = StRot(rightHand.rotation);

        AssemblyPiece ap = task.GetNextPiece();
        if (ap == null)
            return;

        Vector3 piecePos = ap.transform.position;
        frame.partLocation = StPos(piecePos);
        frame.headDistanceToPart = StDistance((piecePos - headPos).magnitude);
    }

    private void UpdateDisplay()
    {
        if (hrText != null)
            hrText.text = "BPM:" + hrm.Bpm;
        if (lookAtText != null)
            lookAtText.text = OutputController.GetCurrentViewedObjectName();
        if (statusText != null)
        {
            if (data == null || data.assemblySequences == null)
                statusText.text = "Not recording";
            else
            {
                statusText.text = "Recording (" + data.assemblySequences.Count + ")";
            }
        }

        if (saveText != null)
            SetSaveText(savedCharCount);
    }







    //utility methods used to process unity classes and units into arrays that can be saved in json.


    private float StDistance(float dist)
    {
        return dist / meterScale;
    }

    private float[] StPos(Vector2 localPos)
    {
        return St(localPos);
    }

    private float[] StPos(Vector3 worldPos)
    {
        return St(transform.worldToLocalMatrix.MultiplyPoint(worldPos) / meterScale);
    }

    private float[] StDir(Vector3 direction)
    {
        return St(transform.worldToLocalMatrix.MultiplyVector(direction).normalized);
    }

    private float[] StRot(Quaternion rotation)
    {
        Quaternion inv = Quaternion.Inverse(transform.rotation);
        return St((inv * rotation).eulerAngles);
    }

    private float[] St(Vector2 input)
    {
        return new float[] { input.x, input.y };
    }

    private float[] St(Vector3 input)
    {
        return new float[] { input.x, input.y, input.z };
    }







    // Static exposed logging methods

    public static void StartSequence()
    {
        if (loggerInstance == null)
            loggerInstance = FindObjectOfType<DataLogger>();
        if (loggerInstance != null)
            loggerInstance.hrm.PollHRI();
        data?.StartSequence();
    }

    public static void FinishSequence()
    {
        if (data != null)
        {
            data.FinishSequence();
            data.Save(loggerInstance, false);
        }

        ServiceLocator.ExperienceService.GoToNext<Block>(false);
    }

    public static void LogEventStart(ExperimentData.Event e, string details = "")
    {
        data?.LogEventStart(e, details);
        Action action = GetAction(e, details);
        if (action != null)
            ServiceLocator.ExperienceService.RegisterAction(action);

        UpdateConditionState(e, true);
    }

    private static void UpdateConditionState(ExperimentData.Event e, bool active)
    {
        Condition condition = GetCondition(e);
        if (condition == null)
            return;
        var conditionSet = ServiceLocator.ExperienceService.CurrentActiveConditions;
        //TODO condition set is not initialized??
        if (conditionSet == null)
            return;
        bool isActive = conditionSet.Contains(condition);
        if (!isActive & active)
            conditionSet.Add(condition);
        else if (isActive & !active)
            conditionSet.Remove(condition);
    }

    /// <summary>
    /// Return eDNA condition associated with the given experiment event
    /// </summary>
    private static Condition GetCondition(ExperimentData.Event e)
    {
        switch (e)
        {
            case ExperimentData.Event.RobotHelp: return conditions[0];
            case ExperimentData.Event.GrabPiece: return conditions[1];
            case ExperimentData.Event.SnapPiece: return conditions[2];
            default: return null;
        }
    }

    /// <summary>
    /// Return eDNA action associated with the given experiment event
    /// </summary>
    private static Action GetAction(ExperimentData.Event e, string details)
    {
        Action action = new Action();
        action.Name = e.ToString();
        action.ConfigId = (int)e;
        action.Description = details;
        return action;
    }

    public static void LogEventEnd(ExperimentData.Event e)
    {
        data?.LogEventEnd(e);
        UpdateConditionState(e, false);
    }

    public static void StartStep(int index, int page, int assist)
    {
        ServiceLocator.ExperienceService.GoToNext<Trial>(false);
        ServiceLocator.ExperienceService.CurrentActiveConditions.Clear();
        data?.StartStep(index, page, assist);
    }

    public static void ConfirmStep(bool correct, float d)
    {
        data?.ConfirmStep(correct, d);
        if (loggerInstance.judgeText != null)
            loggerInstance.judgeText.text = (correct ? "Correct" : "Wrong") + " (" + d.ToString("F2") + ")";
    }

    public static void FeedbackStep(int feedback)
    {
        data?.FeedbackStep(feedback);
    }
}

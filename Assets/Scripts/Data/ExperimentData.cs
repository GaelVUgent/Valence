using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using UnityEngine;
using static ExperimentData.AssemblySequence;

/// <summary>
/// Encapsulates all data recorded in the experiment.
/// This class should be managed by a DataLogger script.
/// </summary>
[Serializable]
public class ExperimentData
{

    private const string FILE_NAME_DATE_FORMAT = "yyyy'-'MM'-'dd'_'HH'-'mm";
    private const string DATE_FORMAT = "yyyy-MM-ddTHH:mm";
    private const string TIME_FORMAT = "hh\\:mm\\:ss";

    private const string CSV_SEP = ",";

    private const int STEP_COUNT = 13; //ignore parts after this count

    [NonSerialized] private DateTime _experimentStartDate;
    [NonSerialized] private int _currentStep;
    [NonSerialized] private AssemblySequence _currentSequence;
    [NonSerialized] private DateTime _taskStartDate;






    /* ----------------------------------------------------------------------
     * ---------------------------- Data content ----------------------------
     * ----------------------------------------------------------------------
     */

    public string date;
    public int participantNumber;

    public List<AssemblySequence> assemblySequences;
    [Serializable]
    public class AssemblySequence
    {

        public int id;
        public string date;
        public float assemblyAngle;
        public int totalNumberOfErrors;
        public string totalTaskTime;

        public List<int> heartRateVariance;

        public List<Frame> frames;
        [Serializable]
        public class Frame
        {
            public int step;
            public float time;

            public float frameTime;

            public string lookAtObject;

            public float pupilDilationLeft; //mm
            public float pupilDilationRight;
            public float eyeOpenessLeft; //normalized
            public float eyeOpenessRight;
            public float[] eyeCoordinatesLeft; // v2
            public float[] eyeCoordinatesRight;
            public float[] gazeRay;
            public int heartRate;
            public float[] headsetLocation;
            public float[] headsetRotation; //euler
            public float[] gloveLocationLeft;
            public float[] gloveLocationRight;
            public float[] gloveRotationLeft;
            public float[] gloveRotationRight;

            public float[] partLocation;
            public float headDistanceToPart;
            public string label;

            public Frame(int step, float time)
            {
                this.step = step;
                this.time = time;
            }
        }

        public List<Step> steps;
        [Serializable]
        public class Step
        {
            public int step;
            public float time;

            public float confirmTime;
            public bool correct;

            public float feedbackTime;
            public int feedback;

            public int instructionPage;
            public int levelOfAssistance;
            public float checkDistance; //normalized

            public Step(int step, float time, int page, int assist)
            {
                this.step = step;
                this.time = time;
                instructionPage = page;
                levelOfAssistance = assist;
            }

            public void Finish(bool correct, float time, float d)
            {
                this.correct = correct;
                confirmTime = time;
                checkDistance = d;
            }

            public void Feedback(int feedback, float time)
            {
                this.feedback = feedback;
                feedbackTime = time;
            }
        }

        public List<Event> events;
        [Serializable]
        public class Event
        {

            [NonSerialized] public ExperimentData.Event _type;
            public string type;

            public int currentStep;
            public float time;
            public float duration;
            public string details;

            public Event(ExperimentData.Event e, float time, int step, string details)
            {
                _type = e;
                type = e.ToString();
                this.time = time;
                currentStep = step;
                this.details = details;
                duration = 0f;
            }

            public void SetEnd(float time)
            {
                duration = time - this.time;
            }
        }

        public AssemblySequence(string startDate, int id)
        {
            this.id = id;
            frames = new List<Frame>();
            steps = new List<Step>();
            events = new List<Event>();
            date = startDate;
            totalNumberOfErrors = 0;
            heartRateVariance = new List<int>();
        }


    }

    public enum Event
    {
        GrabPiece,
        PieceChangeHand,
        DropPiece,
        VisualSnap,
        SnapPiece,
        ConfirmPiece,
        ResetPieces,
        ChangeInstruction,
        RobotHelp,
        RobotGrabPiece,
        RobotReachHandPosition,
        RobotHandPiece,
        Calibration
    }







    /* ----------------------------------------------------------------------
     * ------------------------------ Logging -------------------------------
     * ----------------------------------------------------------------------
     */

    /// <summary>
    /// Time in seconds since start of this sequence
    /// </summary>
    private float sequenceTime
    {
        get
        {
            return (float)DateTime.Now.Subtract(_taskStartDate).TotalSeconds;
        }
    }

    /// <summary>
    /// constructor, data recording is now possible.
    /// </summary>
    public ExperimentData(int participantNumber)
    {
        this.participantNumber = participantNumber;
        _experimentStartDate = DateTime.Now;
        date = _experimentStartDate.ToString(DATE_FORMAT);
        _currentStep = -1;
        assemblySequences = new List<AssemblySequence>();
    }

    /// <summary>
    /// Start new assembly sequence
    /// </summary>
    public void StartSequence()
    {
        _taskStartDate = DateTime.Now;
        int nextId = assemblySequences.Count + 1;
        _currentSequence = new AssemblySequence(_taskStartDate.ToString(DATE_FORMAT), nextId);
        assemblySequences.Add(_currentSequence);
    }

    /// <summary>
    /// Finish current assembly sequence
    /// </summary>
    public void FinishSequence()
    {
        _currentSequence.steps.RemoveAt(_currentSequence.steps.Count - 1);
        TimeSpan duration = DateTime.Now.Subtract(_taskStartDate);
        _currentSequence.totalTaskTime = duration.ToString(TIME_FORMAT);
    }

    /// <summary>
    /// Fire a new event.
    /// </summary>
    public void LogEventStart(Event e, string details = "")
    {
        if (_currentSequence == null)
            return;
        _currentSequence.events.Add(new AssemblySequence.Event(e, sequenceTime, _currentStep, details));
    }

    /// <summary>
    /// Complete the last event of the given type, if it hasn't been completed yet.
    /// </summary>
    public void LogEventEnd(Event e)
    {
        if (_currentSequence == null)
            return;
        int n = _currentSequence.events.Count;
        for (int i = n - 1; i >= 0; i--)
        {
            AssemblySequence.Event ed = _currentSequence.events[i];
            if (ed._type == e)
            {
                //do not overwrite duration if a secondary "ending" is fired
                if (ed.duration <= 0f)
                    ed.SetEnd(sequenceTime);
                return;
            }

        }
    }

    /// <summary>
    /// Start a new step (new piece)
    /// </summary>
    /// <param name="index">Number of the next piece</param>
    /// <param name="page">instruction page during this step</param>
    /// <param name="assist">level of assistance of the robot arm</param>
    public void StartStep(int index, int page, int assist)
    {
        _currentStep = index;
        if (_currentSequence != null & _currentStep <= STEP_COUNT)
        {
            _currentSequence.steps.Add(new Step(_currentStep, sequenceTime, page, assist));
        }
    }

    /// <summary>
    /// Pass on confirmation of the current step, with info about 
    /// whether it was correct or not.
    /// </summary>
    /// <param name="correct"></param>
    /// <param name="d">Normalized distance value, less than 1 should mean correct</param>
    public void ConfirmStep(bool correct, float d)
    {
        if (_currentSequence == null)
            return;
        int i = _currentSequence.steps.Count - 1;
        if (i >= 0 & _currentStep <= STEP_COUNT)
        {
            _currentSequence.steps[i].Finish(correct, sequenceTime, d);
            if (!correct)
                _currentSequence.totalNumberOfErrors++;
        }
    }

    /// <summary>
    /// Pass on feedback data to the current step
    /// </summary>
    /// <param name="feedback"></param>
    public void FeedbackStep(int feedback)
    {
        if (_currentSequence == null)
            return;
        int i = _currentSequence.steps.Count - 1;
        if (i >= 0 & _currentStep <= STEP_COUNT)
            _currentSequence.steps[i].Feedback(feedback, sequenceTime);
    }

    /// <summary>
    /// Make a new frame object with and add it to the list.
    /// Thanks to OOP magic you can edit this frame without 
    /// having to pass it on again.
    /// </summary>
    public Frame MakeFrame()
    {
        Frame frame = new Frame(_currentStep, sequenceTime);
        if (_currentSequence != null)
            _currentSequence.frames.Add(frame);
        return frame;
    }

    /// <summary>
    /// Add a (sub)list of heart rate interval values, to 
    /// be added to the current sequence.
    /// </summary>
    /// <param name="hrv"></param>
    public void AddHRV(List<int> hrv)
    {
        if (_currentSequence != null)
            _currentSequence.heartRateVariance.AddRange(hrv);
    }

    //buffered data for multithreading
    private string savePath;
    private DataLogger logger;
    private Thread saveThread;

    /// <summary>
    /// Save current data
    /// </summary>
    /// <param name="logger">Provide logger so save data result can be passed back</param>
    /// <param name="force">True if save should happen in this thread, i.e. during exit</param>
    public void Save(DataLogger logger, bool force)
    {
        this.logger = logger;
        savePath = GetSavePath();
        if (force)
            SaveThread();
        else
        {
            if (saveThread != null)
                saveThread.Join();
            saveThread = new Thread(SaveThread);
            saveThread.Start();
        }
    }

    /// <summary>
    /// Save functionality that can be seperated to a new thread.
    /// </summary>
    private void SaveThread()
    {
        string directory = Path.GetDirectoryName(savePath);
        Directory.CreateDirectory(directory);
        string sd = PostProcess(JsonUtility.ToJson(this));
        File.WriteAllText(savePath, sd);

        Debug.Log("Saved data to " + savePath);
        logger.savedCharCount = sd.Length;
    }

    /// <summary>
    /// Returns full path where this data should be saved.
    /// The file name includes the start date so that only 
    /// the same experiment data will be overwritten.
    /// </summary>
    private string GetSavePath()
    {
        string fileName = "ExperimentData-";
        fileName += _experimentStartDate.ToString(FILE_NAME_DATE_FORMAT);
        fileName += ".json";
        fileName = Path.Combine(GetDataFolder(), fileName);
        return fileName;
    }

    public static string GetDataFolder()
    {
        return Application.persistentDataPath;
    }

    /// <summary>
    /// Performs extra processing on json save string and returns the result
    /// </summary>
    private static string PostProcess(string json)
    {
        //round long decimals
        string longDecimalPattern = @"[0-9]*\.[0-9]{4,25}\b";
        NumberStyles ns = NumberStyles.AllowDecimalPoint;
        CultureInfo ci = CultureInfo.InvariantCulture;
        json = Regex.Replace(json, longDecimalPattern, (m) =>
        {
            string p = m.ToString();
            double value;
            if (double.TryParse(p, ns, ci, out value))
                return value.ToString("F2", ci);
            Debug.LogWarning("Could not parse decimal value: " + p);
            return p;
        });

        //remove NaN
        json = json.Replace("NaN", "0");

        return json;
    }
}

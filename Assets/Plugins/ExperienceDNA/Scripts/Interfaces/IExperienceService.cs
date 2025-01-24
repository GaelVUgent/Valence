using MICT.eDNA.Models;
using System;
using System.Collections.Generic;

namespace MICT.eDNA.Interfaces
{
    public interface IExperienceService
    {
        event EventHandler<Models.Action> OnCurrentActionChanged;
        event EventHandler<Trial> OnCurrentTrialChanged;
        event EventHandler<Block> OnCurrentBlockChanged;
        event EventHandler<HashSet<Condition>> OnActiveConditionsChanged;
        Experience CurrentExperience { get; }
        Block CurrentBlock { get; set; }
        Trial CurrentTrial { get; set; }
        Models.Action CurrentAction { get; set; }
        HashSet<Condition> CurrentActiveConditions { get; }
        void GoToNext<T>(bool sendNetworkEvent = true) where T : BaseDataObject;
        void GoToPrevious<T>(bool sendNetworkEvent = true) where T : BaseDataObject;
        T GetNext<T>() where T : BaseDataObject;
        void RegisterAction(Models.Action action, bool sendNetworkEvent = true);
        void SetData(Experiment data, bool sendEvent = true);
        void SetData(Experience data, bool sendEvent = true);
        bool IsActionInHistory(Models.Action action);
        Models.Action GetActionFromHistory(Models.Action action);
        void ResetExperiment();
        void OverwriteExperimentName(string name);
    } 
}

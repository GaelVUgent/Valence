using MICT.eDNA.Interfaces;
using MICT.eDNA.Managers;
using MICT.eDNA.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MICT.eDNA.Services
{
    public class ExperienceService: IExperienceService
    {
        public static Condition EmptyCondition = new Condition()
        {
            ConfigId = -100,
            Name = "No condition active",
        };
        public event EventHandler<Models.Action> OnCurrentActionChanged;
        public event EventHandler<Block> OnCurrentBlockChanged;
        public event EventHandler<Trial> OnCurrentTrialChanged;
        public event EventHandler<HashSet<Condition>> OnActiveConditionsChanged;

        public Experience CurrentExperience { private set; get; }
        private Block _currentBlock;
        public Block CurrentBlock
        {
            get
            {
                return _currentBlock;
            }
            set
            {
                if (_currentBlock != value)
                {
                    _currentBlock = value;
                    OnCurrentBlockChanged?.Invoke(this, _currentBlock);
                    SetActiveConditionsFromBlock();
                }
            }
        }

        private Trial _currentTrial;
        public Trial CurrentTrial
        {
            get
            {
                return _currentTrial;
            }
            set
            {
                if (_currentTrial != value)
                {
                    _currentTrial = value;
                    OnCurrentTrialChanged?.Invoke(this, _currentTrial);
                    SetActiveConditionsFromTrial();
                }
            }
        }

        private Models.Action _currentAction;
        public Models.Action CurrentAction
        {
            get
            {
                return _currentAction;
            }
            set
            {
                if (_currentAction != value)
                {
                    _currentAction = value;
                    OnCurrentActionChanged?.Invoke(this, _currentAction);

                    //set currentaction back to none. make sure to initialise class with monobehaviour to support this feature
                    if ((value != null || value != _emptyAction) && _monoBehaviour != null) {
                        _monoBehaviour.StartCoroutine(RunClearAction(value));
                    }
                }
            }
        }
        private HashSet<Condition> _currentActiveConditions;
        public HashSet<Condition> CurrentActiveConditions
        {
            get
            {

                return _currentActiveConditions;
            }
            private set {
                if (_currentActiveConditions == value || (_currentActiveConditions != null &&  _currentActiveConditions.SequenceEqual(value)))
                    return;

                _currentActiveConditions = value;
                OnActiveConditionsChanged?.Invoke(this, _currentActiveConditions);
            }
        }

        private Models.Action _emptyAction = new Models.Action()
        {
            ConfigId = -100,
            DatabaseId = -1,
            Name = "NONE",
        };

        private Queue<Models.Action> _historyOverAllTrials = new Queue<Models.Action>();
        private Experiment _data;
        private int _currentActionIndex = 0;
        private int _currentTrialIndex = 0;
        private int _currentBlockIndex = 0;
        private MonoBehaviour _monoBehaviour;
        
        public ExperienceService() { 
        
        }

        public ExperienceService(MonoBehaviour mb) {
            _monoBehaviour = mb;
        }

        public void GoToPrevious<T>(bool sendNetworkEvent = true) where T : BaseDataObject
        {
            if (sendNetworkEvent)
            {
                ServiceLocator.NetworkService?.SendNetworkCall<IExperienceService>(this, "GoToPrevious", typeof(T).Name);
            }
            switch (typeof(T).Name)
            {
                //TODO: also implement other BaseDataObjects like Block
                case "Trial":
                    if (CurrentBlock?.Trials == null || (CurrentBlock?.Trials != null && _currentTrialIndex == 0))
                    {
                        return;
                    }
                    else
                    {
                        CurrentTrial = GoPrevious<Trial>(ref _currentTrialIndex, CurrentBlock.Trials);
                    }
                    break;
            }
        }
        public void GoToNext<T>(bool sendNetworkEvent = true) where T:BaseDataObject
        {
            if (sendNetworkEvent)
            {
                ServiceLocator.NetworkService?.SendNetworkCall<IExperienceService>(this, "GoToNext", typeof(T).Name);
            }
            switch (typeof(T).Name) {
                case "Block":
                    if (_data?.Blocks != null)
                    {
                        CurrentBlock = GoNext<Block>(ref _currentBlockIndex, _data.Blocks);
                        /*if (_currentTrialIndex == -1 || (CurrentTrial == null && CurrentBlock.Trials?.Count > 0)) {
                            GoToNext<Trial>();
                        }*/
  
                        if (CurrentBlock == null || CurrentBlock.Trials == null || CurrentBlock.Trials?.Count == 0)
                        {
                            CurrentTrial = null;
                        }
                        else {
                            _currentTrialIndex = -1;
                            GoToNext<Trial>(false);
                        }
                    }
                    break;
                case "Trial":
                    //automatically go to next block if we're in the last trial or the current block doesnt have a trial
                    if (CurrentBlock?.Trials == null || (CurrentBlock?.Trials != null && _currentTrialIndex == CurrentBlock?.Trials?.Count - 1))
                    {
                        CurrentTrial = null;
                        GoToNext<Block>(false);
                        return;
                    }
                    else
                    {
                        CurrentTrial = GoNext<Trial>(ref _currentTrialIndex, CurrentBlock.Trials);
                    }
                    break;
                default:
                    if (CurrentTrial?.Actions != null)
                    {
                        CurrentAction = GoNext(ref _currentActionIndex, CurrentTrial.Actions);
                    }
                    break;

            }
            
        }

        public Experiment GetData() {
            return _data;
        }

        public bool IsActionInHistory(Models.Action action) {
            return _historyOverAllTrials.Contains(action);
        }

        public Models.Action GetActionFromHistory(Models.Action action)
        {
            return _historyOverAllTrials.FirstOrDefault(x => x.ConfigId == action.ConfigId);
        }

        public void ResetExperiment() {
            if (_data != null)
            {
                _currentBlockIndex = 0;                
                _currentTrialIndex = -1;
                _currentActionIndex = -1;
                CurrentBlock = _data.Blocks[_currentBlockIndex];
            }
        }

        private T GoNext<T>(ref int index, ICollection<T> array) where T : BaseDataObject{
            if (index < array?.Count - 1)
            {
                index = index + 1;
            }
            else {
                return default(T);
            }
            return array?.ToArray()[index];
        }

        private T GoPrevious<T>(ref int index, ICollection<T> array) where T : BaseDataObject
        {
            if (index > 0)
            {
                index = index - 1;
            }
            else
            {
                return default(T);
            }
            return array?.ToArray()[index];
        }

        public T GetNext<T>() where T : BaseDataObject
        {
            switch (typeof(T).Name)
            {
                case "Block":
                    if (_currentBlockIndex < _data.Blocks?.Count - 1)
                    {
                        return (_data.Blocks[_currentBlockIndex + 1]) as T;
                    }
                    break;
                case "Trial":
                    //if (CurrentBlock == null)
                    //    return null;
                    if (_currentTrialIndex < CurrentBlock?.Trials?.Count - 1)
                    {
                        return (CurrentBlock.Trials[_currentTrialIndex + 1]) as T;
                    }
                    else if (_currentTrialIndex != -1 && _currentTrialIndex == CurrentBlock?.Trials?.Count - 1)
                    {
                        return GetNext<Block>()?.Trials?[0] as T;
                    }
                    break;
                default:
                    if (_currentActionIndex < CurrentTrial?.Actions?.Count - 1)
                    {
                        return (CurrentTrial.Actions[_currentActionIndex + 1]) as T;
                    }
                    break;
            }
            return null;
        }

        public void SetData(Experience data, bool sendEvent = true)
        {
            CurrentExperience = data;
            _data = new Experiment(data);

            _currentBlockIndex = 0;
            _currentTrialIndex = -1;
            _currentActionIndex = -1;

            if (sendEvent)
            {
                if (_data.Blocks == null) {
                    Debug.LogWarning("This experience is empty.");
                    return;
                }
                CurrentBlock = _data.Blocks[_currentBlockIndex];

                if (CurrentBlock.Trials?.Count > 0)
                {
                    _currentTrialIndex = 0;
                    CurrentTrial = CurrentBlock.Trials[_currentTrialIndex];
                }
                else
                {
                    CurrentTrial = null;
                }

                CurrentAction = _emptyAction;
            }
            else
            {
                _currentBlock = _data.Blocks[_currentBlockIndex];
                _currentTrial = null;

                if (CurrentBlock.Trials?.Count > 0)
                {
                    _currentTrialIndex = 0;
                    _currentTrial = CurrentBlock.Trials[_currentTrialIndex];
                }
                _currentAction = _emptyAction;
            }
        }

        public void SetData(Experiment data, bool sendEvent = true)
        {
            _data = data;
            _currentBlockIndex = 0;
            _currentTrialIndex = -1;
            _currentActionIndex = -1;

            if (sendEvent) { 
                CurrentBlock = data.Blocks[_currentBlockIndex];

                if (CurrentBlock.Trials?.Count > 0)
                {
                    _currentTrialIndex = 0;
                    CurrentTrial = CurrentBlock.Trials[_currentTrialIndex];
                }
                else {
                    CurrentTrial = null;
                }

                CurrentAction = _emptyAction;
            }
            else
            {
                _currentBlock = _data.Blocks[_currentBlockIndex];
                _currentTrial = null;

                if (CurrentBlock.Trials?.Count > 0)
                {
                    _currentTrialIndex = 0;
                    _currentTrial = CurrentBlock.Trials[_currentTrialIndex];
                }
                _currentAction = _emptyAction;
            }
        }

        public void RegisterAction(Models.Action action)
        {
            RegisterAction(action);
        }
        
        public void RegisterAction(Models.Action action, bool sendNetworkEvent = true) {

            if (action == null) {
                Debug.LogWarning("Trying to register null action");
                return;
            }

            if (sendNetworkEvent) { 
                ServiceLocator.NetworkService?.SendNetworkCall<IExperienceService>(this, "RegisterAction", action.ConfigId);
            }

            var savedAction = new Models.Action(action);
            savedAction.TimeStamp = DateTime.Now;
            if (CurrentTrial != null)
            {
                savedAction.SetComplete(CurrentTrial);
                if (CurrentTrial.History == null)
                {
                    CurrentTrial.History = new Queue<Models.Action>();
                }
                CurrentTrial.History.Enqueue(savedAction);
            }
            else {
                Debug.LogWarning($"Action {savedAction.Name} registered but no trial is currently active.");
            }
            _historyOverAllTrials?.Enqueue(savedAction);
            Debug.Log($"Action {savedAction.Name.ToString()} registered!");

            CurrentAction = action;

        }

        public void OverwriteExperimentName(string name) {
            if (_data != null) {
                ServiceLocator.DataService.Data.Name = name;
                _data.Name = name;
            }
        }
        private void SetActiveConditionsFromTrial()
        {
            var trialValues = CurrentTrial?.Conditions ?? new HashSet<Condition>() { EmptyCondition };
            var blockValues = CurrentBlock?.Conditions ?? new HashSet<Condition>() { EmptyCondition };

            CurrentActiveConditions = new HashSet<Condition>(trialValues.Union(blockValues));
        }

        private void SetActiveConditionsFromBlock()
        {
            //if no trials, set the conditions. Dont do this id there are trials since the active conditions otherwise we'll get double events
            if (CurrentBlock?.Trials?.Count != 0)
            {
                return;
            }
            CurrentActiveConditions = CurrentBlock?.Conditions ?? new HashSet<Condition>() { EmptyCondition };
            
        }

        private IEnumerator RunClearAction(Models.Action currentAction) {
            yield return new WaitForEndOfFrame();
            yield return new WaitForEndOfFrame();
            yield return new WaitForEndOfFrame();

            if (CurrentAction == currentAction) {
                CurrentAction = _emptyAction;
            }
        }
    }
}
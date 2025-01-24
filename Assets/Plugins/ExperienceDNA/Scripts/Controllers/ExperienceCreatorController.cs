using MICT.eDNA.Models;
using MICT.eDNA.Services;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using System.Linq;
using Newtonsoft.Json;
using UnityEngine.UI;
using MICT.eDNA.Managers;
using MICT.eDNA.Helpers;
using MICT.eDNA.View;

namespace MICT.eDNA.Controllers
{
    [DefaultExecutionOrder(-20)]
    public class ExperienceCreatorController : MonoBehaviour
    {
        public GameObject TogglePrefab;
        private LocalDataService _dataService;
        private Experience _createdData;
        private ExperienceService _experienceService;

        public Transform AllBlocksContainer, AllConditionsContainer, AllActionsContainer, TrialsContainer, ConditionsForBlockContainer, ConditionsForTrialContainer, StepsContainer;
        public CanvasGroup SelectedBlockGroup, SelectedTrialGroup, SelectedStepGroup;
        public TMP_InputField ExperienceName, ExperienceDescription;
        public TMP_InputField SelectedBlockName, SelectedBlockDescription, SelectedTrialName, SelectedTrialDescription, SelectedStepName, SelectedStepDescription, DefaultTimingBlock, DefaultTimingTrial, SelectedConditionName, SelectedConditionDescription;
        public Toggle IsBlockPauseToggle, IsPhysicalStepToggle;
        //private Block _selectedBlock;
        //private Trial _selectedTrial; 
        //private Action _selectedAction;
        private Condition _selectedCondition;
        private List<DataObjectToggle> _toggles = new List<DataObjectToggle>();
        private int _actionCount, _trialCount, _blockCount, _conditionCount;
        private bool _isSelectingConditionForBlock = false, _isSelectingConditionForTrial = false, _isSelectingActionForTrial = false;


        void Awake()
        {
            ClearContainer(AllBlocksContainer);
            ClearContainer(AllConditionsContainer);
            ClearContainer(AllActionsContainer);
            ClearContainer(TrialsContainer);
            ClearContainer(StepsContainer);
            ClearContainer(ConditionsForTrialContainer);
            ClearContainer(ConditionsForBlockContainer);
            if (_dataService == null)
            {
                _dataService = new LocalDataService(null, true);
            }
            if (_experienceService == null)
            {
                _experienceService = new ExperienceService();
            }

            ServiceLocator.AddService(_dataService);
            ServiceLocator.AddService(_experienceService);

            UpdateUI();
        }

        public void CreateNewBlock()
        {
            CreateNew<BlockConfiguration>();
        }

        public void CreateNewTrial()
        {
            CreateNew<TrialConfiguration>();
        }

        public void CreateNewStep()
        {
            CreateNew<ActionConfiguration>();
        }

        public void CreateNewCondition()
        {
            UpdateUI();
            //TODO: Add condition list to experiment data
            CreateNew<Condition>();
        }

        public void AddConditionToBlock()
        {
            _isSelectingConditionForBlock = true;
        }

        public void AddConditionToTrial()
        {
            _isSelectingConditionForTrial = true;
        }

        public void AddActionToTrial()
        {
            _isSelectingActionForTrial = true;
        }

        public void ImportDefaultJsonFromExperiment()
        {
            ClearAllUI();
            var experiment = _dataService?.GetLocalData(false);
            _createdData = new Experience(experiment);
            CopyConfigIdFromDatabaseId();
            CreateNewUIFromData();
        }

        public void ImportDefaultJson()
        {
            ClearAllUI();
            _createdData = _dataService?.GetLocalDataStructure();

            CopyConfigIdFromDatabaseId();
            CreateNewUIFromData();
        }
        //check for zeros. Used when Id was used as primary key in Django database, which starts at 1, not 0.
        private void RemoveObseleteZeroIds()
        {
            var replacementCondition = 10000;
            var replacementAction = 10000;
            var replacementTrial = 10000;

            if (_createdData.AllConditions != null)
            {
                foreach (var condition in _createdData.AllConditions)
                {
                    if (condition.UpdateZeroIdIfNeeded())
                    {
                        replacementCondition = condition.ConfigId;
                        break;
                    }
                }
            }
            if (_createdData.AllActions != null)
            {
                foreach (var action in _createdData.AllActions)
                {
                    if (action.UpdateZeroIdIfNeeded())
                    {
                        replacementAction = action.ConfigId;
                        break;
                    }
                }
            }
            if (_createdData.AllTrials != null)
            {
                foreach (var trial in _createdData.AllTrials)
                {
                    if (trial.UpdateZeroIdIfNeeded())
                    {
                        replacementTrial = trial.ConfigId;
                        break;
                    }
                }
            }

            if (_createdData.Blocks != null)
            {
                foreach (var block in _createdData.Blocks)
                {
                    block.UpdateZeroIdIfNeeded();
                    if (block.Trials != null)
                    {
                        foreach (var trial in block.Trials)
                        {
                            trial.UpdateZeroIdIfNeeded();
                            if (trial.LinkedConditions != null && trial.LinkedConditions.Contains(0))
                            {
                                trial.LinkedConditions.Remove(0);
                                trial.LinkedConditions.Add(replacementCondition);
                            }
                            if (trial.LinkedActions != null && trial.LinkedActions.Contains(0))
                            {
                                trial.LinkedActions.Remove(0);
                                trial.LinkedActions.Add(replacementAction);
                            }
                        }
                    }
                    if (block.LinkedConditions != null && block.LinkedConditions.Contains(0))
                    {
                        block.LinkedConditions.Remove(0);
                        block.LinkedConditions.Add(replacementCondition);
                    }
                }
            }
        }

        //check for empty configIds and copy from databaseId. Used for old files that still use the old id (== databaseID) system
        private void CopyConfigIdFromDatabaseId()
        {
            if (_createdData.AllConditions != null)
            {
                foreach (var condition in _createdData.AllConditions)
                {
                    if (condition.ConfigId < 0)
                    {
                        condition.ConfigId = condition.DatabaseId;
                    }
                }
            }
            if (_createdData.AllActions != null)
            {
                foreach (var action in _createdData.AllActions)
                {
                    if (action.ConfigId < 0)
                    {
                        action.ConfigId = action.DatabaseId;
                    }
                }
            }
            if (_createdData.AllTrials != null)
            {
                foreach (var trial in _createdData.AllTrials)
                {
                    if (trial.ConfigId < 0)
                    {
                        trial.ConfigId = trial.DatabaseId;
                    }
                }
            }

            if (_createdData.Blocks != null)
            {
                foreach (var block in _createdData.Blocks)
                {
                    if (block.ConfigId < 0)
                    {
                        block.ConfigId = block.DatabaseId;
                    }
                }
            }
        }

        private void CreateNewUIFromData()
        {
            if (_createdData.Blocks != null)
            {
                foreach (var block in _createdData.Blocks)
                {
                    CreateNew<BlockConfiguration>(block);
                    if (block.Trials != null)
                    {
                        foreach (var trial in block.Trials)
                        {
                            CreateNew<TrialConfiguration>(trial);
                        }
                    }
                    if (block.Conditions != null)
                    {
                        foreach (var condition in block.Conditions)
                        {
                            CreateNew<Condition>(condition, ConditionsForBlockContainer);
                        }
                    }
                }
            }
            if (_createdData.AllConditions != null)
            {
                foreach (var condition in _createdData.AllConditions)
                {
                    CreateNew<Condition>(condition);
                }
            }
            if (_createdData.AllActions != null)
            {
                foreach (var action in _createdData.AllActions)
                {
                    CreateNew<ActionConfiguration>(action);
                }
            }
            ExperienceName?.SetTextWithoutNotify(_createdData.Name);
            ExperienceDescription?.SetTextWithoutNotify(_createdData.Description);
        }


        private void ClearAllUI()
        {
            ClearContainer(AllBlocksContainer);
            ClearContainer(AllConditionsContainer);
            ClearContainer(AllActionsContainer);
            ClearContainer(TrialsContainer);
            ClearContainer(StepsContainer);
            ClearContainer(ConditionsForTrialContainer);
            ClearContainer(ConditionsForBlockContainer);
            ExperienceName.SetTextWithoutNotify("");
            ExperienceDescription.SetTextWithoutNotify("");
            _toggles?.Clear();
        }

        private void SaveInputfieldChanges()
        {
            if (_experienceService.CurrentBlock != null)
            {
                _experienceService.CurrentBlock.Configuration.Name = SelectedBlockName.text;
                _experienceService.CurrentBlock.Configuration.Description = SelectedBlockDescription.text;
                _toggles.Where(x => x.Type == DataObjectToggle.ObjectType.Block && x.Block.ConfigId == _experienceService.CurrentBlock.ConfigId)?.ForEach<DataObjectToggle>(y => y.SetName(_experienceService.CurrentBlock.Configuration.Name));
            }
            if (_experienceService.CurrentTrial != null)
            {
                _experienceService.CurrentTrial.Configuration.Name = SelectedTrialName.text;
                _experienceService.CurrentTrial.Configuration.Description = SelectedTrialDescription.text;
                _toggles.Where(x => x.Type == DataObjectToggle.ObjectType.Trial && x.Trial.ConfigId == _experienceService.CurrentTrial.ConfigId)?.ForEach<DataObjectToggle>(y => y.SetName(_experienceService.CurrentTrial.Configuration.Name));
            }
            if (_experienceService.CurrentAction != null)
            {
                _experienceService.CurrentAction.Configuration.Name = SelectedStepName.text;
                _experienceService.CurrentAction.Configuration.Description = SelectedStepDescription.text;
                _toggles.Where(x => x.Type == DataObjectToggle.ObjectType.Action && x.Action.ConfigId == _experienceService.CurrentAction.ConfigId)?.ForEach<DataObjectToggle>(y => y.SetName(_experienceService.CurrentAction.Configuration.Name));
            }
            if (_selectedCondition != null)
            {
                _selectedCondition.Name = SelectedConditionName.text;
                _selectedCondition.Description = SelectedConditionDescription.text;
                _toggles.Where(x => x.Type == DataObjectToggle.ObjectType.Condition && x.Condition.ConfigId == _selectedCondition.ConfigId)?.ForEach<DataObjectToggle>(y => y.SetName(_selectedCondition.Name));

            }
        }

        private void CreateNew<T>(T original = null, Transform container = null) where T : BaseDataObject, new()
        {
            SaveInputfieldChanges();
            T newObject = new T();


            if (_createdData == null)
            {
                _createdData = new Experience();
                _createdData.Blocks = new List<BlockConfiguration>();
            }

            newObject.Name = "Unnamed";
            newObject.Description = "";

            if (newObject is BlockConfiguration)
            {
                if (original == null)
                {
                    newObject.ConfigId = _blockCount + 1;
                    _createdData.Blocks.Add(newObject as BlockConfiguration);
                    SpawnForContainer<BlockConfiguration>(AllBlocksContainer, newObject as BlockConfiguration);
                    _blockCount++;
                }
                else
                {
                    SpawnForContainer<BlockConfiguration>(AllBlocksContainer, original as BlockConfiguration);
                    _blockCount = _blockCount <= original.ConfigId ? original.ConfigId + 1 : _blockCount;
                }
            }
            else if (newObject is TrialConfiguration)
            {
                if (original == null)
                {
                    if (_experienceService.CurrentBlock.Trials == null)
                    {
                        _experienceService.CurrentBlock.Trials = new List<Trial>();
                    }
                    newObject.ConfigId = _trialCount + 1;
                    _experienceService.CurrentBlock.Trials.Add(new Trial(newObject as TrialConfiguration));
                    SpawnForContainer<TrialConfiguration>(TrialsContainer, newObject as TrialConfiguration);
                    _trialCount++;
                }
                else
                {
                    SpawnForContainer<TrialConfiguration>(TrialsContainer, original as TrialConfiguration);
                    _trialCount = _trialCount <= original.ConfigId ? original.ConfigId + 1 : _trialCount;
                }
            }
            else if (newObject is ActionConfiguration)
            {
                if (original == null)
                {
                    if (_experienceService.CurrentTrial != null && _experienceService.CurrentTrial.Configuration.LinkedActions == null)
                    {
                        _experienceService.CurrentTrial.Configuration.LinkedActions = new HashSet<int>();
                    }
                    if (_createdData.AllActions == null)
                    {
                        _createdData.AllActions = new HashSet<ActionConfiguration>();
                    }
                    newObject.ConfigId = _actionCount + 1;
                    _createdData.AllActions.Add(newObject as ActionConfiguration);
                    SpawnForContainer<ActionConfiguration>(AllActionsContainer, newObject as ActionConfiguration);
                    _actionCount++;
                }
                else
                {
                    if (container == null || container == AllActionsContainer)
                    {
                        SpawnForContainer<ActionConfiguration>(AllActionsContainer, original as ActionConfiguration);
                        _actionCount = _actionCount <= original.ConfigId ? original.ConfigId + 1 : _actionCount;
                    }
                    else
                    {
                        if (_experienceService.CurrentTrial != null && _experienceService.CurrentTrial.Configuration.LinkedActions == null)
                        {
                            _experienceService.CurrentTrial.Configuration.LinkedActions = new HashSet<int>();
                        }
                        if (_experienceService.CurrentTrial != null && _experienceService.CurrentTrial.Configuration.Actions == null)
                        {
                            _experienceService.CurrentTrial.Configuration.SetActions(new HashSet<ActionConfiguration>());
                        }
                        _experienceService.CurrentTrial?.Configuration.LinkedActions?.Add(original.ConfigId);
                        _experienceService.CurrentTrial?.Configuration.Actions?.Add(original as ActionConfiguration);
                        SpawnForContainer<ActionConfiguration>(StepsContainer, original as ActionConfiguration);
                    }

                }
            }
            else if (newObject is Condition)
            {
                if (original != null)
                {
                    if (_createdData.AllConditions == null)
                    {
                        _createdData.AllConditions = new HashSet<Condition>();
                    }
                    if (container == ConditionsForBlockContainer)
                    {
                        if (_experienceService.CurrentBlock != null && _experienceService.CurrentBlock?.Configuration.LinkedConditions == null)
                        {
                            _experienceService.CurrentBlock.Configuration.LinkedConditions = new HashSet<int>();
                        }
                        if (_experienceService.CurrentBlock != null && _experienceService.CurrentBlock?.Configuration.Conditions == null)
                        {
                            _experienceService.CurrentBlock.Configuration.SetConditions(new HashSet<Condition>());
                        }

                        _experienceService.CurrentBlock?.Configuration.LinkedConditions.Add(original.ConfigId);
                        _experienceService.CurrentBlock?.Configuration.Conditions?.Add(original as Condition);
                        SpawnForContainer<Condition>(ConditionsForBlockContainer, original as Condition);
                    }
                    else if (container == ConditionsForTrialContainer)
                    {
                        if (_experienceService.CurrentTrial != null && _experienceService.CurrentTrial?.Configuration.LinkedConditions == null)
                        {
                            _experienceService.CurrentTrial.Configuration.LinkedConditions = new HashSet<int>();
                        }
                        if (_experienceService.CurrentTrial != null && _experienceService.CurrentTrial?.Configuration.Conditions == null)
                        {
                            _experienceService.CurrentTrial.Configuration.SetConditions(new HashSet<Condition>());
                        }
                        if (_createdData.AllConditions == null)
                        {
                            _createdData.AllConditions = new HashSet<Condition>();
                        }
                        _experienceService.CurrentTrial?.Configuration.LinkedConditions.Add(original.ConfigId);
                        _experienceService.CurrentTrial?.Configuration.Conditions?.Add(original as Condition);
                        SpawnForContainer<Condition>(ConditionsForTrialContainer, original as Condition);
                    }
                    else if (container == AllConditionsContainer || container == null)
                    {
                        if (_createdData.AllConditions == null)
                        {
                            _createdData.AllConditions = new HashSet<Condition>();
                        }
                        //_createdData.AllConditions.Add(original as Condition);
                        SpawnForContainer<Condition>(AllConditionsContainer, original as Condition);
                        _conditionCount = _conditionCount <= original.ConfigId ? original.ConfigId + 1 : _conditionCount;
                    }
                }
                else
                {
                    newObject.ConfigId = _conditionCount + 1;
                    //we're just creating a new condition in the AllConditionsContainer
                    if (_createdData.AllConditions == null)
                    {
                        _createdData.AllConditions = new HashSet<Condition>();
                    }
                    _createdData.AllConditions.Add(newObject as Condition);
                    SpawnForContainer<Condition>(AllConditionsContainer, newObject as Condition);
                    _conditionCount++;
                }

            }
        }

        public void SetActive<T>(T selected) where T : BaseDataObject
        {
            //first save currently edited fields
            SaveInputfieldChanges();

            if (selected is BlockConfiguration)
            {
                ClearContainer(TrialsContainer);
                ClearContainer(StepsContainer);
                ClearContainer(ConditionsForTrialContainer);
                ClearContainer(ConditionsForBlockContainer);

                _experienceService.CurrentBlock = new Block(selected as BlockConfiguration);
                _experienceService.CurrentTrial = null;
                _experienceService.CurrentAction = null;

                if (_experienceService.CurrentBlock?.Trials != null)
                {
                    foreach (var item in _experienceService.CurrentBlock?.Configuration.Trials)
                    {
                        SpawnForContainer<TrialConfiguration>(TrialsContainer, item);
                    }
                }
                if (_experienceService.CurrentBlock?.Configuration.LinkedConditions != null)
                {
                    foreach (var item in _experienceService.CurrentBlock?.Configuration.LinkedConditions)
                    {
                        var condition = _createdData.AllConditions.SingleOrDefault(x => x.ConfigId == item);
                        if (condition != null)
                        {
                            SpawnForContainer<Condition>(ConditionsForBlockContainer, condition);
                        }
                    }
                }

            }
            else if (selected is TrialConfiguration)
            {
                ClearContainer(StepsContainer);
                ClearContainer(ConditionsForTrialContainer);

                _experienceService.CurrentTrial = new Trial(selected as TrialConfiguration);
                _experienceService.CurrentAction = null;

                if (_experienceService.CurrentTrial?.Configuration.LinkedActions != null)
                {

                    foreach (var item in _experienceService.CurrentTrial?.Configuration.LinkedActions)
                    {
                        var action = _createdData.AllActions.SingleOrDefault(x => x.ConfigId == item);
                        SpawnForContainer<ActionConfiguration>(StepsContainer, action);
                    }
                }
                if (_experienceService.CurrentTrial?.Configuration.LinkedConditions != null)
                {
                    foreach (var item in _experienceService.CurrentTrial?.Configuration.LinkedConditions)
                    {
                        var condition = _createdData.AllConditions.SingleOrDefault(x => x.ConfigId == item);
                        if (condition != null)
                        {
                            SpawnForContainer<Condition>(ConditionsForTrialContainer, condition);
                        }
                    }
                }
            }
            else if (selected is ActionConfiguration)
            {
                if (_isSelectingActionForTrial)
                {
                    CreateNew<ActionConfiguration>(selected as ActionConfiguration, StepsContainer);
                }
                else
                {
                    _experienceService.CurrentAction = new Action(selected as ActionConfiguration);
                }
            }
            else if (selected is Condition)
            {
                //condition was selected so it's very probable that the user is adding a condition to a block or trial. Let's check!
                if (_isSelectingConditionForBlock)
                {
                    CreateNew<Condition>(selected as Condition, ConditionsForBlockContainer);


                }
                else if (_isSelectingConditionForTrial)
                {
                    CreateNew<Condition>(selected as Condition, ConditionsForTrialContainer);


                }
                else
                {
                    //condition in AllConditions block was selected
                    _selectedCondition = selected as Condition;
                }
            }

            UpdateUI();
            _isSelectingConditionForTrial = false;
            _isSelectingConditionForBlock = false;
            _isSelectingActionForTrial = false;
        }

        public void SaveDataToJson()
        {
            if (string.IsNullOrEmpty(ExperienceName.text))
            {
                Debug.LogError("[ExperienceCreatorController] Can't save to JSON until the Experience Name is filled in!");
                return;
            }

            try
            {
                _createdData.Name = ExperienceName.text;
                _createdData.Description = ExperienceDescription.text;
                _dataService?.SaveDataStructureToLocal(_createdData, false);
                Debug.Log("OK! Experience structure was saved to StreamingAssets/dummy/data_structure.json");
            }
            catch (System.Exception)
            {
                Debug.LogError("Something went wrong trying to save this experience structure");
            }
        }

        public void ToggleIsBlockPause(bool isOn)
        {
            if (_experienceService.CurrentBlock != null)
            {
                _experienceService.CurrentBlock.IsBreak = isOn;
            }
        }

        public void ToggleIsPhysicalStep(bool isOn)
        {
            if (_experienceService.CurrentAction != null)
            {
                _experienceService.CurrentAction.IsPhysical = isOn;
            }
            if (_experienceService.CurrentAction?.Configuration != null)
            {
                _experienceService.CurrentAction.Configuration.IsPhysical = isOn;
            }
        }

        private void UpdateUI()
        {
            SelectedBlockGroup.alpha = _experienceService.CurrentBlock == null ? 0 : 1;
            SelectedTrialGroup.alpha = _experienceService.CurrentTrial == null ? 0 : 1;
            SelectedStepGroup.alpha = _experienceService.CurrentAction == null ? 0 : 1;

            //GetComponentsInChildren<DisplayExperienceElement>().ForEach(x => x.UpdateUI());
            //SelectedBlockName?.SetTextWithoutNotify(_experienceService.CurrentBlock == null ? "" : _experienceService.CurrentBlock.Name);
            //SelectedBlockDescription?.SetTextWithoutNotify(_experienceService.CurrentBlock == null ? "" : _experienceService.CurrentBlock.Description);
            //SelectedTrialName?.SetTextWithoutNotify(_experienceService.CurrentTrial == null ? "" : _experienceService.CurrentTrial.Name);
            //SelectedTrialDescription?.SetTextWithoutNotify(_experienceService.CurrentTrial == null ? "" : _experienceService.CurrentTrial.Description);
            //SelectedStepName?.SetTextWithoutNotify(_experienceService.CurrentAction == null ? "" : _experienceService.CurrentAction.Name);
            //SelectedStepDescription?.SetTextWithoutNotify(_experienceService.CurrentAction == null ? "" : _experienceService.CurrentAction.Description);
            //IsBlockPauseToggle.isOn = _experienceService.CurrentBlock == null ? false : _experienceService.CurrentBlock.IsBreak;
            IsPhysicalStepToggle.isOn = _experienceService.CurrentAction == null ? false : _experienceService.CurrentAction.IsPhysical;
            SelectedConditionName?.SetTextWithoutNotify(_selectedCondition == null ? "" : _selectedCondition.Name);
            SelectedConditionDescription?.SetTextWithoutNotify(_selectedCondition == null ? "" : _selectedCondition.Description);
        }

        private void RegisterToggle(DataObjectToggle toggle)
        {
            _toggles.Add(toggle);
        }

        private void ParseData(string uri)
        {
            var data = _dataService?.GetLocalDataStructure();
            if (data != null)
            {
                _createdData = data;
            }
        }

        private void ClearContainer(Transform container)
        {
            if (container.childCount == 0)
                return;
            for (int i = container.childCount - 1; i >= 0; i--)
            {
                var toggle = container.GetChild(i).GetComponent<DataObjectToggle>();
                if (toggle != null && _toggles.Contains(toggle))
                {
                    _toggles.Remove(container.GetChild(i).GetComponent<DataObjectToggle>());
                }
                Destroy(container.GetChild(i).gameObject);
            }
        }

        private void SpawnForContainer<T>(Transform container, T dataobject) where T : BaseDataObject
        {
            var go = Instantiate(TogglePrefab, container);
            var toggle = go.GetComponent<DataObjectToggle>();
            if (toggle)
            {
                RegisterToggle(toggle);
                toggle.Init<T>(dataobject);
            }
        }




    }
}

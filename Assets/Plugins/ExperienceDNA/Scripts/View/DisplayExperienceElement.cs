using MICT.eDNA.Managers;
using MICT.eDNA.Models;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace MICT.eDNA.View
{
    public class DisplayExperienceElement : MonoBehaviour, IPointerClickHandler
    {
        public ExperienceElement Type;
        [SerializeField]
        private bool _displayCurrent = true;
        //TODO: add editor to toggle bools correctly so we cant have both true
        [SerializeField]
        private bool _displayNext = false;
        [Tooltip("Only applicable to inputfields. Do you want the input field to change the value of the chosen experience element?")]
        [SerializeField]
        private bool _allowedToUpdateData = false;
        //TODO: add editor to hide index field if checkboxes are true
        //[SerializeField]
        //private int _indexToShow;
        private TMP_Text _text;
        private bool _isInputField = false;
        private bool _isToggle = false;
        private TMP_InputField _inputField;
        private Toggle _toggle;
        private string _currentString;
        private bool _currentBool;

        private void OnEnable()
        {
            _text = GetComponent<TMP_Text>();
            _inputField = GetComponent<TMP_InputField>();
            _toggle = GetComponent<Toggle>();
            //if no text, maybe this is an inputfield?
            if (_inputField != null)
            {
                _isInputField = true;
                _text = _inputField?.textComponent;
                if (_allowedToUpdateData)
                {
                    _inputField.onEndEdit.AddListener(InputFieldChanged);
                }
            }
            //else maybe toggle?
            else if (_toggle != null)
            {
                _isToggle = true;
            }

            if (Type.ToString().StartsWith("Action"))
            {
                ServiceLocator.ExperienceService.OnCurrentActionChanged += ExperienceService_OnElementChanged;
                ExperienceService_OnElementChanged(this, ServiceLocator.ExperienceService.CurrentAction);
                
            }
            else if (Type.ToString().StartsWith("Trial"))
            {
                ServiceLocator.ExperienceService.OnCurrentTrialChanged += ExperienceService_OnElementChanged;
                ExperienceService_OnElementChanged(this, ServiceLocator.ExperienceService.CurrentTrial);
                
            }
            else if (Type.ToString().StartsWith("Block"))
            {
                ServiceLocator.ExperienceService.OnCurrentBlockChanged += ExperienceService_OnElementChanged;
                ExperienceService_OnElementChanged(this, ServiceLocator.ExperienceService.CurrentBlock);
                
            }
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (!_isToggle)
                return;
            GetCurrentElements(out var block, out var trial, out var action);
            OnElementChanged(block, trial, action, false, "", _toggle.isOn);

        }

        public void UpdateUI() {
            if (_isToggle)
            {
                _toggle.isOn = _currentBool;
            }
            else if (_isInputField) {
                _inputField.SetTextWithoutNotify(_currentString);
            }else
            {
                _text?.SetText(_currentString);
            }
        }

        private void OnDisable()
        {
            if (Type.ToString().StartsWith("Action"))
            {
                ServiceLocator.ExperienceService.OnCurrentActionChanged -= ExperienceService_OnElementChanged;
            }
            else if (Type.ToString().StartsWith("Trial"))
            {
                ServiceLocator.ExperienceService.OnCurrentTrialChanged -= ExperienceService_OnElementChanged;
            }
            else if (Type.ToString().StartsWith("Block"))
            {
                ServiceLocator.ExperienceService.OnCurrentBlockChanged -= ExperienceService_OnElementChanged;
            }
            if (_inputField != null && _allowedToUpdateData)
            {
                _inputField.onEndEdit.RemoveListener(InputFieldChanged);
            }
        }

        private void GetCurrentElements(out Block block, out Trial trial, out Models.Action action)
        {
            action = ServiceLocator.ExperienceService.CurrentAction;
            trial = ServiceLocator.ExperienceService.CurrentTrial;
            block = ServiceLocator.ExperienceService.CurrentBlock;
            if (_displayNext)
            {
                action = ServiceLocator.ExperienceService.GetNext<Models.Action>();
                trial = ServiceLocator.ExperienceService.GetNext<Trial>();
                block = ServiceLocator.ExperienceService.GetNext<Block>();
            }
        }
        private void InputFieldChanged(string text)
        {

            GetCurrentElements(out var block, out var trial, out var action);
            OnElementChanged(block, trial, action, false, text);
        }

        private void ExperienceService_OnElementChanged<T>(object sender, T e) where T : BaseDataObject
        {
            GetCurrentElements(out var block, out var trial, out var action);
            OnElementChanged(block, trial, action);
        }

        private void OnElementChanged(Block block, Trial trial, Models.Action action, bool getInfo = true, string text = "", bool isOn = false)
        {
            switch (Type)
            {
                case ExperienceElement.ActionName:
                    if (getInfo)
                    {
                        _currentString = action == null ? "" : action.Name;
                        UpdateUI();
                    }
                    else if (!_isToggle)
                    {
                        action.Name = text;
                    }
                    break;
                case ExperienceElement.ActionDescription:
                    if (getInfo)
                    {
                        _currentString = (action == null ? "" : action.Description);
                        UpdateUI();
                    }
                    else if (!_isToggle)
                    {
                        action.Description = text;
                    }
                    break;
                case ExperienceElement.ActionIsRequired:
                    if (getInfo)
                    {
                       _currentBool = action.IsRequired;
                        UpdateUI();
                    }
                    else if (_isToggle && _allowedToUpdateData)
                    {
                        action.IsRequired = isOn;
                    }
                    break;

                case ExperienceElement.TrialName:
                    if (getInfo)
                    {
                        _currentString = (trial == null ? "" : trial.Name);
                        UpdateUI();
                    }
                    else if (!_isToggle)
                    {
                        trial.Name = text;
                    }
                    break;
                case ExperienceElement.TrialDescription:
                    if (getInfo)
                    {
                        _currentString = (trial == null ? "" : trial.Description);
                        UpdateUI();
                    }
                    else if (!_isToggle)
                    {
                        trial.Description = text;
                    }
                    break;
                case ExperienceElement.TrialIsRequired:
                    if (getInfo)
                    {
                        _currentBool = trial.IsRequired;
                        UpdateUI();
                    }
                    else if (_isToggle && _allowedToUpdateData)
                    {
                        trial.IsRequired = isOn;
                    }
                    break;

                case ExperienceElement.BlockName:
                    if (getInfo)
                    {
                        _currentString = (block == null ? "" : block.Name);
                        UpdateUI();
                    }
                    else if (!_isToggle)
                    {
                        block.Name = text;
                    }
                    break;
                case ExperienceElement.BlockDescription:
                    if (getInfo)
                    {
                        _currentString = (block == null ? "" : block.Description);
                        UpdateUI();
                    }
                    else if (!_isToggle)
                    {
                        block.Description = text;
                    }
                    break;
                case ExperienceElement.BlockIsBreak:
                    if (getInfo)
                    {
                        _currentBool = block?.IsBreak ?? false;
                        UpdateUI();
                    }
                    else if (_isToggle && _allowedToUpdateData)
                    {
                        block.IsBreak = isOn;
                    }
                    break;
                case ExperienceElement.BlockDuration: 
                    if (getInfo)
                    {
                        _currentString = (block == null ? "" : text);
                        UpdateUI();
                    }
                    else if (_isInputField)
                    {
                        try
                        {
                            float number = float.Parse(text);
                            block.Duration = number;
                        }
                        catch {
                            Debug.LogError("Inputfield for duration block pause has unrecognized characters. Please use only numbers. Using default value 0 for now.");
                            block.Duration = 0;
                        }
                    }
                    break;
                default:
                    Debug.LogWarning($"{Type} hasnt been declared yet. Get to codin, woman!");
                    break;
            }
        }

        [Serializable]
        public enum ExperienceElement
        {
            Order = 0,
            ActionName = 20,
            ActionDescription = 21,
            ActionOrderNameDescription = 22,
            ActionTimestamp = 23,
            ActionIsRequired = 24,
            ActionDuration = 25,
            TrialName = 50,
            TrialDescription = 51,
            TrialDuration = 52,
            TrialIsRequired = 53,
            TrialStatus = 54,
            TrialAccuracy = 55,
            BlockName = 80,
            BlockDescription = 81,
            BlockIsBreak = 82,
            BlockDuration = 83,
            BlockAccuracy = 84,
            TrialCompletionStatus = 56,
        }
    }

}
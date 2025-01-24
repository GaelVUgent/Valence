using MICT.eDNA.Interfaces;
using MICT.eDNA.Managers;
using MICT.eDNA.Models;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

namespace MICT.eDNA.View
{
    public class ActionTrigger : MonoBehaviour
    {
        public UnityEvent OnTriggerEntered;
        public UnityEvent OnTriggerExited;
        public ActionSelector SelectedAction;
        //TODO: replace Actionmodel here with ActionConfiguration model
        private Action _action, _overrideAction;
        //TODO: replace Trialmodel here with TrialConfiguration model
        private List<Trial> _allowTriggerOnSelectedTrials;
        [Tooltip("Leave empty if you want to send an event on all trials")]
        public List<TrialSelector> AllowTriggerOnSelectedTrials;
        public bool HideMesh = true;
        [SerializeField, Tooltip("Leave empty if you want to send an event when colliding against anything")]
        protected Collider[] _collidersToSendEvent;
        [Tooltip("Actions needed to be sent before allowing this one")]
        public List<ActionSelector> RequiredPreviousActions;
        public bool IsInteractable = true;
        protected List<Action> _requiredPreviousActions;
        protected IExperienceService _experienceService;
        protected bool _isDoubleTapEnter = false, _isDoubleTapExit = false;
        protected Coroutine _runCheckDoubleTapEnter, _runCheckDoubleTapExit;
        private float _doubleTapDuration = 0.1f;
        protected bool _objectIsInTrigger = false;
        protected bool _oneOfTheSelectedTrialsIsActive = false;
        protected HashSet<Collider> _collidersInside = new HashSet<Collider>();

        private void Awake()
        {
            ServiceLocator.DataService.OnDataLoaded += OnDataLoaded;
            if (ServiceLocator.DataService.Data != null)
            {
                OnDataLoaded(this, null);
            }
            ServiceLocator.ExperienceService.OnCurrentTrialChanged += ExperienceService_OnCurrentTrialChanged;
            if (ServiceLocator.ExperienceService.CurrentTrial != null)
            {
                ExperienceService_OnCurrentTrialChanged(this, ServiceLocator.ExperienceService.CurrentTrial);
            }

            if (HideMesh && GetComponent<Renderer>() != null)
            {
                GetComponent<Renderer>().enabled = false;
            }
        }

        private void Start()
        {
            _experienceService = ServiceLocator.ExperienceService;
            if (!(_allowTriggerOnSelectedTrials?.Count > 0))
            {
                _oneOfTheSelectedTrialsIsActive = true;
            }
        }

        /*private void Update()
        {
            if ((!_objectIsInTrigger && _collidersInside.Count > 0) || (_objectIsInTrigger && _collidersInside.Count == 0)){
                print("something hinky");
            }
        }*/

        protected virtual void OnTriggerEnter(Collider other)
        {
            _collidersInside.Add(other);
            _objectIsInTrigger = true;
            if (_isDoubleTapEnter)
            {
                return;
            }
            else
            {
                _isDoubleTapEnter = true;
                if (_runCheckDoubleTapEnter == null)
                {
                    _runCheckDoubleTapEnter = StartCoroutine(RunWait(() => {
                        _isDoubleTapEnter = false;
                        _runCheckDoubleTapEnter = null;
                    }));
                }
                bool shouldTrigger = false;
                if (_collidersToSendEvent == null || _collidersToSendEvent.Length == 0)
                {
                    shouldTrigger = true;
                }
                else
                {
                    foreach (var coll in _collidersToSendEvent)
                    {
                        if (coll == other)
                        {
                            shouldTrigger = true;
                            break;
                        }
                    }
                }
                if (shouldTrigger)
                {
                    SendAction();

                }
            }
        }

        protected virtual void OnTriggerExit(Collider other)
        {
            if (_collidersInside.Contains(other)) {
                _collidersInside.Remove(other);
            }
            _objectIsInTrigger = false;
            if (_isDoubleTapExit)
            {
                return;
            }
            else
            {
                if (_runCheckDoubleTapExit == null)
                {
                    _isDoubleTapExit = true;
                    _runCheckDoubleTapExit = StartCoroutine(RunWait(() => {
                        _isDoubleTapExit = false;
                        _runCheckDoubleTapExit = null;
                    }));
                }
                bool shouldTrigger = false;
                if (_collidersToSendEvent == null || _collidersToSendEvent.Length == 0)
                {
                    shouldTrigger = true;
                }
                else
                {
                    foreach (var coll in _collidersToSendEvent)
                    {
                        if (coll == other)
                        {
                            shouldTrigger = true;
                            break;
                        }
                    }
                }
                if (shouldTrigger)
                {
                    if (_oneOfTheSelectedTrialsIsActive)
                    {
                        OnTriggerExited?.Invoke();
                    }

                }
            }
        }

        private void OnDestroy()
        {
            ServiceLocator.DataService.OnDataLoaded -= OnDataLoaded;
            ServiceLocator.ExperienceService.OnCurrentTrialChanged -= ExperienceService_OnCurrentTrialChanged;
        }

        public void ToggleInteractivity(bool isOn) {
            IsInteractable = isOn;

            if (IsInteractable && _collidersInside?.Count > 0) {
                bool shouldTrigger = false;
                if (_collidersToSendEvent == null || _collidersToSendEvent.Length == 0)
                {
                    shouldTrigger = true;
                }
                else
                {
                    var validCols = _collidersInside.Intersect(_collidersToSendEvent);
                    if (validCols?.Count() > 0) {
                        shouldTrigger = true;
                    }
                }
                if (shouldTrigger)
                {
                    SendAction();
                }
            }

        }

        public void SetAction(Action action)
        {
            if (_overrideAction != action)
                _overrideAction = action;
        }

        public void SendAction()
        {
            if (!IsInteractable)
                return;
            if (_requiredPreviousActions?.Count > 0) {
                foreach (var item in _requiredPreviousActions)
                {
                    if (!ServiceLocator.ExperienceService.IsActionInHistory(item)) {
                        Debug.Log($"Tried to trigger action ({_action?.Name}) but did not meet required action: ({item?.Name})");
                        return;
                    }
                }
            }

            if (_oneOfTheSelectedTrialsIsActive)
            {
                //getting action again. we might've changed the editor field in the editor for testing purposes :)
#if UNITY_EDITOR
                _action = ServiceLocator.DataService.GetActionFromSelector(SelectedAction);
#endif
                _experienceService.RegisterAction(_overrideAction ?? _action);
                OnTriggerEntered?.Invoke();
            }
        }

        private IEnumerator RunWait(System.Action completionHandler)
        {
            
            yield return new WaitForSeconds(_doubleTapDuration);
            completionHandler?.Invoke();
        }

        private void ExperienceService_OnCurrentTrialChanged(object sender, Trial e)
        {
            if (e == null)
                return;
            if (_allowTriggerOnSelectedTrials?.Count > 0)
            {
                _oneOfTheSelectedTrialsIsActive = _allowTriggerOnSelectedTrials.Select(x => x.ConfigId).Contains(e.ConfigId);
            }
            else
            {
                _oneOfTheSelectedTrialsIsActive = true;
            }
            if (_objectIsInTrigger && !_oneOfTheSelectedTrialsIsActive)
            {
                _objectIsInTrigger = false;
                OnTriggerExited?.Invoke();
            }
        }

        protected virtual void OnDataLoaded(object sender, System.EventArgs e)
        {
            _action = ServiceLocator.DataService.GetActionFromSelector(SelectedAction);
            _allowTriggerOnSelectedTrials = new List<Trial>();
            foreach (var item in AllowTriggerOnSelectedTrials)
            {
                _allowTriggerOnSelectedTrials.Add(ServiceLocator.DataService.GetTrialFromSelector(item));
            }
            _requiredPreviousActions = new List<Action>();
            foreach (var item in RequiredPreviousActions)
            {
                _requiredPreviousActions.Add(ServiceLocator.DataService.GetActionFromSelector(item));
            }
        }
    }
}
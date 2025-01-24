using MICT.eDNA.Managers;
using MICT.eDNA.Models;
using UnityEngine;

namespace MICT.eDNA.View
{
    public class MultipleActionTrigger : ActionTrigger
    {
        [Header("Different action if triggered by specific colliders")]
        public ActionSelector CustomAction;
        //TODO: replace Actionmodel here with ActionConfiguration model
        private Action _customAction;
        [SerializeField]
        protected Collider[] _collidersToSendCustomEvent;

        protected override void OnTriggerEnter(Collider other)
        {
            if (!_isDoubleTapEnter)
            {
                bool shouldTrigger = false;
                if (_collidersToSendCustomEvent != null && _collidersToSendCustomEvent.Length > 0)
                {
                    
                    foreach (var coll in _collidersToSendCustomEvent)
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
                    SendCustomAction();
                }
            }
            base.OnTriggerEnter(other);
        }

        protected override void OnTriggerExit(Collider other)
        {
            if (!_isDoubleTapExit)
            {
                bool shouldTrigger = false;
                if (_collidersToSendCustomEvent != null && _collidersToSendCustomEvent.Length > 0)
                {
                    foreach (var coll in _collidersToSendCustomEvent)
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
            base.OnTriggerExit(other);
        }

        public void SendCustomAction()
        {
            if (!IsInteractable)
                return;
            if (_requiredPreviousActions?.Count > 0) {
                foreach (var item in _requiredPreviousActions)
                {
                    if (!ServiceLocator.ExperienceService.IsActionInHistory(item)) {
                        Debug.Log($"Tried to trigger action ({_customAction?.Name}) but did not meet required action: ({item?.Name})");
                        return;
                    }
                }
            }

            if (_oneOfTheSelectedTrialsIsActive)
            {
                //getting action again. we might've changed the editor field in the editor for testing purposes :)
#if UNITY_EDITOR
                _customAction = ServiceLocator.DataService.GetActionFromSelector(CustomAction);
#endif
                _experienceService.RegisterAction(_customAction);
                OnTriggerEntered?.Invoke();
            }
        }
        
        protected override void OnDataLoaded(object sender, System.EventArgs e)
        {
            base.OnDataLoaded(sender, e);
            _customAction = ServiceLocator.DataService.GetActionFromSelector(CustomAction);
        }
    }
}

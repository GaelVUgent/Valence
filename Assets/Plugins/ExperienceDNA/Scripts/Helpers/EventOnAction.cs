using MICT.eDNA.Managers;
using MICT.eDNA.Models;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace MICT.eDNA.Helpers
{
    public class EventOnAction : MonoBehaviour
    {
        public UnityEvent OnActionCalled;
        public List<ActionSelector> Actions = new List<ActionSelector>();
        //TODO: replace Actionmodel here with ActionConfiguration model
        private List<Action> _actions = new List<Action>();


        private void Start()
        {
            ServiceLocator.DataService.OnDataLoaded += OnDataLoaded;
            if (ServiceLocator.DataService.Data != null)
            {
                OnDataLoaded(this, null);
            }
        }

        private void OnDestroy()
        {
            ServiceLocator.ExperienceService.OnCurrentActionChanged -= OnChange;
            ServiceLocator.DataService.OnDataLoaded -= OnDataLoaded;
        }

        private void OnChange(object sender, Action u)
        {
            if (_actions != null && _actions.Contains(u))
            {
                OnActionCalled?.Invoke();
            }
        }

        private void OnDataLoaded(object sender, System.EventArgs e)
        {
            foreach (var item in Actions)
            {
                _actions.Add(ServiceLocator.DataService.GetActionFromSelector(item));
            }

            ServiceLocator.ExperienceService.OnCurrentActionChanged += OnChange;
            if (ServiceLocator.ExperienceService.CurrentAction != null)
            {
                OnChange(this, ServiceLocator.ExperienceService.CurrentAction);
            }
        }
    }
}
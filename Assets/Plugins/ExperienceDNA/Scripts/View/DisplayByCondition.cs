using MICT.eDNA.Managers;
using MICT.eDNA.Models;
using System.Collections.Generic;
using UnityEngine;

namespace MICT.eDNA.View
{
    public class DisplayByCondition : MonoBehaviour
    {
        public List<ConditionSelector> EnableOnCondition = new List<ConditionSelector>();
        [Tooltip("Leave empty if you want to disable on all")]
        public List<ConditionSelector> DisableOnCondition = new List<ConditionSelector>();

        private List<Condition> _enableOnCondition = new List<Condition>();
        private List<Condition> _disableOnCondition = new List<Condition>();

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
            ServiceLocator.ExperienceService.OnActiveConditionsChanged -= OnChange;
            ServiceLocator.DataService.OnDataLoaded -= OnDataLoaded;
        }

        private void OnChange(object sender, HashSet<Condition> u)
        {
            if (_disableOnCondition?.Count == 0)
            {
                gameObject.SetActive(false);
            }
            else
            {
                foreach (var item in _disableOnCondition)
                {
                    if (u.Contains(item))
                    {
                        gameObject.SetActive(false);
                        break;
                    }
                }
            }

            if (_enableOnCondition?.Count == 0)
            {
                gameObject.SetActive(true);
            }
            else
            {
                foreach (var item in _enableOnCondition)
                {
                    if (u.Contains(item))
                    {
                        gameObject.SetActive(true);
                        break;
                    }
                }
            }
        }

        private void OnDataLoaded(object sender, System.EventArgs e)
        {
            foreach (var item in EnableOnCondition)
            {
                _enableOnCondition.Add(ServiceLocator.DataService.GetConditionFromSelector(item));
            }
            foreach (var item in DisableOnCondition)
            {
                _disableOnCondition.Add(ServiceLocator.DataService.GetConditionFromSelector(item));
            }

            ServiceLocator.ExperienceService.OnActiveConditionsChanged += OnChange;
            if (ServiceLocator.ExperienceService.CurrentActiveConditions != null)
            {
                OnChange(this, ServiceLocator.ExperienceService.CurrentActiveConditions);
            }
        }
    }
}
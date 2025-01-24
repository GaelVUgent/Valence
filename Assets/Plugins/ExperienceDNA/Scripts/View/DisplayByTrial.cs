using MICT.eDNA.Managers;
using MICT.eDNA.Models;
using System.Collections.Generic;
using UnityEngine;

namespace MICT.eDNA.View
{
    public class DisplayByTrial : MonoBehaviour
    {
        public List<TrialSelector> EnableOnTrial = new List<TrialSelector>();
        [Tooltip("Leave empty if you want to disable on all")]
        public List<TrialSelector> DisableOnTrial = new List<TrialSelector>();
        //TODO: replace Trialmodel here with TrialConfiguration model
        private List<Trial> _enableOnTrial = new List<Trial>();
        private List<Trial> _disableOnTrial = new List<Trial>();

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
            ServiceLocator.ExperienceService.OnCurrentTrialChanged -= OnChange;
            ServiceLocator.DataService.OnDataLoaded -= OnDataLoaded;
        }

        private void OnChange(object sender, Trial u)
        {
            if (_enableOnTrial?.Count == 0 || _enableOnTrial.Contains(u))
            {
                gameObject.SetActive(true);
            }
            else if (_disableOnTrial?.Count == 0 || _disableOnTrial.Contains(u)) {
                gameObject.SetActive(false);
            }
        }

        private void OnDataLoaded(object sender, System.EventArgs e)
        {
            foreach (var item in EnableOnTrial)
            {
                _enableOnTrial.Add(ServiceLocator.DataService.GetTrialFromSelector(item));
            }
            foreach (var item in DisableOnTrial)
            {
                _disableOnTrial.Add(ServiceLocator.DataService.GetTrialFromSelector(item));
            }

            ServiceLocator.ExperienceService.OnCurrentTrialChanged += OnChange;
            if (ServiceLocator.ExperienceService.CurrentTrial != null)
            {
                OnChange(this, ServiceLocator.ExperienceService.CurrentTrial);
            }
        }
    }
}
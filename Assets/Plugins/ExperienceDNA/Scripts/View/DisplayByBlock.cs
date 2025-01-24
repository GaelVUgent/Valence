using MICT.eDNA.Managers;
using MICT.eDNA.Models;
using System.Collections.Generic;
using UnityEngine;

namespace MICT.eDNA.View
{
    public class DisplayByBlock : MonoBehaviour
    {
        public List<BlockSelector> EnableOnBlock = new List<BlockSelector>();
        [Tooltip("Leave empty if you want to disable on all")]
        public List<BlockSelector> DisableOnBlock = new List<BlockSelector>();
        //TODO: replace Blockmodel here with BlockConfiguration model
        private List<Block> _enableOnBlock = new List<Block>();
        private List<Block> _disableOnBlock = new List<Block>();

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
            ServiceLocator.ExperienceService.OnCurrentBlockChanged -= OnChange;
            ServiceLocator.DataService.OnDataLoaded -= OnDataLoaded;
        }

        private void OnChange(object sender, Block u)
        {
            if (_enableOnBlock?.Count == 0 || _enableOnBlock.Contains(u))
            {
                gameObject.SetActive(true);
            }
            else if (_disableOnBlock?.Count == 0 || _disableOnBlock.Contains(u)) {
                gameObject.SetActive(false);
            }
        }

        private void OnDataLoaded(object sender, System.EventArgs e)
        {
            foreach (var item in EnableOnBlock)
            {
                _enableOnBlock.Add(ServiceLocator.DataService.GetBlockFromSelector(item));
            }
            foreach (var item in DisableOnBlock)
            {
                _disableOnBlock.Add(ServiceLocator.DataService.GetBlockFromSelector(item));
            }

            ServiceLocator.ExperienceService.OnCurrentBlockChanged += OnChange;
            if (ServiceLocator.ExperienceService.CurrentBlock != null)
            {
                OnChange(this, ServiceLocator.ExperienceService.CurrentBlock);
            }
        }
    }
}
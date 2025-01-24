using MICT.eDNA.Managers;
using MICT.eDNA.Models;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using UnityEngine.Events;

namespace MICT.eDNA.View
{
    public class DisplayByAction : MonoBehaviour
    {
        public UnityEvent OnEnabled;
        public UnityEvent OnDisabled;
        public UnityEventWithString OnActivatedWithParameter;
        public List<ActionSelector> EnableOnAction = new List<ActionSelector>();
        [Tooltip("Leave empty if you want to disable on all")]
        public List<ActionSelector> DisableOnAction = new List<ActionSelector>();
        public bool KeepActiveOnceEnabled = true;
        
        public bool UseMeshRendererInstead = false;
        [Tooltip("Use as override when you want to use the events but not change the gameobject activeself state")]
        public bool DontToggleGameObjectActiveState = false;
        //TODO: replace Actionmodel here with ActionConfiguration model
        private List<Action> _enableOnAction = new List<Action>();
        private List<Action> _disableOnAction = new List<Action>();
        private bool _isInitialDisable = true;
        private Renderer _renderer;

        private void Start()
        {
            _renderer = GetComponent<Renderer>();
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
            if (!_isInitialDisable && (u == null || u?.ConfigId == -100))
            {
                return;
            }
            if (_enableOnAction?.Count == 0 || _enableOnAction.Contains(u))
            {

                if ((!UseMeshRendererInstead && !gameObject.activeSelf) || (UseMeshRendererInstead && !_renderer.enabled))
                {
                    if (!UseMeshRendererInstead)
                    {
                        if(!DontToggleGameObjectActiveState) 
                            gameObject.SetActive(true);
                    }
                    else
                        _renderer.enabled = true;
                    OnEnabled?.Invoke();
                    
                }
                if (!string.IsNullOrEmpty(u?.Parameter?.ToString()))
                {
                    OnActivatedWithParameter?.Invoke(u?.Parameter?.ToString());
                }
            }
            else if (_disableOnAction?.Count == 0 || _disableOnAction.Contains(u))
            {
                if (!_isInitialDisable && KeepActiveOnceEnabled && ((!UseMeshRendererInstead && gameObject.activeSelf) || (UseMeshRendererInstead && _renderer.enabled)))
                {
                    return;
                }
                if ((!UseMeshRendererInstead && gameObject.activeSelf) || (UseMeshRendererInstead && _renderer.enabled))
                {
                    _isInitialDisable = false;
                    if (!UseMeshRendererInstead)
                    {
                        if (!DontToggleGameObjectActiveState)
                            gameObject.SetActive(false);
                    }
                    else
                        _renderer.enabled = false;
                    OnDisabled?.Invoke();
                }
            }
        }

        private void OnDataLoaded(object sender, System.EventArgs e)
        {
            foreach (var item in EnableOnAction)
            {
                _enableOnAction.Add(ServiceLocator.DataService.GetActionFromSelector(item));
            }
            foreach (var item in DisableOnAction)
            {
                _disableOnAction.Add(ServiceLocator.DataService.GetActionFromSelector(item));
            }

            ServiceLocator.ExperienceService.OnCurrentActionChanged += OnChange;
            if (ServiceLocator.ExperienceService.CurrentAction != null)
            {
                OnChange(this, ServiceLocator.ExperienceService.CurrentAction);
            }
        }
    }
}
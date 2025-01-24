using MICT.eDNA.Managers;
using MICT.eDNA.Models;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace MICT.eDNA.View
{
    public class DisplayByRoleAndMonitor : MonoBehaviour
    {
        public Canvas Canvas;
        private Dictionary<UserRole, Monitor> _statesPerRole = new Dictionary<UserRole, Monitor>();
        public List<Monitor> States = new List<Monitor>();

        private void Start()
        {
            foreach (var item in States)
            {
                _statesPerRole.Add(item.Role,item);
            }
            ServiceLocator.UserService.OnRoleSet += UserService_OnRoleSet;
            if (ServiceLocator.UserService.CurrentUser != null)
            {
                UserService_OnRoleSet(this, ServiceLocator.UserService.CurrentUser);
            }
        }

        private void OnDestroy()
        {
            ServiceLocator.UserService.OnRoleSet -= UserService_OnRoleSet;
        }

        private void UserService_OnRoleSet(object sender, Models.User u)
        {
            if (_statesPerRole.ContainsKey(u.Role))
            {
                gameObject.SetActive(_statesPerRole[u.Role].GameObjectActiveState);
                Canvas.targetDisplay = _statesPerRole[u.Role].Display;
            }
        }
    }
    [Serializable]
    public class Monitor{
        public UserRole Role;
        public bool GameObjectActiveState;
        public int Display;
    }
}
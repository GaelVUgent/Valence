using MICT.eDNA.Managers;
using MICT.eDNA.Models;
using System.Collections.Generic;
using UnityEngine;

namespace MICT.eDNA.View
{
    public class DisplayByRole : MonoBehaviour
    {
        public List<UserRole> EnableOnRole = new List<UserRole>();
        public List<UserRole> DisableOnRole = new List<UserRole>();

        private void Start()
        {
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
            if (EnableOnRole.Contains(u.Role))
            {
                gameObject.SetActive(true);
            }
            else if (DisableOnRole.Contains(u.Role)) {
                gameObject.SetActive(false);
            }
        }
    }
}
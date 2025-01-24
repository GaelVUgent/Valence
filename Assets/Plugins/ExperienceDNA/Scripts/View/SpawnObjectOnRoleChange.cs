using MICT.eDNA.Managers;
using MICT.eDNA.Models;
using UnityEngine;

namespace MICT.eDNA.View
{
    public class SpawnObjectOnRoleChange : BaseSpawnObject
    {
        [SerializeField]
        private UserRole _role;
        [SerializeField]
        private bool _destroyOnRoleChange = false;

        protected override void Start()
        {
            base.Start();
            ServiceLocator.UserService.OnRoleSet += UserService_OnRoleSet;            
            if (ServiceLocator.UserService.CurrentUser?.Role == _role)
            {
                UserService_OnRoleSet(this, ServiceLocator.UserService.CurrentUser);
            }
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            ServiceLocator.UserService.OnRoleSet -= UserService_OnRoleSet;
        }

       
        private void UserService_OnRoleSet(object sender, User e)
        {
            if (e.Role == _role)
            {
                CreateInstance(ref _createdInstance);
            }
            else if (_destroyOnRoleChange)
            {
                DestroyInstance(ref _createdInstance);
            }
        }
    }
}

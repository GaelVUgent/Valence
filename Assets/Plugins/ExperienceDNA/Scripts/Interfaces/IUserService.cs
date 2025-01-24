using MICT.eDNA.Models;
using System;

namespace MICT.eDNA.Interfaces
{
    public interface IUserService
    {
        event EventHandler<User> OnRoleSet;
        User CurrentUser { get; set; }
        void OverrideParticipantNumber(int i);       
        void AssignUserRole(UserRole role);
        [Obsolete]
        void SetPlayer(User player);
        void CreateNewUser(bool usePreviousProperties = true);
    } 
}

using MICT.eDNA.Interfaces;
using MICT.eDNA.Models;
using System;
using UnityEngine;

namespace MICT.eDNA.Services
{
    public class UserService: IUserService
    {
        public event EventHandler<User> OnRoleSet;
        public User CurrentUser { get; set; }
        private const string _participantIdKey = "ParticipantId";
        private int _participantNumber = -1;

        public UserService()
        {
            CurrentUser = new User() { Role = UserRole.Undefined, ParticipantNumber = _participantNumber };
            if (PlayerPrefs.GetInt(_participantIdKey) > -1)
            {
                CurrentUser.ParticipantNumber = PlayerPrefs.GetInt(_participantIdKey) + 1;
                PlayerPrefs.SetInt(_participantIdKey, CurrentUser.ParticipantNumber);
                PlayerPrefs.Save();
            }
        }

        [Obsolete]
        public void SetPlayer(User player)
        {
        }

        public void CreateNewUser(bool usePreviousProperties = true)
        {
            var currentUserNumber = CurrentUser.ParticipantNumber;
            currentUserNumber++;

            CurrentUser = new User() { Role = usePreviousProperties ? CurrentUser.Role : UserRole.Undefined, ParticipantNumber = currentUserNumber };
            OverrideParticipantNumber(currentUserNumber);
            
        }

        public void OverrideParticipantNumber(int i) {
            CurrentUser.ParticipantNumber = i;
            if (PlayerPrefs.GetInt(_participantIdKey) == i)
            {
                Debug.LogWarning($"WARNING: We're overriding the participant number to {i} but we already have a data file with that number. Is this intended behaviour?");
            }
            PlayerPrefs.SetInt(_participantIdKey, CurrentUser.ParticipantNumber);
            PlayerPrefs.Save();
        }

        //NOTE: Good start but note how the two methods are almost exactly the same. This should be an indicator that you can write this in a more efficient manner. E.g. AssignRole(UserRole r)
        public void AssignUserRole (UserRole role)
        {
            CurrentUser.Role = role;
            OnRoleSet?.Invoke(this, CurrentUser);
        }
    } 
}
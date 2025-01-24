using System;

namespace MICT.eDNA.Models
{
    [Serializable]
    public class User
    {
        public int ParticipantNumber;
        public UserRole Role;
        public string Name
        {
            get { return _name; }
            set {
                _name = value;
                FullName = $"{_name} {UnityEngine.SystemInfo.deviceName}";
            }
        }
        private string _name;
        public string FullName { get; protected set; }
    } 
}

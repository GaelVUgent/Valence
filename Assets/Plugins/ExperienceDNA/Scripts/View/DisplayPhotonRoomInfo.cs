using TMPro;
using Photon.Pun;
using UnityEngine;
using MICT.eDNA.Managers;

namespace MICT.eDNA.View
{
    public class DisplayPhotonRoomInfo : MonoBehaviour
    {
        public TMP_Text StatusText;
        string _cache = null;
        private string _stringFormat = "Photon room name: {0}";

        private void Start()
        {
            if (StatusText == null)
            {
                StatusText = GetComponent<TMP_Text>();
            }
        }

        void Update()
        {
            if (StatusText == null)
            {
                return;
            }

            if (ServiceLocator.DataService.Data.Name != _cache)
            {
                _cache = ServiceLocator.DataService.Data.Name;
                StatusText?.SetText(string.Format(_stringFormat, _cache));
            }
            else
            {
                if (_cache == null)
                {
                    _cache = null;
                    StatusText?.SetText(string.Format(_stringFormat, "n/a"));
                }
            }
            
            if (PhotonNetwork.CurrentRoom != null)
            {
                if ((PhotonNetwork.CurrentRoom.CustomProperties["nm"] != _cache))
                {
                    _cache = PhotonNetwork.CurrentRoom.CustomProperties["nm"] as string;
                    StatusText?.SetText(string.Format(_stringFormat, _cache));
                }
            }
            else
            {
                if (_cache == null)
                {
                    _cache = null;
                    StatusText?.SetText(string.Format(_stringFormat, "n/a"));
                }
            }
        }
    }
}
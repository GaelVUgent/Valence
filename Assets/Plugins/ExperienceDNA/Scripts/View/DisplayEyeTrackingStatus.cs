using TMPro;
using UnityEngine;
using ViveSR.anipal.Eye;

namespace MICT.eDNA.View
{
    public class DisplayEyeTrackingStatus : MonoBehaviour
    {
        public TMP_Text StatusText;
        private string _stringFormat = "Eye tracking status: {0}";
        
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
            if (SRanipal_Eye_Framework.Instance == null || !SRanipal_Eye_Framework.Instance.EnableEye)
            {
                StatusText?.SetText(string.Format(_stringFormat, "not enabled yet"));
                return;
            }
            StatusText?.SetText(string.Format(_stringFormat, SRanipal_Eye_Framework.Status.ToString()));
        }
    }
}

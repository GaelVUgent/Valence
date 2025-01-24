using Photon.Pun;
using UnityEngine;

namespace MICT.eDNA.Helpers
{
    public class PhotonTag : MonoBehaviourPunCallbacks, IPunInstantiateMagicCallback
    {
        public void OnPhotonInstantiate(PhotonMessageInfo info)
        {
            info.Sender.TagObject = (object)(this.gameObject);
        }
    }

}
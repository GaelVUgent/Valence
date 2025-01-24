using MICT.eDNA.Controllers;
using MICT.eDNA.View;
using UnityEngine;

namespace MICT.eDNA.Helpers
{
    public class LinkDynamicEyeDataCapturerToOutput : MonoBehaviour
    {
        public BaseSpawnObject AvatarSpawner;
        public OutputController OutputController;
        private EyeDataCapturer _eyeDataCapturer = null;
        
        void Start()
        {
            if (AvatarSpawner != null)
            {
                if (AvatarSpawner?.GetSpawnedObject() != null)
                {
                    AvatarSpawner_OnObjectSpawned(this, AvatarSpawner.GetSpawnedObject());
                }
                AvatarSpawner.OnObjectSpawned += AvatarSpawner_OnObjectSpawned;
            }
        }

        private void OnDestroy()
        {
            AvatarSpawner.OnObjectSpawned -= AvatarSpawner_OnObjectSpawned;
        }

        private void AvatarSpawner_OnObjectSpawned(object sender, GameObject e)
        {
            GetEyeDataCapturer(e);

            if (_eyeDataCapturer == null)
            {
                var photonSpawn = e?.GetComponent<BaseSpawnObject>();
                if (photonSpawn != null)
                {
                    photonSpawn.OnObjectSpawned += (object sender2, GameObject e2) => GetEyeDataCapturer(e2);
                }
            }
        }

        private void GetEyeDataCapturer(GameObject ob)
        {
            _eyeDataCapturer = ob.GetComponentInChildren<EyeDataCapturer>(true);
            if (_eyeDataCapturer != null)
            {
                OutputController.SetEyeDataCapturer(_eyeDataCapturer, ob.GetComponentInChildren<TextMesh>(true));
            }
        }
    } 
}

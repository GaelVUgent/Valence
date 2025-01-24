using UnityEngine;

namespace Manus.Haptics
{
    /// <summary>
    /// This is the class which needs to be on a joint of a finger of a hand, it keeps track of the amount of collisions.
    /// The JointHaptics is used by the FingerHaptics in order to determine the amount of haptic feedback that should be given.
    /// </summary>
	[DisallowMultipleComponent]
    public class JointHaptics : MonoBehaviour
    {
        public int collisions
        {
            get
            {
                return m_Colliders;
            }
        }

        int m_Colliders = 0;

        private void OnCollisionEnter(Collision p_Collision)
        {
            m_Colliders++;
        }

        private void OnCollisionExit(Collision p_Collision)
        {
            m_Colliders--;
            if (m_Colliders < 0)
                m_Colliders = 0;
        }

        private void OnTriggerEnter(Collider p_Other)
        {
            if (p_Other.isTrigger) return;
            m_Colliders++;
        }

        private void OnTriggerExit(Collider p_Other)
        {
            if (p_Other.isTrigger) return;
            m_Colliders--;
            if (m_Colliders < 0)
                m_Colliders = 0;
        }

        /// <summary>
        /// Resets the number of collisions to 0
        /// </summary>
        public void ResetHaptics()
		{
            m_Colliders = 0;
		}

    }
}

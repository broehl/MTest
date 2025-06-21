using UnityEngine;

namespace ConestogaMultiplayer
{
    public class TrackerReferences : MonoBehaviour
    {
        public Transform headTracker, leftHandTracker, rightHandTracker, xrOrigin;
        public static TrackerReferences instance;
        private void Awake() => instance = this;
    }
}

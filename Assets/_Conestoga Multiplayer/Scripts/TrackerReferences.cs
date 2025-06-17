using UnityEngine;

namespace ConestogaMultiplayer
{
    public class TrackerReferences : MonoBehaviour
    {
        public Transform headTracker, leftHandTracker, rightHandTracker;
        public static TrackerReferences instance;
        private void Awake() => instance = this;
    }
}

// Keep references to the trackers and XR Origin

// Written by Bernie Roehl, June 2025

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

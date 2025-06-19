using Unity.Netcode;
using UnityEngine;

namespace ConestogaMultiplayer
{
    public class Player : NetworkBehaviour
    {
        [SerializeField] Transform head, leftHand, rightHand;

        private void LateUpdate()
        {
            if (!IsOwner) return;
            head.SetPositionAndRotation(TrackerReferences.instance.headTracker.position, TrackerReferences.instance.headTracker.rotation);
            leftHand.SetPositionAndRotation(TrackerReferences.instance.leftHandTracker.position, TrackerReferences.instance.leftHandTracker.rotation);
            rightHand.SetPositionAndRotation(TrackerReferences.instance.rightHandTracker.position, TrackerReferences.instance.rightHandTracker.rotation);
        }
    }
}

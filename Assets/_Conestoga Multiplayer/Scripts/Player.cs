// The netcode Player script

// Written by Bernie Roehl, June 2025

// Updates the pose of our body parts based on controller inputs

using Unity.Netcode;
using Unity.XR.CoreUtils;
using UnityEngine;

namespace ConestogaMultiplayer
{
    public class Player : NetworkBehaviour
    {
        [SerializeField] Transform head, leftHand, rightHand;

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            if (!IsOwner) return;
            GameObject[] spawnpoints = GameObject.FindGameObjectsWithTag("Respawn");
            if (spawnpoints.Length > 0)
            {
                Transform spawnpoint = spawnpoints[((int) OwnerClientId) % spawnpoints.Length].transform;
                GameObject.FindObjectOfType<XROrigin>().transform.SetPositionAndRotation(spawnpoint.position, spawnpoint.rotation);
            }
        }

        private void LateUpdate()
        {
            if (!IsOwner) return;
            head.SetPositionAndRotation(TrackerReferences.instance.headTracker.position, TrackerReferences.instance.headTracker.rotation);
            leftHand.SetPositionAndRotation(TrackerReferences.instance.leftHandTracker.position, TrackerReferences.instance.leftHandTracker.rotation);
            rightHand.SetPositionAndRotation(TrackerReferences.instance.rightHandTracker.position, TrackerReferences.instance.rightHandTracker.rotation);
        }
    }
}

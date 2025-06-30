// Claim ownership of an object when we grab it in VR

// Written by Bernie Roehl, June 2025

using Unity.Netcode;
using UnityEngine.XR.Interaction.Toolkit;

namespace ConestogaMultiplayer
{
    public class ClaimOwnershipOnGrab : NetworkBehaviour
    {
        NetworkObject networkObject;

        private void Awake()
        {
            networkObject = GetComponent<NetworkObject>();
            XRGrabInteractable interactable = GetComponent<XRGrabInteractable>();
            interactable.selectEntered.AddListener(OnSelect);
            interactable.selectExited.AddListener(OnDeselect);
        }

        private void OnSelect(SelectEnterEventArgs _)
        {
            if (IsClient && !IsOwner) RequestOwnershipRpc(NetworkManager.Singleton.LocalClientId);
        }

        public void OnDeselect(SelectExitEventArgs _)
        {
            if (IsOwner) ReleaseOwnershipRpc();
        }

        [Rpc(SendTo.Server)]
        void RequestOwnershipRpc(ulong newClientId) => networkObject.ChangeOwnership(newClientId);

        [Rpc(SendTo.Server)]
        void ReleaseOwnershipRpc() => networkObject.RemoveOwnership();

    }
}
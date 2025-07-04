// Take ownership of an object when we grab it in VR

// Written by Bernie Roehl, June 2025

using Unity.Netcode;
using UnityEngine.XR.Interaction.Toolkit;

namespace ConestogaMultiplayer
{
    public class TakeOwnershipOnGrab : NetworkBehaviour
    {
        NetworkObject networkObject;  // only used on server

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            if (IsServer) networkObject = GetComponent<NetworkObject>();
            IXRSelectInteractable interactable = GetComponent<IXRSelectInteractable>();
            interactable.selectEntered.AddListener(OnSelect);
            interactable.selectExited.AddListener(OnDeselect);
        }

        private void OnSelect(SelectEnterEventArgs args)
        {
            if (IsClient && !IsOwner) RequestOwnershipRpc(NetworkManager.Singleton.LocalClientId);
        }

        public void OnDeselect(SelectExitEventArgs args)
        {
            if (IsOwner) ReleaseOwnershipRpc();
        }

        [Rpc(SendTo.Server)]
        void RequestOwnershipRpc(ulong newClientId) => networkObject.ChangeOwnership(newClientId);

        [Rpc(SendTo.Server)]
        void ReleaseOwnershipRpc() => networkObject.RemoveOwnership();
    }
}
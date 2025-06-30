// Claim ownership of an object when we grab it in VR

// Written by Bernie Roehl, June 2025

using Unity.Netcode;
using UnityEngine.XR.Interaction.Toolkit;

namespace ConestogaMultiplayer
{
    public class ClaimOwnershipOnGrab : NetworkBehaviour
    {
        NetworkObject networkObject;

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            networkObject = GetComponent<NetworkObject>();
            IXRSelectInteractable interactable = GetComponent<IXRSelectInteractable>();
            interactable.selectEntered.AddListener(OnSelect);
            interactable.selectExited.AddListener(OnDeselect);
        }

        private void OnSelect(SelectEnterEventArgs args)
        {
            if (!(args.interactorObject is XRDirectInteractor || args.interactorObject is XRRayInteractor)) return;
            print("Select");
            if (IsClient && !IsOwner)
            {
                print("Change ownership");
                RequestOwnershipRpc(NetworkManager.Singleton.LocalClientId);
            }
        }

        public void OnDeselect(SelectExitEventArgs args) 
        {
            if (!(args.interactorObject is XRDirectInteractor || args.interactorObject is XRRayInteractor)) return;
            print("Deselect");
            if (IsOwner)
            {
                ReleaseOwnershipRpc();
                print("Release ownership");
            }
        }

        [Rpc(SendTo.Server)]
        void RequestOwnershipRpc(ulong newClientId) => networkObject.ChangeOwnership(newClientId);

        [Rpc(SendTo.Server)]
        void ReleaseOwnershipRpc() => networkObject.RemoveOwnership();

    }
}
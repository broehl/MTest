using Unity.Netcode;

public class ClaimOwnershipOnGrab : NetworkBehaviour
{
    public void OnSelect()
    {
        if (IsClient && !IsOwner)
        {
            print($"Requesting ownership by {NetworkManager.Singleton.LocalClientId}");
            RequestOwnershipRpc(NetworkManager.Singleton.LocalClientId);
        }
    }

    [Rpc(SendTo.Server)]
    void RequestOwnershipRpc(ulong newClientId)
    {
        print($"Got ownership change request from {newClientId}");
        GetComponent<NetworkObject>().ChangeOwnership(newClientId);
    }
}

using Unity.Netcode;

public class ClaimOwnershipOnGrab : NetworkBehaviour
{
    public void OnSelect()
    {
        if (IsClient && !IsOwner) RequestOwnershipRpc(NetworkManager.Singleton.LocalClientId);
    }

    [Rpc(SendTo.Server)]
    void RequestOwnershipRpc(ulong newClientId)
    {
        GetComponent<NetworkObject>().ChangeOwnership(newClientId);
    }
}

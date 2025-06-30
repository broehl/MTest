using Unity.Netcode;
using UnityEngine;

public class LightController : NetworkBehaviour
{
    Light lightSource;

    NetworkVariable<bool> lightState = new NetworkVariable<bool>();

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        lightSource = GetComponent<Light>();
        lightState.OnValueChanged += UpdateLightState;
        if (IsOwner) lightState.Value = lightSource.enabled;
    }

    void UpdateLightState(bool _, bool newvalue) => lightSource.enabled = newvalue;

    [Rpc(SendTo.Server)]
    public void ToggleRpc() => lightState.Value = !lightState.Value;
}

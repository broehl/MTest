using UnityEngine;

public class NonNetworkLightController : MonoBehaviour
{
    Light lightSource;

    bool lit;

    void Start()
    {
        lightSource = GetComponent<Light>();
        lit = lightSource.enabled;
    }

    public void ToggleRpc()
    {
        lit = !lit;
        lightSource.enabled = lit;
    }
}


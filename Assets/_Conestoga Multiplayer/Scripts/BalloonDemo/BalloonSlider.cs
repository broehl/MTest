// Set the balloon's height (on the server) based on the slider position

// Written by Bernie Roehl, June 2025

using Unity.Netcode;
using UnityEngine.XR.Content.Interaction;

namespace ConestogaMultiplayerBalloonDemo
{
    public class BalloonSlider : NetworkBehaviour
    {
        XRSlider slider;
        private void Awake() => slider = GetComponent<XRSlider>();

        private void Update()
        {
            if (IsOwner) BalloonGameLogic.instance.SetBalloonHeightRpc(slider.value);
        }
    }
}
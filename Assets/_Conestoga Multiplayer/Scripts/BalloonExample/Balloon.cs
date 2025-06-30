using Unity.Netcode;
using UnityEngine;

namespace ConestogaMultiplayer
{
    public class Balloon : NetworkBehaviour
    {
        [Rpc(SendTo.ClientsAndHost)]
        public void PopRpc()
        {
            GetComponent<Renderer>().enabled = false;
            GetComponent<ParticleSystem>()?.Play();
            GetComponent<AudioSource>()?.Play();
        }
    }
}

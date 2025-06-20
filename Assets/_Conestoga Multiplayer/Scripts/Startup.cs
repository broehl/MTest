// Handle network discovery

// Written by Bernie Roehl, June 2025

using System;
using System.Collections;
using NetworkDiscoveryUnity;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;

namespace ConestogaMultiplayer
{
    public class Startup : MonoBehaviour
    {
        public enum Mode { CLIENT, SERVER, HOST }
        [SerializeField] Mode mode = Mode.CLIENT;

        bool gotServer = false;

        NetworkDiscovery discovery;
        private void Awake() => discovery = GetComponent<NetworkDiscovery>();

        void Start()
        {
            // If we're running as an executable and specify a mode on the command line,
            // it overrides the default set in the Inspector.
            string[] args = Environment.GetCommandLineArgs();
            foreach (string arg in args)
            {
                switch (arg.ToLower())
                {
                    case "client": mode = Mode.CLIENT; break;
                    case "server": mode = Mode.SERVER; break;
                    case "host": mode = Mode.HOST; break;
                }
            }

            // Start in the appropriate mode
            switch (mode)
            {
                case Mode.SERVER:
                    discovery.EnsureServerIsInitialized();
                    NetworkManager.Singleton.StartServer();
                    break;
                case Mode.HOST:
                    discovery.EnsureServerIsInitialized();
                    NetworkManager.Singleton.StartHost();
                    break;
                case Mode.CLIENT:
                    StartCoroutine(AskForServers());
                    break;
            }
        }

        private IEnumerator AskForServers()
        {
            WaitForSeconds delay = new WaitForSeconds(1);
            while (!gotServer)
            {
                discovery.SendBroadcast();
                yield return delay;
            }
        }

        void OnEnable() => discovery.onReceivedServerResponse.AddListener(FoundServer);
        void OnDisable() => discovery.onReceivedServerResponse.RemoveListener(FoundServer);

        private void FoundServer(NetworkDiscovery.DiscoveryInfo info)
        {
            gotServer = true;  // we marry the first person who proposes to us
            UnityTransport transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
            transport.SetConnectionData(info.EndPoint.Address.ToString(), info.GetGameServerPort());
            NetworkManager.Singleton.StartClient();
        }
    }
}

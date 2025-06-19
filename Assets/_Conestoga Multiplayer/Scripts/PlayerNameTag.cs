// Nametag above each avatar's head

// Written by Bernie Roehl, June 2025

using TMPro;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

namespace ConestogaMultiplayer
{
    public class PlayerNameTag : NetworkBehaviour
    {
        [SerializeField] private TextMeshProUGUI nameTagText;

        private Transform cameraTransform;
        private Transform nameTagTransform;

        NetworkVariable<FixedString32Bytes> networkedNameTag = new NetworkVariable<FixedString32Bytes>();

        private void Awake()
        {
            cameraTransform = Camera.main.transform;
            nameTagTransform = nameTagText.GetComponentInParent<Canvas>().transform;
        }


        void Update()
        {
            Quaternion lookRot = Quaternion.LookRotation(nameTagTransform.position - cameraTransform.position).normalized;
            nameTagTransform.rotation = Quaternion.Euler(new Vector3(0, lookRot.eulerAngles.y, 0));
        }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            networkedNameTag.OnValueChanged += UpdateNametag;
            if (IsOwner) ServerSetNameRPC(System.Environment.MachineName);
            else UpdateNametag("", networkedNameTag.Value);
        }

        [Rpc(SendTo.Server)]
        void ServerSetNameRPC(FixedString32Bytes name)
        {
            networkedNameTag.Value = name;
        }

        private void UpdateNametag(FixedString32Bytes oldname, FixedString32Bytes newname)
        {
            nameTagText.text = networkedNameTag.Value.ToString();
        }

    }
}
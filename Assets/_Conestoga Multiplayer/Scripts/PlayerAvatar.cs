// The visible avatar that the player is wearing

// Written by Bernie Roehl, June 2025

using System;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;

namespace ConestogaMultiplayer
{
    public class PlayerAvatar : NetworkBehaviour
    {
        [SerializeField] GameObject[] avatarPrefabs;
        [SerializeField] Transform headAnchor, leftHandAnchor, rightHandAnchor;   // tracked body parts (or offset children of them)
        [SerializeField] bool behead = false;  // if true, hide the local player's head
        [SerializeField] float bodyAlignmentRate = 0.01f;

        public GameObject playerAvatar { get; private set; }
        [HideInInspector] public UnityEvent<GameObject> playerAvatarChangedEvent = new UnityEvent<GameObject>();

        AvatarReferences refs;  // references to the various parts of the avatar (head, hands, etc)

        // these variables are used to scale the avatar to match the player
        NetworkVariable<float> networkedPlayerHeight = new NetworkVariable<float>(1.6f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
        float avatarHeight;  // this gets set when the avatar is first loaded

        NetworkVariable<int> networkedAvatarNumber = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            networkedPlayerHeight.OnValueChanged += UpdatePlayerHeight;
            networkedAvatarNumber.OnValueChanged += UpdateAvatarNumber;
            if (avatarPrefabs.Length == 0) return;  // no avatars!
            if (IsOwner) networkedAvatarNumber.Value = ((int)NetworkManager.Singleton.LocalClientId) % avatarPrefabs.Length;
            UpdateAvatarNumber(-1, networkedAvatarNumber.Value);
            if (IsOwner) SetPlayerHeight();
            UpdatePlayerHeight(-1, networkedPlayerHeight.Value);
        }

        public override void OnNetworkDespawn()
        {
            base.OnNetworkDespawn();
            if (!enabled) return;
            networkedPlayerHeight.OnValueChanged -= UpdatePlayerHeight;
            networkedAvatarNumber.OnValueChanged -= UpdateAvatarNumber;
            if (playerAvatar) Destroy(playerAvatar);
        }

        void UpdateAvatarNumber(int previousValue, int newValue)
        {
            if (playerAvatar) Destroy(playerAvatar);
            playerAvatar = LoadAvatar(avatarPrefabs[newValue]);
        }

        GameObject LoadAvatar(GameObject avatarPrefab)
        {
            GameObject avatarRoot = Instantiate(avatarPrefab);
            refs = avatarRoot.GetComponent<AvatarReferences>();
            avatarHeight = refs.headBone.position.y;
            if (IsOwner && behead) refs.headBone.localScale = Vector3.zero;
            playerAvatarChangedEvent.Invoke(playerAvatar);
            return avatarRoot;
        }

        void SetPlayerHeight()
        {
            if (TrackerReferences.instance?.headTracker?.position.y > 0)
                networkedPlayerHeight.Value = TrackerReferences.instance.headTracker.position.y;
        }

        void UpdatePlayerHeight(float oldheight, float newheight) => ResizeAvatar();

        void ResizeAvatar() => playerAvatar.transform.localScale = (networkedPlayerHeight.Value / avatarHeight) * Vector3.one;

        void LateUpdate()
        {
            if (playerAvatar == null || refs == null) return;
            playerAvatar.transform.position = refs.headIK_target.position - networkedPlayerHeight.Value * Vector3.up;
            Quaternion destinationRotation = Quaternion.Euler(playerAvatar.transform.eulerAngles.x, refs.headIK_target.eulerAngles.y, playerAvatar.transform.eulerAngles.z);
            playerAvatar.transform.rotation = Quaternion.Lerp(playerAvatar.transform.rotation, destinationRotation, bodyAlignmentRate);
            refs.headIK_target.SetPositionAndRotation(headAnchor.position, headAnchor.rotation);
            refs.leftArmIK_target.SetPositionAndRotation(leftHandAnchor.position, leftHandAnchor.rotation);
            refs.rightArmIK_target.SetPositionAndRotation(rightHandAnchor.position, rightHandAnchor.rotation);
            // resize player when spacebar is pressed
            if (IsOwner && playerAvatar && Input.GetKeyDown(KeyCode.Space)) SetPlayerHeight();
        }
    }
}

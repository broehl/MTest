// The visible avatar that the player is wearing

// Written by Bernie Roehl, June 2025

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

        AvatarReferences refs;

        // these variables are used to scale the avatar to match the player
        NetworkVariable<float> networkedPlayerHeight = new NetworkVariable<float>();
        float avatarHeight;  // this gets set when the avatar is first loaded

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            playerAvatar = LoadAvatar(avatarPrefabs[((int)OwnerClientId) % avatarPrefabs.Length]);
            networkedPlayerHeight.OnValueChanged += OnUpdatePlayerHeight;
            if (IsOwner) SetPlayerHeight();
            ResizeAvatar();
        }

        public override void OnNetworkDespawn()
        {
            base.OnNetworkDespawn();
            Destroy(playerAvatar);
        }

        private GameObject LoadAvatar(GameObject avatarPrefab)
        {
            GameObject avatarRoot = Instantiate(avatarPrefab);
            refs = avatarRoot.GetComponent<AvatarReferences>();
            avatarHeight = refs.headBone.position.y;
            if (IsOwner && behead) refs.headBone.localScale = Vector3.zero;
            playerAvatarChangedEvent.Invoke(playerAvatar);
            return avatarRoot;
        }

        void SetPlayerHeight() => ServerSetPlayerHeightRPC(TrackerReferences.instance.headTracker.position.y);
        [Rpc(SendTo.Server)] void ServerSetPlayerHeightRPC(float height) => networkedPlayerHeight.Value = height;
        void OnUpdatePlayerHeight(float oldheight, float newheight) => ResizeAvatar();
        void ResizeAvatar() => playerAvatar.transform.localScale = (networkedPlayerHeight.Value / avatarHeight) * Vector3.one;

        void LateUpdate()
        {
            if (playerAvatar)
            {
                playerAvatar.transform.position = refs.headIK_target.position - networkedPlayerHeight.Value * Vector3.up;
                Quaternion destinationRotation = Quaternion.Euler(playerAvatar.transform.eulerAngles.x, refs.headIK_target.eulerAngles.y, playerAvatar.transform.eulerAngles.z);
                playerAvatar.transform.rotation = Quaternion.Lerp(playerAvatar.transform.rotation, destinationRotation, bodyAlignmentRate);
            }
            if (refs)
            {
                refs.headIK_target.SetPositionAndRotation(headAnchor.position, headAnchor.rotation);
                refs.leftArmIK_target.SetPositionAndRotation(leftHandAnchor.position, leftHandAnchor.rotation);
                refs.rightArmIK_target.SetPositionAndRotation(rightHandAnchor.position, rightHandAnchor.rotation);
            }
            // resize player when spacebar is pressed
            if (IsOwner && playerAvatar && Input.GetKeyDown(KeyCode.Space)) SetPlayerHeight();
        }
    }
}

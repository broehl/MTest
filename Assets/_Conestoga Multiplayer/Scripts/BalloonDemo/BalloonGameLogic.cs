// Game logic for the balloon interactions

// Written by Bernie Roehl, June 2025

using Unity.Netcode;
using UnityEngine;

namespace ConestogaMultiplayerBalloonDemo
{
    public class BalloonGameLogic : NetworkBehaviour
    {
        [SerializeField] private GameObject balloonPrefab;
        [SerializeField] private Transform balloonLaunchPoint;
        [SerializeField] private float balloonMaxHeight = 3;

        GameObject balloon;
        bool popped = false;

        public static BalloonGameLogic instance;
        private void Awake() => instance = this;

        [Rpc(SendTo.Server)]
        public void CreateBalloonRpc()
        {
            if (balloon) return;  // only allow one balloon
            balloon = Instantiate(balloonPrefab, balloonLaunchPoint);
            balloon.GetComponent<NetworkObject>().Spawn();
        }

        [Rpc(SendTo.Server)]
        public void SetBalloonHeightRpc(float height)
        {
            if (balloon == null) return;
            balloon.transform.position = balloonLaunchPoint.position + height * balloonMaxHeight * Vector3.up;
        }

        private void Update()
        {
            if (!IsServer) return;
            if (balloon && !popped && balloon.transform.position.y >= balloonMaxHeight)
            {
                popped = true;
                balloon.GetComponent<Balloon>().PopRpc();
            }
        }
    }
}

// References to IK targets and head bone

// Written by Bernie Roehl, June 2025

using UnityEngine;

namespace ConestogaMultiplayer
{
    public class AvatarReferences : MonoBehaviour
    {
        public Transform headIK_target, leftArmIK_target, rightArmIK_target;
        public Transform headBone;
        public AudioSource mouthAudio;
    }
}
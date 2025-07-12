using System.Collections;
using UnityEngine;

public class AutoBlink : MonoBehaviour
{
    [SerializeField, Range(1, 10), Tooltip("How long to wait between blinks")]
    private float blinkInterval = 3f;

    [SerializeField, Range(0, 2), Tooltip("How long the eyes stay closed while blinking")]
    private float blinkDuration = 0.1f;

    [SerializeField, Tooltip("The SkinnedMeshRenderer that has the eyelids")]
    private SkinnedMeshRenderer headMesh;

    IEnumerator Start()
    {
        int eyeBlinkLeftBlendShapeIndex = headMesh.sharedMesh.GetBlendShapeIndex("eyeBlinkLeft");
        int eyeBlinkRightBlendShapeIndex = headMesh.sharedMesh.GetBlendShapeIndex("eyeBlinkRight");
        while (true)
        {
            yield return new WaitForSeconds(blinkInterval);
            headMesh.SetBlendShapeWeight(eyeBlinkLeftBlendShapeIndex, 1);
            headMesh.SetBlendShapeWeight(eyeBlinkRightBlendShapeIndex, 1);
            yield return new WaitForSeconds(blinkDuration);
            headMesh.SetBlendShapeWeight(eyeBlinkLeftBlendShapeIndex, 0);
            headMesh.SetBlendShapeWeight(eyeBlinkRightBlendShapeIndex, 0);
        }
    }
}

using System.Collections;
using UnityEngine;

public class AutoGaze : MonoBehaviour
{
    [SerializeField, Tooltip("How long between saccades")]
    private float newGazeTime = 3;
    
    [SerializeField]
    private Transform leftEyeBone, rightEyeBone;

    public bool enableRandomGazing = true;

    private const int VERTICAL_MARGIN = 15, HORIZONTAL_MARGIN = 5;  // range of angles to gaze in

    IEnumerator Start()
    {
        while (true)
        {
            yield return new WaitForSeconds(newGazeTime);
            if (enableRandomGazing) {
                float vertical = Random.Range(-VERTICAL_MARGIN, VERTICAL_MARGIN);
                float horizontal = Random.Range(-HORIZONTAL_MARGIN, HORIZONTAL_MARGIN);
                leftEyeBone.localRotation = rightEyeBone.localRotation = Quaternion.Euler(horizontal, vertical, 0);
            }
        }
    }
}

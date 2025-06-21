using UnityEngine;

public class IKFootSolver : MonoBehaviour
{
    [SerializeField] LayerMask terrainLayer = default;
    [SerializeField] Transform body = default;
    [SerializeField] IKFootSolver otherFoot = default;
    [SerializeField] float speed = 4;
    [SerializeField] float stepDistance = 0.2f;
    [SerializeField] float stepLength = 0.2f;
    [SerializeField] float sideStepLength = 0.1f;
    [SerializeField] float stepHeight = 0.3f;
    [SerializeField] Vector3 footOffset = default;
    [SerializeField] Vector3 footRotOffset;
    [SerializeField] float footYPosOffset = 0.1f;
    [SerializeField] float rayStartYOffset = 0;
    [SerializeField] float rayLength = 1.5f;
    
    float footSpacing;
    Vector3 oldPosition, currentPosition, newPosition;

    float lerp = 1;
    public bool IsMoving() => lerp < 1;

    private void Start()
    {
        footSpacing = transform.localPosition.x;
        currentPosition = newPosition = oldPosition = transform.position;
    }

    void Update()
    {
        transform.position = currentPosition + footYPosOffset * Vector3.up;
        transform.localRotation = Quaternion.Euler(footRotOffset);

        Ray ray = new Ray(body.position + footSpacing * body.right + rayStartYOffset * Vector3.up, Vector3.down);
        Debug.DrawRay(body.position + footSpacing * body.right + rayStartYOffset * Vector3.up, Vector3.down);
        if (Physics.Raycast(ray, out RaycastHit info, rayLength, terrainLayer.value))
        {
            if (Vector3.Distance(newPosition, info.point) > stepDistance && !otherFoot.IsMoving() && lerp >= 1)
            {
                lerp = 0;
                Vector3 direction = Vector3.ProjectOnPlane(info.point - currentPosition, Vector3.up).normalized;
                float angle = Vector3.Angle(body.forward, body.InverseTransformDirection(direction));
                if (angle < 50 || angle > 130)
                    newPosition = info.point + direction * stepLength + footOffset;
                else
                    newPosition = info.point + direction * sideStepLength + footOffset;
            }
        }

        if (lerp < 1)
        {
            Vector3 tempPosition = Vector3.Lerp(oldPosition, newPosition, lerp);
            tempPosition.y += Mathf.Sin(lerp * Mathf.PI) * stepHeight;
            currentPosition = tempPosition;
            lerp += Time.deltaTime * speed;
        }
        else
        {
            oldPosition = newPosition;
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(newPosition, 0.1f);
    }
}

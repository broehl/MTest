using UnityEngine;

public class Pouring : MonoBehaviour
{
    [SerializeField] GameObject bottle, bowlLiquid, flow;
    [SerializeField] float bottleMinLevel, bottleMaxLevel;
    [SerializeField] float bowlMinLevel, bowlMaxLevel;
    [SerializeField] float pourRate = 0.01f;

    float bowlLevel = 0, bottleLevel = 1;  // bowl starts off empty, bottle starts off full

    Material bottleLiquidMaterial;
    Material bowlLiquidMaterial;

    LineRenderer flowLineRenderer;

    void Start()
    {
        bottleLiquidMaterial = bottle.GetComponent<Renderer>().materials[1];
        bowlLiquidMaterial = bowlLiquid.GetComponent<Renderer>().materials[0];
        flowLineRenderer = flow.GetComponent<LineRenderer>();
    }

    void Update()
    {
        flow.SetActive(false);  // we only see the flow while pouring
        float bottleAngle = Vector3.Angle(bottle.transform.up, Vector3.up);
        if (bottleAngle > 90)   // bottle is past the horizontal
        {
            float rate = pourRate * (bottleAngle - 90) / 90;

            bottleLevel -= rate * Time.deltaTime;
            bottleLiquidMaterial.SetFloat("_Fill", Mathf.Lerp(bottleMinLevel, bottleMaxLevel, Mathf.Clamp01(bottleLevel)));

            Vector3 flowStartPosition = flow.transform.TransformPoint(flowLineRenderer.GetPosition(0));
            if (Physics.Raycast(flowStartPosition, Vector3.down, out RaycastHit hit))
            {
                print(hit.collider.name);
                Vector3 hitPoint = hit.point;
                if (hit.collider.name == bowlLiquid.name)
                {
                    bowlLevel += rate * Time.deltaTime;
                    bowlLiquidMaterial.SetFloat("_Fill", Mathf.Lerp(bowlMinLevel, bowlMaxLevel, Mathf.Clamp01(bowlLevel)));
                    print($"Parent is {bowlLiquid.transform.parent.name}");
                    hitPoint = bowlLiquid.transform.parent.position;
                }
                flowLineRenderer.SetPosition(1, flow.transform.InverseTransformPoint(hitPoint));
                flow.SetActive(bottleLevel > 0);
            }
        }
    }
}

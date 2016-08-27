using UnityEngine;
using System.Collections;

[RequireComponent(typeof(LineRenderer))]
public class ArcUI : MonoBehaviour {

    public int segments = 50;
    public float radius = 0.5f;
    public float width = 0.08f;
    public float offsetX = 0.0f;
    public float offsetY = 0.0f;
    LineRenderer lineRenderer;

    TankController tank;
    float startAngle = 0.0f;
    float endAngle = 0.0f;

    // Use this for initialization
    void Start () {
        if (transform.parent == null)
            throw new MissingComponentException("Missing tank. Perhaps you should use the prefab?");

        tank = transform.parent.GetComponent<TankController>();
        startAngle = tank.startAngle;
        endAngle = tank.endAngle;

        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.SetVertexCount(segments);
        lineRenderer.SetColors(Color.red, Color.red);
	}
	
	// Update is called once per frame
	void LateUpdate()
    {
        Vector3[] arcPoints = new Vector3[segments];
        float angle = startAngle;
        float arcLength = endAngle - startAngle;
        for (int i = 0; i < segments; i++)
        {
            float x = Mathf.Sin(Mathf.Deg2Rad * angle) * radius;
            float y = Mathf.Cos(Mathf.Deg2Rad * angle) * radius;

            arcPoints[i] = new Vector3((tank.transform.position.x + x) + offsetX, (tank.transform.position.y + y) + offsetY, 0.25f);

            angle += (arcLength / segments);
        }
        lineRenderer.SetPositions(arcPoints);
    }
}
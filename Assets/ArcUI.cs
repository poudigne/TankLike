using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(LineRenderer))]
[ExecuteInEditMode]
public class ArcUI : MonoBehaviour
{

    public int segments = 50;
    public float startAngle = 0.0f;
    public float endAngle = 90.0f;
    public float radius = 0.5f;
    public float width = 0.02f;

    LineRenderer lineRenderer;

    void Start()
    {
        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.SetVertexCount(segments);
        lineRenderer.SetWidth(width, width);
        lineRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;

        renderLine();
    }

    void renderLine()
    {
        Vector3[] arcPoints = new Vector3[segments];
        float angle = startAngle;
        float arcLength = endAngle - startAngle;
        for (int i = 0; i < segments; i++)
        {
            float x = Mathf.Sin(Mathf.Deg2Rad * angle) * radius;
            float y = Mathf.Cos(Mathf.Deg2Rad * angle) * radius;

            arcPoints[i] = new Vector3(transform.position.x + x, transform.position.y + y, 0.0f);

            angle += (arcLength / segments);
        }
        Debug.Log(arcPoints.ToString());
        lineRenderer.SetPositions(arcPoints);
    }
}

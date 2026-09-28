using UnityEngine;

public class LineNoise : MonoBehaviour
{
    private LineRenderer _lineRenderer;
    [SerializeField] private float spacing = 0.5f;
    [SerializeField] private float frequency = 0.1f;
    [SerializeField] private int segments = 10;

    void Start()
    {
        _lineRenderer = GetComponent<LineRenderer>();
    }

    void Update()
    {
        _lineRenderer.positionCount = segments;
        int count = _lineRenderer.positionCount;
        Vector3[] points = new Vector3[count];

        for (int i = 0; i < count; i++)
        {
            float x = i * spacing;
            float noiseX = i * frequency + Time.time; 
        
            float y = Mathf.PerlinNoise(noiseX, 0) * 5f;
        
            points[i] = new Vector3(x, y, 0);
        }

        _lineRenderer.SetPositions(points);
    }
}

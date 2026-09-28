using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class VerletMaterial : VerletRope
{
    private Point[,] points;
    private List<Stick> sticks;
    [SerializeField] private int width = 5;
    [SerializeField] private int height = 5;
    [SerializeField] private float spacing = 1f;
    bool wasPinnedBeforeDrag;
    
    private Mesh mesh;
    private Vector3[] vertices;
    private int[] triangles;
    private Vector2[] uvs;

    private void Start()
    {
        InitiateMaterial();
        _camera = Camera.main;
    }

    private void Update()
    {
        UpdateMesh();
        CalculateMouseInteraction();
    }
    
    private void FixedUpdate()
    {
        CalculatePoints();
        CalculateSticks();
    }
    
    private void CalculatePoints()
    {
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                Point point = points[y, x];
                
                if (point.isPinned) continue;

                Vector2 velocity = (point.position - point.oldPosition) * friction;
                point.oldPosition = point.position;
                float windStrength = Mathf.PerlinNoise(Time.time, point.position.y);
                Vector2 windForce = new Vector2(windStrength, 0);
                point.position += velocity + (gravity + windForce) * Mathf.Pow(Time.fixedDeltaTime, 2);
            
                if (point.position.y < floorLevel) {
                    point.position.y = floorLevel;
                }                
            }
        }
    }

    private void CalculateSticks()
    {
        for (int i = 0; i < iterations; i++) 
        {
            foreach (Stick stick in sticks) 
            {
                Vector2 diff = stick.pointA.position - stick.pointB.position;
                float dist = diff.magnitude;
                float error = stick.length - dist;
                Vector2 gradient = diff.normalized * (error * 0.5f); // half from A and half from B 
            
                if (!stick.pointA.isPinned) stick.pointA.position += gradient;
                if (!stick.pointB.isPinned) stick.pointB.position -= gradient;
            }
        }
    }
    
    private void InitiateMaterial()
    {
        SetupMesh();
        points = new Point[height, width];
        sticks = new List<Stick>();

        Vector2 startPos = transform.position;

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                Vector2 pos = startPos + new Vector2(x * spacing, -y * spacing);
                points[y, x] = new Point(pos);

                if (y == 0) points[y, x].isPinned = true;
            }
        }

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                if (x < width - 1)
                    sticks.Add(new Stick(points[y, x], points[y, x + 1], spacing));

                if (y < height - 1)
                    sticks.Add(new Stick(points[y, x], points[y + 1, x], spacing));
            }
        }
        
        SetupMesh();
    }
    
    private void CalculateMouseInteraction()
    {
        Vector2 mousePosScreen = Mouse.current.position.ReadValue();
        float distanceToPlane = Mathf.Abs(_camera.transform.position.z - transform.position.z);
        Vector3 worldMouse = _camera.ScreenToWorldPoint(new Vector3(mousePosScreen.x, mousePosScreen.y, distanceToPlane));
        
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            float closestDist = mouseRange;
            draggedPoint = null;
            
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    Point point = points[y, x];
                    float d = Vector2.Distance(worldMouse, point.position);
                    if (d < closestDist)
                    {
                        closestDist = d;
                        draggedPoint = point;
                    }                    
                }
            }

            if (draggedPoint != null)
            {
                wasPinnedBeforeDrag = draggedPoint.isPinned;
                draggedPoint.isPinned = true;
            }
        }

        if (Mouse.current.leftButton.isPressed && draggedPoint != null)
        {
            draggedPoint.position = worldMouse;
            draggedPoint.oldPosition = worldMouse; 
        }

        if (Mouse.current.leftButton.wasReleasedThisFrame && draggedPoint != null)
        {
            draggedPoint.isPinned = wasPinnedBeforeDrag;
            draggedPoint = null;
        }
    }


    void SetupMesh()
    {
        mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;

        vertices = new Vector3[width * height];
        uvs = new Vector2[width * height];
        triangles = new int[(width - 1) * (height - 1) * 6 * 2]; // drawing on both sides, front and back

        int triIndex = 0;
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                int i = y * width + x;
                uvs[i] = new Vector2((float)x / (width - 1), (float)y / (height - 1));

                if (x < width - 1 && y < height - 1)
                {
                    triangles[triIndex++] = i;
                    triangles[triIndex++] = i + width;
                    triangles[triIndex++] = i + 1;

                    triangles[triIndex++] = i + 1;
                    triangles[triIndex++] = i + width;
                    triangles[triIndex++] = i + width + 1;
                    
                    triangles[triIndex++] = i;
                    triangles[triIndex++] = i + 1; 
                    triangles[triIndex++] = i + width;

                    triangles[triIndex++] = i + 1;
                    triangles[triIndex++] = i + width + 1; 
                    triangles[triIndex++] = i + width;
                }
            }
        }

        mesh.vertices = vertices;
        mesh.uv = uvs;
        mesh.triangles = triangles;
    }

    private void UpdateMesh()
    {
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                int i = y * width + x;
                vertices[i] = transform.InverseTransformPoint(points[y, x].position);
            }

        }

        mesh.vertices = vertices;
        mesh.RecalculateBounds();
        mesh.RecalculateNormals();
    }
}
using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class VerletRope : MonoBehaviour
{
    private Point[] points;
    private Stick[] sticks;
    [SerializeField] private int ropeLength = 5;
    private LineRenderer lineRenderer;
    protected Vector2 gravity = new Vector2(0, -9.81f);
    [SerializeField] protected int iterations = 10;
    [SerializeField] protected float friction = 0.99f;
    [SerializeField] protected int floorLevel;
    [SerializeField] protected float mouseRange = 0.5f;
    protected Camera _camera;
    protected Point draggedPoint = null;    
    
    private void Start()
    {
        InitiateRope();
        _camera = Camera.main;
    }

    private void Update()
    {
        for (int i = 0; i < points.Length; i++)
        {
            lineRenderer.SetPosition(i, points[i].position);
        }

        CalculateMouseInteraction(); 
    }

    private void FixedUpdate()
    {
        CalculatePoints();
        CalculateSticks();
    }

    private void CalculatePoints()
    {
        foreach (Point point in points)
        {
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

    private void CalculateMouseInteraction()
    {
        Vector2 mousePosScreen = Mouse.current.position.ReadValue();
        Vector3 worldMouse = _camera.ScreenToWorldPoint(new Vector3(mousePosScreen.x, mousePosScreen.y, 10f));

        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            float closestDist = mouseRange;
            foreach (var p in points)
            {
                float d = Vector2.Distance(worldMouse, p.position);
                if (d < closestDist)
                {
                    closestDist = d;
                    draggedPoint = p;
                }
            }
        
            if (draggedPoint != null) draggedPoint.isPinned = true;
        }

        if (Mouse.current.leftButton.isPressed && draggedPoint != null)
        {
            draggedPoint.position = worldMouse;
            draggedPoint.oldPosition = worldMouse; 
        }

        if (Mouse.current.leftButton.wasReleasedThisFrame && draggedPoint != null)
        {
            if (draggedPoint != points[0]) 
            {
                draggedPoint.isPinned = false;
            }
            draggedPoint = null;
        }
    }

    private void InitiateRope()
    {
        lineRenderer = GetComponent<LineRenderer>();
        points = new Point[ropeLength];
        sticks = new Stick[ropeLength - 1];
        lineRenderer.positionCount = points.Length;

        float totalDesiredLength = 5f; 
        float segmentLength = totalDesiredLength / (points.Length - 1);
        Vector2 pos = transform.position;
        
        for (int i = 0; i < points.Length; i++)
        {
            points[i] = new Point(new Vector2(pos.x + i * segmentLength, pos.y));

            if (i > 0)
            {
                sticks[i - 1] = new Stick(points[i - 1], points[i], 0.5f);
            }
        }
        
        points[0].isPinned = true;
    }

    private void OnDrawGizmos()
    {
        if (Mouse.current == null || Camera.main == null) return;

        Vector2 mousePos = Mouse.current.position.ReadValue();

        Vector3 screenPosition = new Vector3(mousePos.x, mousePos.y, 10f);

        Vector3 worldPosition = Camera.main.ScreenToWorldPoint(screenPosition);

        Gizmos.color = Color.azure;
        Gizmos.DrawWireSphere(worldPosition, mouseRange);
    }
}

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class CircleMovement : MonoBehaviour
{
    [SerializeField] private float mouseRange = 3f;
    [SerializeField] private float separationRange = 1f;
    [SerializeField] private float mouseForceMultiplier = 3f;
    [SerializeField] private float otherForceMultiplier = 3f;
    [SerializeField] private float alignmentForceMultiplier = 3f;
    [SerializeField] private float groupGravityForceMultiplier = 3f;
    [SerializeField] private float friction = 0.975f;
    private float preventZero = 0.5f;

    private Vector3 velocity;
    private Camera mainCamera;
    private bool isMoving;
    private float maxVelocity = 20f;
    [SerializeField] private float visibilityRange = 5f;
    
    // boundaries
    private float camHeight;
    private float camWidth;
    [SerializeField] private float bounciness = 0.8f;
    private float circleRadius;

    [SerializeField] private bool showVisibilityRange;
    [SerializeField] private bool showMouseRange;

    private void Start()
    {
        mainCamera = Camera.main;
        camHeight = mainCamera.orthographicSize;
        camWidth = camHeight * mainCamera.aspect;
        circleRadius = GetComponent<GeneratedCircle>().Radius;
    }

    private void FixedUpdate()
    {
        ApplyMouseForce();
        ApplySeparationForce();
        ApplyCohesionForce();
        ApplyAlignmentForce(); 
        
        Move();
        
        BounceOfBoundaries();
    }
    
    private void OnDrawGizmos()
    {
        Vector3 position = transform.position;

        if (showMouseRange)
        {
            // mouse range
            Gizmos.color = isMoving ? Color.lightCoral : Color.magenta;
            Gizmos.DrawWireSphere(position, mouseRange);   
        }

        if (showVisibilityRange)
        {
            // visibility range
            Gizmos.color = Color.seaGreen;
            Gizmos.DrawWireSphere(position, visibilityRange);   
        }
    }

    private void Move()
    {
        velocity *= friction;
        velocity = Vector3.ClampMagnitude(velocity, maxVelocity);
        velocity += new Vector3(Random.Range(-0.1f, 0.1f), Random.Range(-0.1f, 0.1f), 0); // noise
        transform.position += velocity * Time.fixedDeltaTime;
    }

    private void ApplyMouseForce()
    {
        Vector3 mousePos = mainCamera.ScreenToWorldPoint(Mouse.current.position.ReadValue());
        mousePos.z = 0;
        
        Vector3 difference = transform.position - mousePos;
        float dist = difference.magnitude;
        if (dist < mouseRange)
        {
            float force = 1f / (dist + preventZero) * mouseForceMultiplier;
            velocity += difference.normalized * (force * Time.fixedDeltaTime);
        }
    }

    private void ApplySeparationForce()
    {
        foreach (GeneratedCircle otherCircle in GeneratedCircle.circles)
        {
            if (otherCircle.gameObject == gameObject) continue;

            Vector3 diff = transform.position - otherCircle.transform.position;
            float dist = diff.magnitude;
        
            if (dist < separationRange)
            {
                float force = 1f / (dist + preventZero) * otherForceMultiplier;
                velocity += diff.normalized * (force * Time.fixedDeltaTime);
            }
        }
    }

    private void ApplyCohesionForce()
    {
        Vector3 midPoint = Vector3.zero;
        int neighboursCount = 0;

        List<GeneratedCircle> circles = GeneratedCircle.circles;
        foreach (GeneratedCircle otherCircle in circles)
        {
            if (otherCircle.gameObject == gameObject) continue;

            float distance = (otherCircle.transform.position - transform.position).magnitude;
            if (distance < Mathf.Pow(visibilityRange, 2))
            {
                midPoint += otherCircle.transform.position;
                neighboursCount++;
            }
        }

        if (neighboursCount > 0)
        {
            Vector3 position = transform.position;
            
            midPoint /= neighboursCount;
            Vector3 direction = midPoint - position;
            velocity += direction.normalized * (groupGravityForceMultiplier * Time.fixedDeltaTime);
            
            Debug.DrawLine(position, midPoint, Color.blue);
        }
    }

    private void ApplyAlignmentForce()
    {
        Vector3 avgVelocity = Vector3.zero;
        int neighboursCount = 0;

        List<GeneratedCircle> circles = GeneratedCircle.circles;
        foreach (GeneratedCircle otherCircle in circles)
        {
            if (otherCircle.gameObject == gameObject) continue;

            float distance = (otherCircle.transform.position - transform.position).magnitude;
            if (distance < Mathf.Pow(visibilityRange, 2))
            {
                avgVelocity += otherCircle.circleMovement.velocity;
                neighboursCount++;
            }
        }

        if (neighboursCount > 0)
        {
            avgVelocity /= neighboursCount;
            velocity += avgVelocity.normalized * (alignmentForceMultiplier * Time.fixedDeltaTime);
        }
    }
    
    private void BounceOfBoundaries()
    {
        Vector3 pos = transform.position;
        float leftLimit = -camWidth + circleRadius;
        float rightLimit = camWidth - circleRadius;
        float bottomLimit = -camHeight + circleRadius;
        float topLimit = camHeight - circleRadius;

        // Poziomo
        if (pos.x < leftLimit)
        {
            pos.x = leftLimit; // DOCISKASZ DO ŚCIANY
            velocity.x = Mathf.Abs(velocity.x) * bounciness; // Wymuszasz ruch w prawo
        }
        else if (pos.x > rightLimit)
        {
            pos.x = rightLimit; // DOCISKASZ DO ŚCIANY
            velocity.x = -Mathf.Abs(velocity.x) * bounciness; // Wymuszasz ruch w lewo
        }

        // Pionowo
        if (pos.y < bottomLimit)
        {
            pos.y = bottomLimit;
            velocity.y = Mathf.Abs(velocity.y) * bounciness;
        }
        else if (pos.y > topLimit)
        {
            pos.y = topLimit;
            velocity.y = -Mathf.Abs(velocity.y) * bounciness;
        }

        transform.position = pos;
    }
}

using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class ConeMovement : MonoBehaviour
{
    [SerializeField] private float friction;
    [SerializeField] private float rotationSpeed = 5f;
    [Range(0.1f, 1f)] [SerializeField] private float noise = 0.25f;
    [Header("Alignment")]
    [SerializeField] private float visibleRange;
    [SerializeField] private float alignmentForce;
    [Header("Separation")]
    [SerializeField] private float separationRange;
    [SerializeField] private float separationForce;
    [Header("Cohesion")]
    [SerializeField] private float cohesionRange;
    [SerializeField] private float cohesionForce;
    [Header("Boundaries")]
    [Range(0f, 20f)] [SerializeField] private float avoidBoundaryForce = 10f;
    [Range(0f, 5f)]  [SerializeField] private float boundaryMargin = 2f;
    private Vector3 velocity;
    private float maxVelocity = 20f;
    private float preventZero = 0.5f;
    List<ConeMovement> shortGrid = new List<ConeMovement>();

    void Update()
    {
        if (velocity != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(velocity) * Quaternion.Euler(90, 0, 0);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
    }
    
    private void FixedUpdate()
    {
        ApplyForces();

        Move();
    }

    private void Move()
    {
        velocity *= friction;
        velocity = Vector3.ClampMagnitude(velocity, maxVelocity);
        velocity += new Vector3(Random.Range(-noise, noise), Random.Range(-noise, noise), Random.Range(-noise, noise)); // noise
        transform.position += velocity * Time.fixedDeltaTime;
    }

    private void ApplyAlignmentForce(List<ConeMovement> cones)
    {
        Vector3 avgVelocity = Vector3.zero;
        int neighboursCount = 0;

        foreach (ConeMovement cone in cones)
        {
            if (cone.gameObject == gameObject) continue;

            float distanceSqr = (cone.transform.position - transform.position).sqrMagnitude;
            if (distanceSqr < Mathf.Pow(visibleRange, 2))
            {
                avgVelocity += cone.velocity;
                neighboursCount++;   
            }
        }

        if (neighboursCount > 0)
        {
            avgVelocity /= neighboursCount;
            velocity += avgVelocity.normalized * (alignmentForce * Time.fixedDeltaTime);
        }
    }
    
    private void ApplyCohesionForce(List<ConeMovement> cones)
    {
        Vector3 midPoint = Vector3.zero;
        int neighboursCount = 0;
        
        foreach (ConeMovement cone in cones)
        {
            if (cone.gameObject == gameObject) continue;    
            
            float distanceSqr = (cone.transform.position - transform.position).sqrMagnitude;
            
            if (distanceSqr < Mathf.Pow(cohesionRange, 2))
            {
                midPoint += cone.transform.position;
                neighboursCount++;   
            }
        }
        
        if (neighboursCount > 0)
        {
            midPoint /= neighboursCount;
            Vector3 position = transform.position;
            Vector3 direction = midPoint - position;
            velocity += direction.normalized * (cohesionForce * Time.fixedDeltaTime);
            
            Debug.DrawLine(position, midPoint, Color.blue);
        }
    }
    
    private void ApplySeparationForce(List<ConeMovement> cones)
    {
        foreach (ConeMovement cone in cones)
        {
            if (cone.gameObject == gameObject) continue;

            Vector3 diff = transform.position - cone.transform.position;
            float dist = diff.magnitude;
        
            if (dist < Mathf.Pow(separationRange, 2))
            {
                float force = 1f / (dist + preventZero) * separationForce;
                velocity += diff.normalized * (force * Time.fixedDeltaTime);
            }
        }
    }

    private void ApplyBoundaryAvoidance()
    {
        Vector3 pos = transform.position;
        Vector3 size = SimulationGenerator.Instance.BoundarySize; // Nasz globalny box
        Vector3 force = Vector3.zero;

        float halfX = size.x / 2f;
        float halfY = size.y / 2f;
        float halfZ = size.z / 2f;

        // Oś X
        if (pos.x > halfX - boundaryMargin) 
            force.x -= 1f; // Pchaj w lewo
        else if (pos.x < -halfX + boundaryMargin) 
            force.x += 1f; // Pchaj w prawo

        // Oś Y
        if (pos.y > halfY - boundaryMargin) 
            force.y -= 1f; // Pchaj w dół
        else if (pos.y < -halfY + boundaryMargin) 
            force.y += 1f; // Pchaj w górę

        // Oś Z
        if (pos.z > halfZ - boundaryMargin) 
            force.z -= 1f; // Pchaj w tył
        else if (pos.z < -halfZ + boundaryMargin) 
            force.z += 1f; // Pchaj w przód

        // Dodajemy siłę do prędkości
        // normalized sprawia, że siła jest stała, niezależnie od tego jak blisko ściany jesteśmy
        if (force != Vector3.zero)
        {
            velocity += force.normalized * (avoidBoundaryForce * Time.fixedDeltaTime);
        }
    }

    private void ApplyForces()
    {
        List<ConeMovement> cones = GetShortenCones();
        ApplyAlignmentForce(cones);
        ApplyCohesionForce(cones);
        ApplySeparationForce(cones);
        ApplyBoundaryAvoidance();
    }

    private List<ConeMovement> GetShortenCones()
    {
        shortGrid.Clear();
        Vector3Int myCell = SimulationGenerator.Instance.PositionToIndex(transform.position);

        for (int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                for (int z = -1; z <= 1; z++)
                {
                    Vector3Int neighborCell = myCell + new Vector3Int(x, y, z);
                
                    if (SimulationGenerator.Instance.grid.TryGetValue(neighborCell, out List<ConeMovement> conesInCell))
                    {
                        shortGrid.AddRange(conesInCell);
                    }
                }
            }
        }

        shortGrid.Remove(this);
        return shortGrid;
    }
    
    public Vector3 Velocity => velocity;
}

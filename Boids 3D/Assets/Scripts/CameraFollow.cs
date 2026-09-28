using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("Follow")]
    [SerializeField] private float distance = 10f; 
    [SerializeField] private float height = 5f;     
    [SerializeField] private float smoothSpeed = 2f; 

    private Vector3 currentVelocity; 

    void LateUpdate() 
    {
        Vector3 center = SimulationGenerator.Instance.GlobalCenter;
        Vector3 flockDir = SimulationGenerator.Instance.GlobalVelocity;

        if (flockDir == Vector3.zero) return; 
        
        Vector3 targetPos = center - (flockDir.normalized * distance) + (Vector3.up * height);
        
        transform.position = Vector3.SmoothDamp(transform.position, targetPos, ref currentVelocity, 1f / smoothSpeed);
        
        transform.LookAt(center + flockDir.normalized * 2f);

        Debug.DrawLine(SimulationGenerator.Instance.GlobalCenter, transform.position, Color.magenta);
    }
}

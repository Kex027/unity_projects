using UnityEngine;
using Random = UnityEngine.Random;

public class SimulationManager : MonoBehaviour
{
    [SerializeField] private int numberOfCircles = 0;
    [SerializeField] private float radius = 5f;
    [SerializeField] private bool showGizmo = true;
    [SerializeField] private GameObject circleObj;

    void Start()
    {
        for (int i = 0; i < numberOfCircles; i++)
        {
            Vector2 randomXY = Random.insideUnitCircle * radius;
            Vector3 newRandomPosition = new Vector3(randomXY.x, randomXY.y, 0);
            // to optimize, use Object Pooling or GPU Instancing
            Instantiate(circleObj, newRandomPosition, Quaternion.identity);
        }
    }
    
    private void OnDrawGizmos()
    {
        if (!showGizmo) return;
        
        Gizmos.color = Color.beige;
        Gizmos.DrawWireSphere(transform.position, radius);
    }
}

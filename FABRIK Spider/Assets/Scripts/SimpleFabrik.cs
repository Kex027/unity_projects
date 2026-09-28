using System.Collections;
using UnityEngine;

public class SimpleFabrik : MonoBehaviour
{
    [Header("Setup")]
    public Transform target;
    public Transform home;
    public int iterations = 10;

    private LineRenderer lineRenderer;

    private Transform[] joints; 
    private float[] boneLengths;
    private Vector3[] positions;
    [Range(0.0f, 1f)]
    [SerializeField] private float jointStiffness = 0.1f;
    public bool takingStep;
    [SerializeField] private float distanceToMakeStep = 1.75f;
    
    void Start()
    {
        lineRenderer = GetComponent<LineRenderer>();
        joints = GetComponentsInChildren<Transform>();
        boneLengths = new float[joints.Length - 1];
        positions = new Vector3[joints.Length];

        for (int i = 0; i < joints.Length - 1; i++)
            boneLengths[i] = Vector3.Distance(joints[i].position, joints[i + 1].position);

        for (int i = 0; i < joints.Length; i++) positions[i] = joints[i].position;
    }

    private void Update()
    {
        for (int i = 0; i < joints.Length; i++) 
            positions[i] = joints[i].position;
        
        MoveLeg();
        DrawDebugLine();
    }

    private void MoveLeg()
    {
        for (int i = 0; i < iterations; i++)
        {
            // I step
            // Backward
            positions[^1] = target.position; // move last element to target
            for (int j = positions.Length - 1; j > 0; j--)
            {
       
                Vector3 diff = positions[j - 1] - positions[j];
                positions[j - 1] = positions[j] + diff.normalized * boneLengths[j - 1];
            }
    
            // II step
            // Forward
            positions[0] = joints[0].position;
            for (int j = 0; j < positions.Length - 1; j++)
            {
                Vector3 dir = (positions[j + 1] - positions[j]).normalized;
                positions[j + 1] = positions[j] + dir * boneLengths[j];
            }

            Vector3 desiredJointDir = transform.up; 
            for (int j = 1; j < positions.Length - 1; j++)
            {
                positions[j] += desiredJointDir * jointStiffness;
            }
        }
    
        for (int j = 0; j < joints.Length; j++)
        {
            joints[j].position = Vector3.Lerp(joints[j].position, positions[j], Time.deltaTime * 10f);
        }
    }

    IEnumerator MoveTargetPoint() {
        Vector3 startPos = target.position;
        float duration = 0.15f; 
        float time = 0;

        while (time < duration) {
            time += Time.deltaTime;
            float progress = time / duration;

            Vector3 currentTarget = Vector3.Lerp(startPos, home.position, progress);
        
            currentTarget.y += Mathf.Sin(progress * Mathf.PI) * 1.2f;

            target.position = currentTarget;
            yield return null;
        }

        target.position = home.position;
        takingStep = false;
    }

    private void DrawDebugLine()
    {
        if (lineRenderer == null) return;

        lineRenderer.positionCount = joints.Length;
        for (int i = 0; i < joints.Length; i++)
        {
            lineRenderer.SetPosition(i, joints[i].position);
        }
    }
    
    public bool NeedsStep() 
    {
        return Vector3.Distance(target.position, home.position) > distanceToMakeStep;
    }
    
    public void TryStep() 
    {
        if (takingStep) return;
        takingStep = true;

        StartCoroutine(MoveTargetPoint());
    }
}

using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class SimulationGenerator : MonoBehaviour
{
    public Vector3 BoundarySize;
    public Vector3 GlobalCenter;
    public Vector3 GlobalVelocity;
    [SerializeField] private int howManyToSpawn;
    [SerializeField] private GameObject cone;
    private Vector3 spawnCubeSize;
    [SerializeField] private int cellSize;
    [SerializeField] private int numberOfCells;
    public Dictionary<Vector3Int, List<ConeMovement>> grid = new Dictionary<Vector3Int, List<ConeMovement>>();
    private Stack<List<ConeMovement>> listPool = new Stack<List<ConeMovement>>();
    [SerializeField] private float radius = 0.1f;
    [SerializeField] private float length = 1.5f;
    [SerializeField] private int segmentsOnBase = 16;
    public Mesh SharedConeMesh { get; private set; }
    
    public static SimulationGenerator Instance { get; private set; }
    
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
        
        QualitySettings.vSyncCount = 0;  // Wyłącza VSync
        Application.targetFrameRate = 9999; // Zdejmuje kaganiec z FPS
        SharedConeMesh = GenerateSharedConeMesh();
        
        spawnCubeSize = new Vector3(
            cellSize * numberOfCells,
            cellSize * numberOfCells,
            cellSize * numberOfCells
        );
        BoundarySize = spawnCubeSize;
    }

    void Start()
    {
        for (int i = 0; i < howManyToSpawn; i++)
        {
            Instantiate(cone, GetRandomPosition(), Quaternion.identity);
        }
    }

    private void FixedUpdate()
    {
        Instance.AssignConesToCells();
    }

    void Update() {
        Vector3 avgPosition = Vector3.zero;
        Vector3 avgVelocity = Vector3.zero;
        foreach(GenerateCone generatedCone in GenerateCone.cones) {
            avgPosition += generatedCone.transform.position;
            avgVelocity += generatedCone.coneMovement.Velocity;
        }
        GlobalCenter = avgPosition / GenerateCone.cones.Count;
        GlobalVelocity = avgVelocity / GenerateCone.cones.Count;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.azure;
        Gizmos.DrawWireCube(transform.position, spawnCubeSize);
    }
    
    Vector3 GetRandomPosition()
    {
        Vector3 randomPos = new Vector3(
            Random.Range(-spawnCubeSize.x / 2f, spawnCubeSize.x / 2f),
            Random.Range(-spawnCubeSize.y / 2f, spawnCubeSize.y / 2f),
            Random.Range(-spawnCubeSize.z / 2f, spawnCubeSize.z / 2f)
        );
    
        return randomPos;
    }
    
    public void AssignConesToCells()
    {
        foreach (var pair in grid)
        {
            pair.Value.Clear();
            listPool.Push(pair.Value);
        }
        
        grid.Clear();
        foreach (var c in GenerateCone.cones)
        {
            Vector3Int gridIndex = PositionToIndex(c.transform.position);
            if (!grid.TryGetValue(gridIndex, out List<ConeMovement> cellList))
            {
                cellList = listPool.Count > 0 ? listPool.Pop() : new List<ConeMovement>();
                grid[gridIndex] = cellList;
            }
            grid[gridIndex].Add(c.coneMovement);
        }
    }
    
    public Vector3Int PositionToIndex(Vector3 conePosition)
    {
        return new Vector3Int(
            Mathf.FloorToInt(conePosition.x / cellSize),
            Mathf.FloorToInt(conePosition.y / cellSize),
            Mathf.FloorToInt(conePosition.z / cellSize)
        );
    }
    
    private Mesh GenerateSharedConeMesh()
    {
        Mesh mesh = new Mesh();
        mesh.name = "Cone";
        // + base center + top vertex
        Vector3[] vertices = new Vector3[segmentsOnBase + 2]; 
        // each triangle has three outerVertices *3
        // base + walls *2
        int[] triangles = new int[segmentsOnBase * 3 * 2];

        // top
        vertices[0] = new Vector3(0, length, 0);
        // base center
        vertices[1] = Vector3.zero;
        
        // calculating vertices placement
        float angleStep = 2 * Mathf.PI / segmentsOnBase;
        for (int i = 0; i < segmentsOnBase; i++)
        {
            float angle = angleStep * i;
            float x = Mathf.Sin(angle) * radius;
            float z = Mathf.Cos(angle) * radius;
            vertices[i + 2] = new Vector3(x, 0, z); // skip two first
        }
        
        // calculating triangles
        // base
        int verticesOffset = 2;
        for (int i = 0; i < segmentsOnBase; i++)
        {
            int current = verticesOffset + i;
            int next = verticesOffset + i + 1 <= segmentsOnBase + 1 ? verticesOffset + i + 1 : 2;
            
            triangles[i * 6 + 0] = 1;
            triangles[i * 6 + 1] = current;
            triangles[i * 6 + 2] = next;
            triangles[i * 6 + 3] = 0;
            triangles[i * 6 + 4] = next;
            triangles[i * 6 + 5] = current;
        }
        
        mesh.Clear(); // clear garbage 
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals(); // for good lighting
        mesh.RecalculateBounds();
        
        return mesh;
    }
}

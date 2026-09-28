using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class GeneratedCircle : MonoBehaviour
{
    public CircleMovement circleMovement;
    public static List<GeneratedCircle> circles = new List<GeneratedCircle>();
    [SerializeField] private float radius = 1f;
    [SerializeField] private int outerVertices = 25;
    private Mesh mesh;
    
    private void OnEnable() => circles.Add(this);
    private void OnDisable() => circles.Remove(this);

    private void Awake()
    {
        circleMovement = GetComponent<CircleMovement>();
    }

    void Start()
    {
        mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;
        GenerateCircle();
        GetComponent<MeshRenderer>().material.color = Random.ColorHSV();
    }

    void GenerateCircle()
    {
        Vector3[] vertices = new Vector3[outerVertices + 1]; // + center
        int[] triangles = new int[outerVertices * 3]; // each triangle has three outerVertices

        vertices[0] = Vector3.zero;
        for (int i = 0; i < outerVertices; i++) 
        {
            float angle = (360f / outerVertices) * i;
            float x = Mathf.Sin(angle / 180 * Mathf.PI) * radius;
            float y = Mathf.Cos(angle / 180 * Mathf.PI) * radius;
            vertices[i + 1] = new Vector3(x, y, 0); // skip first
        }

        for (int i = 0; i < outerVertices; i++)
        {
            triangles[i*3 + 0] = 0;
            triangles[i*3 + 1] = i+1;
            triangles[i*3 + 2] = i + 2 <= outerVertices ? (i + 2) : 1;
        }

        mesh.Clear(); // clear garbage 
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals(); // Żeby światło wiedziało jak padać
    }

    public float Radius => radius;
}

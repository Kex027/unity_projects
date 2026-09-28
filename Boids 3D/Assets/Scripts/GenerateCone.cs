using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class GenerateCone : MonoBehaviour
{
    private Mesh mesh;
    public ConeMovement coneMovement;
    public static List<GenerateCone> cones = new List<GenerateCone>();
    
    private static readonly int ColorProperty = Shader.PropertyToID("_BaseColor");
    private static MaterialPropertyBlock _block;
    
    private void OnEnable() => cones.Add(this);
    private void OnDisable() => cones.Remove(this);
    
    private void Awake()
    {
        coneMovement = GetComponent<ConeMovement>();
    }
    
    void Start()
    {
        if (_block == null) _block = new MaterialPropertyBlock();
        
        GetComponent<MeshFilter>().sharedMesh = SimulationGenerator.Instance.SharedConeMesh;
        
        // 0.0 – 0.1: Czerwienie i pomarańcze
        // 0.1 – 0.2: Żółcie
        // 0.3 – 0.4: Zielenie
        // 0.5 – 0.7: Błękity i cyjan
        // 0.8 – 0.9: Fiolety i róż
        Color randomColor = Random.ColorHSV(0.6f, 0.9f, 0.8f, 1f, 0.8f, 1f);
        _block.SetColor(ColorProperty, randomColor);
        GetComponent<MeshRenderer>().SetPropertyBlock(_block);
    }
}

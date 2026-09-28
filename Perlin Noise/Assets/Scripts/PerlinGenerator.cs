using UnityEngine;

public class PerlinGenerator : MonoBehaviour
{
    public int width = 256;
    public int height = 256;

    private Texture2D _noiseTexture;
    private PerlinNoise _perlin;
    private MeshRenderer _meshRenderer;

    [Header("Noise Settings")]
    [Range(0.1f, 10f)] public float morphSpeed = 1.0f;
    [Range(0.1f, 20f)] public float scale = 5.0f;
    [Range(0f, 10f)] public float scrollSpeed = 0.5f;
    private float timeAccumulator;

    void Start()
    {
        _perlin = new PerlinNoise(width, height);
        _meshRenderer = GetComponent<MeshRenderer>();
        _noiseTexture = new Texture2D(width, height);
        
        _meshRenderer.material.mainTexture = _noiseTexture;
        _noiseTexture.filterMode = FilterMode.Point;
        _noiseTexture.wrapMode = TextureWrapMode.Clamp;

        GenerateTexture();
    }

    void Update()
    {
        timeAccumulator += Time.deltaTime * morphSpeed;
        _perlin.UpdateGradients(timeAccumulator);
        GenerateTexture();
    }

    void GenerateTexture()
    {
        Color[] pixels = new Color[width * height];

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                float sampleX = (x / (float)width) * scale + (Time.time * scrollSpeed);
                float sampleY = (y / (float)height) * scale;

                float noiseValue = _perlin.GetNoise(sampleX, sampleY);

                float normalizedValue = (noiseValue + 1f) * 0.5f;

                // pixels[y * width + x] = normalizedValue > 0.5f ? Color.saddleBrown : Color.sandyBrown;
                pixels[y * width + x] = new Color(normalizedValue, normalizedValue, normalizedValue);
            }
        }

        _noiseTexture.SetPixels(pixels);
        _noiseTexture.Apply();
    }
}

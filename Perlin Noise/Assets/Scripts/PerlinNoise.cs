using UnityEngine;

public class PerlinNoise
{
    private float[,] angles;
    private Vector2[,] grid;
    private int width;
    private int height;

    public PerlinNoise(int width, int height)
    {
        this.width = width;
        this.height = height;
        grid = new Vector2[width, height];
        angles = new float[width, height];

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                float angle = Random.Range(0f, Mathf.PI * 2);
                angles[x, y] = angle;
                grid[x, y] = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
            }
        }
    }
    
    public void UpdateGradients(float time)
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                float offset = (x + y) * 0.5f; 
                float currentAngle = angles[x, y] + Mathf.Sin(time + offset);
                grid[x, y] = new Vector2(Mathf.Cos(currentAngle), Mathf.Sin(currentAngle));
            }
        }
    }
    
    public float GetNoise(float x, float y)
    {
        int xFloor = Mathf.FloorToInt(x);
        int yFloor = Mathf.FloorToInt(y);
        Vector2Int leftTopIndex = new Vector2Int(xFloor, yFloor + 1);
        Vector2Int rightTopIndex = new Vector2Int(xFloor + 1, yFloor + 1);
        Vector2Int leftBottomIndex = new Vector2Int(xFloor, yFloor);
        Vector2Int rightBottomIndex = new Vector2Int(xFloor + 1, yFloor);

        float dx = x - xFloor;
        float dy = y - yFloor;

        Vector2 distTR = new Vector2(dx - 1f, dy - 1f);
        float dotTR = Vector2.Dot(grid[rightTopIndex.x, rightTopIndex.y], distTR);

        Vector2 distTL = new Vector2(dx, dy - 1f);
        float dotTL = Vector2.Dot(grid[leftTopIndex.x, leftTopIndex.y], distTL);
        
        Vector2 distBL = new Vector2(dx, dy);
        float dotBL = Vector2.Dot(grid[leftBottomIndex.x, leftBottomIndex.y], distBL);
            
        Vector2 distBR = new Vector2(dx - 1f, dy);
        float dotBR = Vector2.Dot(grid[rightBottomIndex.x, rightBottomIndex.y], distBR);
        
        float u = Fade(dx);
        float v = Fade(dy);

        float topInterpolation = Mathf.Lerp(dotTL, dotTR, u);
        float bottomInterpolation = Mathf.Lerp(dotBL, dotBR, u);

        float finalResult = Mathf.Lerp(bottomInterpolation, topInterpolation, v);
        
        return finalResult;
    }
    
    float Fade(float t)
    {
        // Perlin's fade
        // 6t^5 - 15t^4 + 10t^3
        return t * t * t * (t * (t * 6 - 15) + 10);
    }
}
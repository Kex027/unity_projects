using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Tilemaps;

public class WfcGenerator : MonoBehaviour
{
    private Cell[,] grid;
    [SerializeField] private int width = 15;
    [SerializeField] private int height = 15;
    public Tilemap tilemap;
    public List<TileConfig> allTiles;
    [SerializeField] private int cellsPerClick = 10;
    
    void Start()
    {
        grid = new Cell[width, height];
        
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                grid[x, y] = new Cell(allTiles, new Vector2Int(x, y));
            }
        }
    }
    
    void Update()
    {
        if (Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            for (int i = 0; i < cellsPerClick; i++)
            {
                Cell nextCell = FindLowestEntropyCell();
                if (nextCell != null)
                {
                    CollapseCell(nextCell);
            
                    Vector2Int pos = nextCell.coords;
                    Propagate(pos.x, pos.y);
            
                    DrawMap();
                }
            }
        }

        if (Keyboard.current.mKey.wasPressedThisFrame)
        {
            GenerateMap();
        }
        
        if (Keyboard.current.rKey.wasPressedThisFrame)
        {
            ResetGrid();
        }
    }
    
    public void GenerateMap()
    {
        int safetyIterator = 0; 
        bool success = false;

        while (!success && safetyIterator < 50) 
        {
            safetyIterator++;
            ResetGrid(); 
        
            if (RunWfc())
            {
                success = true;
                Debug.Log($"Mapa wygenerowana pomyślnie za {safetyIterator} razem!");
            }
            else
            {
                Debug.LogWarning($"Próba {safetyIterator} nieudana (sprzeczność). Próbuję ponownie...");
            }
        }
    
        DrawMap();
    }

    private bool RunWfc()
    {
        while (true)
        {
            Cell nextCell = FindLowestEntropyCell();
        
            if (nextCell == null) return true; 

            if (nextCell.possibleTiles.Count == 0) return false;

            CollapseCell(nextCell);
            Propagate(nextCell.coords.x, nextCell.coords.y);
        }
    }

    private void CollapseCell(Cell nextCell)
    {
        if (nextCell.possibleTiles.Count == 0)
        {
            Debug.LogError("Sprzeczność! Komórka nie ma pasujących opcji.");
            ResetGrid();
            return;
        }

        int randomIndex = Random.Range(0, nextCell.possibleTiles.Count);
        TileConfig chosenTile = nextCell.possibleTiles[randomIndex];

        nextCell.possibleTiles.Clear();
        nextCell.possibleTiles.Add(chosenTile);
    
        nextCell.collapsed = true;
    }

    private Cell FindLowestEntropyCell()
    {
        Cell bestCell = null;
        int lowestEntropy = int.MaxValue;

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Cell cell = grid[x, y];
            
                if (cell.collapsed) continue;

                int entropy = cell.GetEntropy;

                if (entropy < lowestEntropy)
                {
                    lowestEntropy = entropy;
                    bestCell = cell;
                }
            }
        }

        return bestCell;
    }
    void Propagate(int x, int y)
    {
        Stack<Vector2Int> stack = new Stack<Vector2Int>();
        stack.Push(new Vector2Int(x, y));

        while (stack.Count > 0)
        {
            Vector2Int currentPos = stack.Pop();

            CheckNeighbor(currentPos, Vector2Int.up, stack);
            CheckNeighbor(currentPos, Vector2Int.down, stack);
            CheckNeighbor(currentPos, Vector2Int.left, stack);
            CheckNeighbor(currentPos, Vector2Int.right, stack);
        }
    }

    void CheckNeighbor(Vector2Int current, Vector2Int direction, Stack<Vector2Int> stack)
    {
        int nx = current.x + direction.x;
        int ny = current.y + direction.y;

        if (nx >= 0 && nx < width && ny >= 0 && ny < height)
        {
            Cell neighbor = grid[nx, ny];
            if (neighbor.collapsed) return;

            int startingCount = neighbor.possibleTiles.Count;
        
            neighbor.possibleTiles.RemoveAll(tileInNeighbor => !IsPossible(grid[current.x, current.y], tileInNeighbor, direction));

            if (neighbor.possibleTiles.Count < startingCount)
            {
                stack.Push(new Vector2Int(nx, ny));
            }
        }
    }
    
    private void DrawMap()
    {
        tilemap.ClearAllTiles(); 

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Cell cell = grid[x, y];

                if (cell.collapsed && cell.possibleTiles.Count > 0)
                {
                    TileBase visual = cell.possibleTiles[0].visual;
                    
                    tilemap.SetTile(new Vector3Int(x, y, 0), visual);
                }
            }
        }
    }
    
    public void ResetGrid()
    {
        tilemap.ClearAllTiles();

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                grid[x, y].collapsed = false;
            
                grid[x, y].possibleTiles = new List<TileConfig>(allTiles);
            }
        }
    }
    
    bool IsPossible(Cell currentCell, TileConfig neighborTile, Vector2Int direction)
    {
        foreach (var currentTile in currentCell.possibleTiles)
        {
            if (direction == Vector2Int.up && currentTile.socketTop == neighborTile.socketBottom) return true;
            if (direction == Vector2Int.down && currentTile.socketBottom == neighborTile.socketTop) return true;
            if (direction == Vector2Int.left && currentTile.socketLeft == neighborTile.socketRight) return true;
            if (direction == Vector2Int.right && currentTile.socketRight == neighborTile.socketLeft) return true;
        }
        return false;
    }
}

[System.Serializable]
public class Cell
{
    public bool collapsed;
    public List<TileConfig> possibleTiles;
    public Vector2Int coords; 
    
    public int GetEntropy => possibleTiles.Count;

    public Cell(List<TileConfig> tiles, Vector2Int position)
    {
        possibleTiles = new List<TileConfig>(tiles);
        coords = position;
    }
}
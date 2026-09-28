using UnityEngine;

public class Point
{ 
    public Vector2 position;
    public Vector2 oldPosition;
    public bool isPinned;
    
    public Point(Vector2 pos) { position = pos; oldPosition = pos; isPinned = false; }
}
using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu(fileName = "New Tile Config", menuName = "WFC/Tile Config")]
public class TileConfig : ScriptableObject
{
    public TileBase visual;
    public int socketTop, socketBottom, socketRight, socketLeft;
}
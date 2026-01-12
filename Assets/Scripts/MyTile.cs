using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu]
public class MyTile : Tile
{
    public TileType tileType;
}

public enum TileType
{
    Normal,
    Ice,
    Lava,
    Glass
}
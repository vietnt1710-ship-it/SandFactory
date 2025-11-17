using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tile : MonoBehaviour
{
    [Serializable]
    public enum CellStatus
    {
        empty,
        active,
        watting,
        wall,
        other

    }
    public CellStatus status;
    public TileType type;

    public int row;
    public int col;

    [HideInInspector] public bool visited = false;

 

}
[Serializable]
public enum TileType
{
    normal,
    hiden,
    frezee,
    pipe,
    key,
    _lock
}

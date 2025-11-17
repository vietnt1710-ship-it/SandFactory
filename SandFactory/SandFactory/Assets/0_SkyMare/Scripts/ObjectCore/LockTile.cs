using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LockTile : ObjectTile
{
    public KeyTile target;
    public int lockID;
    public override void Init()
    {
        jar.ApplyLockWaittingJar(colorID, ColorWithID(), sandAmount, row);
    }
    public void Unlock(Transform key)
    {
        type = TileType.normal;
        jar.UnLock(key);
    }
    public override void Active()
    {
        jar.ActiveLock();
    }
    public override void ClickProcess(Slot item)
    {
        jar.TransJar(item);
    }
}

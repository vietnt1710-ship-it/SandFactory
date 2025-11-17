using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KeyTile : ObjectTile
{
    public LockTile target;
    public int keyID;
    public override void Init()
    {
        jar.ApplyKeyWaittingJar(colorID, ColorWithID(), sandAmount, row);

    }
    public override void Active()
    {
        jar.ActiveNormal();
       
    }
    public override void ClickProcess(Slot item)
    {
        StackItem item1 = new StackItem();
        target.Unlock(jar.key.transform);
        //Action keyDone = jar.KeyStackDone;
        jar.TransJar(item);
    }
}

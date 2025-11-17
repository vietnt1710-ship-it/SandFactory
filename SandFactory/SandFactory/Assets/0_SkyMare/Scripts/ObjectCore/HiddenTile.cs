using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HiddenTile : ObjectTile
{
    public override void Init()
    {
        jar.ApplyHiddenWaittingJar(colorID, ColorWithID(), sandAmount, row);
    }
    public override void Active()
    {
        jar.HiddenStackDone();
        jar.ActiveNormal();
    }
    public override void ClickProcess(Slot item)
    {
        StackItem item1 = new StackItem();
        jar.TransJar(item);
    }
}

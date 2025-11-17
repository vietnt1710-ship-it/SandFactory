using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NormalPipeTile : ObjectTile
{
    public override void Init()
    {
        jar.ApplyNormalWaittingJar(colorID, ColorWithID(), sandAmount, row);
    }
    public override void Active()
    {
        jar.ActivePipe();
    }
    public override void ClickProcess(Slot item)
    {
        jar.TransJar(item);
    }
}

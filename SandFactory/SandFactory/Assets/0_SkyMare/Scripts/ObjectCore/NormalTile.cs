using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NormalTile : ObjectTile
{

    public override void Init()
    {
        jar.ApplyNormalWaittingJar(colorID, ColorWithID() , sandAmount, row);
    }
    public override void Active()
    {
        jar.ActiveNormal();
    }
    public override void ClickProcess(Slot item)
    {
        jar.TransJar(item);
    }
}

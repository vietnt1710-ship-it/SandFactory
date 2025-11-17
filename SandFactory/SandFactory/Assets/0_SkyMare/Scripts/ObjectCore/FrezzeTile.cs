using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Frezze : ObjectTile
{
    public int invativeCount = 3;
    public override void Init()
    {
        jar.ApplyFrezzWaittingJar(colorID, ColorWithID(), sandAmount, row);
    }

    public bool UnFezze()
    {
        invativeCount--;

        if(invativeCount == 0)
        {
            type = TileType.normal;
            jar.UnFrezze();
            return true;
        }
       return false;
    }
  
    public override void Active()
    {
        jar.ActiveFrezze();
    }
    public override void ClickProcess(Slot item)
    {
        StackItem item1 = new StackItem();
        jar.TransJar(item);
    }
}

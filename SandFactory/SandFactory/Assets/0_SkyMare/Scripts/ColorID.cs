using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ColorID", menuName = "GameData/ColorID")]
public class ColorID : ScriptableObject
{
   public List<ColorWithID> colorWithIDs;

    private void OnValidate()
    {
        for (int i = 0; i < colorWithIDs.Count; i++)
        {
            colorWithIDs[i].ChangID(i+1);
        }
    }
}
[Serializable]
public class ColorWithID
{
    public int ID;
    public Color color;

    [Header("HolderColor")]
    public Color liquidColor;
    public Color surfaceColor;
    public Color gradiantColor;

    [Header("CapColor")]
    public Color capColor;
    public Color capOutlineColor;

    public void ChangID(int i)
    {
        this.ID = i;
        Color a = Color.black; a.a = 0;
        if (liquidColor == a) liquidColor = color;
        if(surfaceColor == a) surfaceColor = color;
        if(gradiantColor == a) gradiantColor = color;


        if (capColor == a) capColor = color;
        if(capOutlineColor == a) capOutlineColor = color;
    }
}

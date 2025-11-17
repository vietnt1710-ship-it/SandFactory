using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class ETC_Heart : ETC
{
    public const int maxHearts = 5;

    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.T))
        {
            item.Value = -1;
        }
    }
    public override void ActionStart()
    {
        
    }
    public override void UpdateValue(int value)
    {
        Debug.Log($"On Item Changed {item.id} : {value}");
        int hearts = item.Value;
        txt_Value.text = hearts < maxHearts ? $"{hearts}" : "Full";
    }
}



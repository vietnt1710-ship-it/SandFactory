using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class StackManager : MonoBehaviour
{
    public List<StackItem> items;
    string tag = "StackManager Check";
    bool losed = false;
    private void Start()
    {
        items = GetComponentsInChildren<StackItem>().ToList();
    }

    public void LoseAnim()
    {
        for (int i = 0; i < items.Count; i++)
        {
            int idx = i;
            DOVirtual.DelayedCall(0.2f * idx, () =>
            {
                items[idx].jar.jarAnimation.CloseCap();

            });
        }
    }
    public void Check(string callFrom)
    {
        if (losed) return;
        Debug.Log($"{tag} start check");
        for (int i = 0; i < items.Count; i++)
        {
            if (!items[i].stackReady) return;
        }
        DOVirtual.DelayedCall(0.02f, () =>
        {
            Debug.Log($"{tag} full stack");

            if (LevelManager.I.m_painting.HavePieceFilling()) return;

            Debug.Log($"{tag} no piece filling {callFrom}" );

            LevelManager.I.Lose();
            losed = true;
        });
       
    }
    public void Reset()
    {
        losed = false;   
        for (int i = 0; i < items.Count; i++)
        {
            items[i].Reset();
        }
    }

    public StackItem YoungestStackEmpty()
    {
        foreach (StackItem item in items)
        {
            if (item.isEmpty)
            {
                item.isEmpty = false;
                return item;
            }
        }
        return null;
    }
}

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SlotsManager : MonoBehaviour
{
    public List<Slot> items;

    private void Start()
    {
        items = GetComponentsInChildren<Slot>().ToList();

        LevelManager.I.tube.OnPouringDone += FindMatchingSlotWithLowestLiquid;
    }
    public void FindMatchingSlotWithLowestLiquid(List<int> colorIDs)
    {
        // Sắp xếp items theo progressValue giảm dần (từ lớn nhất xuống nhỏ)
        var sortedItems = items.OrderByDescending(item => item.progressValue).ToList();

        for (int i = 0; i < sortedItems.Count; i++)
        {
            if (sortedItems[i].CheckMatchingLiquid(colorIDs))
            {
                return;
            }
        }
        CheckEnd();
    }

    public Slot YoungestStackEmpty()
    {
        foreach (Slot item in items)
        {
            if (item.isEmpty)
            {
                item.isEmpty = false;
                return item;
            }
        }
        return null;
    }
    bool losing = false;
    public void CheckEnd()
    {
        if (losing) return;

        if (CheckFullSlot())
        {
            losing = true;
            Debug.Log("End Lose");
            LevelManager.I.Lose();
        }
        else
        {
            Debug.Log("End No Lose");
        }
    }
    public bool CheckFullSlot()
    {
        if (LevelManager.I.tube.isPouring) return false;
        for (int i = 0;i < items.Count;i++)
        {
            if (items[i].isEmpty) return false;
        }
        return true;
    }
}

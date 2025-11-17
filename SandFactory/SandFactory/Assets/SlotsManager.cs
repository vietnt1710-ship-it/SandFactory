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
}

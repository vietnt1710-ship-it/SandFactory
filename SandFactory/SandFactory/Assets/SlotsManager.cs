using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SlotsManager : MonoBehaviour
{
    public List<Slot> allSlots;
    public List<Slot> slotUnlocks;

    public void LoseAnim()
    {
        for (int i = 0; i < slotUnlocks.Count; i++)
        {
            int idx = i;
            DOVirtual.DelayedCall(0.2f * idx, () =>
            {
                slotUnlocks[idx].jar.jarAnimation.CloseCap();

            });
        }
    }
    private void Start()
    {
        

        LevelManager.I.tube.OnPouringDone += FindMatchingSlotWithLowestLiquid;
    }

    public void Unlock(int count)
    {
        allSlots = GetComponentsInChildren<Slot>().ToList();

        for (int i = 0; i < allSlots.Count; i++)
        {
            if(i == 0)
            {
                if(count >= 6)
                {
                    allSlots[i].ChangeStatus(false);
                    slotUnlocks.Add(allSlots[i]);
                }
                else
                {
                    allSlots[i].ChangeStatus(true);
                }

            }
            else if(i == 6)
            {
                if (count >= 7)
                {
                    allSlots[i].ChangeStatus(false);
                    slotUnlocks.Add(allSlots[i]);
                }
                else
                {
                    allSlots[i].ChangeStatus(true);
                }
            }
            else
            {
                allSlots[i].ChangeStatus(false);
                slotUnlocks.Add(allSlots[i]);
            }
        }
    }
    public void FindMatchingSlotWithLowestLiquid(List<int> colorIDs)
    {
        // Sắp xếp items theo progressValue giảm dần (từ lớn nhất xuống nhỏ)
        var sortedItems = slotUnlocks.OrderByDescending(item => item.progressValue).ToList();

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
        foreach (Slot item in slotUnlocks)
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
            DOVirtual.DelayedCall(0.5f, () =>
            {
                LevelManager.I.Lose();
            });
         
        }
        else
        {
            Debug.Log("End No Lose");
        }
    }
    public bool CheckFullSlot()
    {
        if (LevelManager.I.tube.isPouring) return false;
        for (int i = 0;i < slotUnlocks.Count;i++)
        {
            if (slotUnlocks[i].isEmpty) return false;
        }
        return true;
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlotProgress : MonoBehaviour
{
    public List<Transform> item;

    public void ChangePocess(int count)
    {
        for (int i = 0; i < item.Count; i++)
        {
            if(i < count)
            {
                item[i].transform.GetChild(0).gameObject.SetActive(true);
            }
            else
            {
                item[i].transform.GetChild(0).gameObject.SetActive(false);
            }
           
        }
    }
}

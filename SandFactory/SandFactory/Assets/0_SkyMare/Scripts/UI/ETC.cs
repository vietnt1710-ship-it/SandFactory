using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using DG.Tweening;
using data;
using UnityEngine.UI;
public abstract class ETC : MonoBehaviour
{
    public TMP_Text txt_Value;
    public Text txt_Value2;
    public ItemID itemID;

    public Item item { get; private set; }

    public void Awake()
    {
        item = GameManger.I.datas.items.GetItemByID(itemID);

        if(txt_Value != null)
        {
            txt_Value.text = item.Value.ToString();
        }
        else if (txt_Value2 != null)
        {
            txt_Value2.text = item.Value.ToString();
        }
     
        item.OnItemChanged += UpdateValue;
        ActionStart();
    }
    public abstract void ActionStart();
    public abstract void UpdateValue(int value);
}


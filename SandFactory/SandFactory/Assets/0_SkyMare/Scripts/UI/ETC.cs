using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using DG.Tweening;
using data;
public abstract class ETC : MonoBehaviour
{
    public TMP_Text txt_Value;

    public ItemID itemID;

    public Item item { get; private set; }

    public void Awake()
    {
        item = GameManger.I.datas.items.GetItemByID(itemID);

        txt_Value.text = item.Value.ToString();
        item.OnItemChanged += UpdateValue;
        ActionStart();
    }
    public abstract void ActionStart();
    public abstract void UpdateValue(int value);
}


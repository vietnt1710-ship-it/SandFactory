using data;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static data.IAPData;

public abstract class ShopItem : MonoBehaviour
{
    [Serializable]
    public struct Item
    {
        public Image ic;
        public TMP_Text value;
    }

    public IAPData datas;
    public Image packIcon;

    public Button button;
    

    public void Awake()
    {
        if (datas.isNoAdsPack && GameData.PayedRemoveAds())
        {
            this.gameObject.SetActive(false);
        }
        else if (datas.isNoAdsPack && !GameData.PayedRemoveAds() && GetComponent<PopUp>() == null)
        {
            GameData.OnPayedRemoveAds += DisableIAP;
        }

    }
    private void Start()
    {

        if (button == null)
            button = GetComponent<Button>();

        button.onClick.AddListener(GetPack);
        LoadPack();
    }
    void DisableIAP()
    {
        this.gameObject.SetActive(false);
    }
    public void UnSubDisableIAP()
    {
        GameData.OnPayedRemoveAds -= DisableIAP;
    }

    public void GetPack()
    {
        List<ItemWithValue> items = datas.items;

        for (int i = 0; i < items.Count; i++)
        {
            data.Item item = GameManger.I.datas.items.GetItemByID(items[i].item);
            item.Value = items[i].value;
        }
        if (datas.isNoAdsPack)
        {
            GameData.SetPayedRemoveAds();
        }
    }
    public abstract void LoadPack();


}

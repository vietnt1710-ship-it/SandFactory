using data;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class PopUpBoster : PopUp
{
    public Item item;

    public Image ic;

    public List<BigIconOfItem> bigIconOfItems;

    public Button btn_Reward;
    public Button btn_Coin;

    public void MiniSub()
    {
        btn_Reward.onClick.AddListener(PlayRewardEvent);
        btn_Coin.onClick.AddListener(BuyRewardEvent);
    }
    public void OpenBoster(Item item)
    {
        this.item = item;
        this.ic.sprite = bigIconOfItems.FirstOrDefault(bc => bc.itemID == item.id).bigIC;
        ic.SetNativeSize();
        base.Show();
    }
    public void PlayRewardEvent()
    {
        GetReward();
    }

    public void BuyRewardEvent()
    {
        var coin = GameManger.I.datas.items.GetItemByID(ItemID.coin);
        if(coin.Value < 1000)
        {
            HapticManager.PlayPreset(HapticManager.Preset.HeavyImpact);
        }
        else
        {
            coin.Value = -1000;
            GetReward();
        }
    }
    public void GetReward()
    {
        item.Value = 3;
    }
}
[Serializable]
public struct BigIconOfItem
{
    public ItemID itemID;
    public Sprite bigIC;
}

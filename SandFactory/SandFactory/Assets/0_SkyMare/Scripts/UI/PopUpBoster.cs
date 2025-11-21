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

    public Text boster_Name;
    public Text boster_Discription;
    public Text boster_Price;
    public int price;

    public List<BigIconOfItem> bigIconOfItems;

    public Button btn_Reward;
    public Button btn_Coin;

    public override void MiniSub()
    {
        btn_Reward.onClick.AddListener(PlayRewardEvent);
        btn_Coin.onClick.AddListener(BuyRewardEvent);
    }
    public void OpenBoster(Item item)
    {
        this.item = item;
        var data = bigIconOfItems.FirstOrDefault(bc => bc.itemID == item.id);
        this.ic.sprite = data.bigIC;
        this.boster_Name.text = data.name;
        this.boster_Discription.text = data.discription;
        this.boster_Price.text = data.price.ToString();
        this.price = data.price;

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
        if(coin.Value < this.price)
        {
            HapticManager.PlayPreset(HapticManager.Preset.HeavyImpact);
        }
        else
        {
            coin.Value = -this.price;
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
    public string name;
    public string discription;
    public int price;
    public Sprite bigIC;
}

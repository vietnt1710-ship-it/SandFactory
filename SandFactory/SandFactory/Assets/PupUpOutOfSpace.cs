using data;
using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using static data.IAPData;

public class PupUpOutOfSpace : PopUp
{
    public ItemWithValue itemGold;
    public Button claim;

    public List<ItemWithValue> boster;
    public List<Image> images;
    public Text goldValue;

    public Button btnX;
    public Button btnBuy;

    public PopUpLose popUpLose;
    public PupUpLoseConfirm pupUpLoseConfirm;

    public override void MiniSub()
    {
       for (int i = 0; i < boster.Count; i++)
        {
            var item = GameManger.I.datas.items.datas.FirstOrDefault(item => item.id == boster[i].item);
            images[i].sprite = item.itemSprite;
            images[i].SetNativeSize();
        }
        goldValue.text = itemGold.value.ToString();

        btnX.onClick.AddListener(ActionX);
        btnBuy.onClick.AddListener(ActionBuy);
    }
    public void ActionBuy()
    {
        var coin = GameManger.I.datas.items.GetItemByID(ItemID.coin);
        if (coin.Value < itemGold.value)
        {
            HapticManager.PlayPreset(HapticManager.Preset.HeavyImpact);
        }
        else
        {
            coin.Value = - itemGold.value;

            base.Close();
            DOVirtual.DelayedCall(0.1f, () =>
            {
                pupUpLoseConfirm.Show();
            });
        }
      
    }
    public void ActionX()
    {
        base.Close();
        DOVirtual.DelayedCall(0.1f, () =>
        {
            popUpLose.Show();
        });
    }
}

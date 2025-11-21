using data;
using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static data.IAPData;
public class PopUpLose : PopUp
{
    //[Header("Button Ingame")]
    //public Button btn_Home;
    //public Button btn_TryAgain;
    public ItemWithValue itemGold;
    public Text goldValue;

    public Button btnX;
    public Button btnBuy;

    public PupUpOutOfSpace popUpOutOfSpace;
    public PupUpLoseConfirm pupUpLoseConfirm;

    public override void MiniSub()
    {
        //UIManager.I.eventManager.Subscribe(EventManager.Event.close_lose_gameplay, base.Close);

        //btn_Home.onClick.AddListener(BackHomeEvent);
        //btn_TryAgain.onClick.AddListener(TryAgainEvent);

        btnX.onClick.AddListener(ActionX);
        btnBuy.onClick.AddListener(ActionBuy);
        goldValue.text = itemGold.value.ToString();
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
            pupUpLoseConfirm.Show();
        });
    }

    public void BackHomeEvent()
    {
        UIManager.I.eventManager.Active(EventManager.Event.close_lose_gameplay);
        DOVirtual.DelayedCall(0.5f, () =>
        {
            UIManager.I.m_transferPanel.Open(() =>
            {
                UIManager.I.eventManager.Active(EventManager.Event.close_gameplay);
                DOVirtual.DelayedCall(0.1f, () =>
                {
                    UIManager.I.m_transferPanel.Close();
                });
            });
        });

    }
    public void TryAgainEvent()
    {
        UIManager.I.eventManager.Active(EventManager.Event.close_lose_gameplay);
        DOVirtual.DelayedCall(0.5f, () =>
        {
            UIManager.I.m_transferPanel.Open(() =>
            {
                UIManager.I.eventManager.Active(EventManager.Event.reset_gameplay);
                DOVirtual.DelayedCall(0.1f, () =>
                {
                    UIManager.I.m_transferPanel.Close();
                });
            });
        });
    }
}

using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using static data.IAPData;

public class PupUpWin : PopUp
{
    public ItemWithValue itemGold;

    public Button btn_Claim;
    public Button btn_X2;

    public PopUp m_popUpWinCoin;

    public override void MiniSub()
    {
        UIManager.I.eventManager.Subscribe(EventManager.Event.close_win_gameplay, base.Close);
        btn_Claim.onClick.AddListener(Get);
    }

    private void OnEnable()
    {
        DOVirtual.DelayedCall(5, () =>
        {
            base.Close();
            DOVirtual.DelayedCall(0.1f, () =>
            {
                m_popUpWinCoin.Show();
            });
           
        });
    }


    public void Get()
    {
        ClaimReward(false);
    }
    public void X2()
    {
        ClaimReward(true);
    }
    public void ClaimReward(bool x2 = false)
    {
        var item = GameManger.I.datas.items.datas.FirstOrDefault(i => i.id == itemGold.item);
        item.Value = x2 ? itemGold.value * 2 : itemGold.value;

        DOVirtual.DelayedCall(1, () =>
        {
            BackHomeEvent();
        });
    }
    public void BackHomeEvent()
    {
        UIManager.I.eventManager.Active(EventManager.Event.close_win_gameplay);
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
    //public Item
}

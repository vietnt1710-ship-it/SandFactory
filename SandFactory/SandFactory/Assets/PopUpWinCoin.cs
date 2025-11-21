using DG.Tweening;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static data.IAPData;

public class PopUpWinCoin : PopUp
{
    public ItemWithValue itemGold;

    public Button btn_Claim;

    public override void MiniSub()
    {
        Debug.Log("MiniSub PopUpWinCoin");
        btn_Claim.onClick.AddListener(Get);
    }
    public void Get()
    {
        ClaimReward();
    }

    public void ClaimReward()
    {
        var item = GameManger.I.datas.items.datas.FirstOrDefault(i => i.id == itemGold.item);
        item.Value =itemGold.value;

        DOVirtual.DelayedCall(1, () =>
        {
            Close();
            DOVirtual.DelayedCall(tweenDuration, () =>
            {
                SceneManager.LoadScene(0);
            });
        });
    }

    public void BackHomeEvent()
    {
        //UIManager.I.eventManager.Active(EventManager.Event.close_win_gameplay);
        //DOVirtual.DelayedCall(0.5f, () =>
        //{
        //    UIManager.I.m_transferPanel.Open(() =>
        //    {
        //        UIManager.I.eventManager.Active(EventManager.Event.close_gameplay);
        //        DOVirtual.DelayedCall(0.1f, () =>
        //        {
        //            UIManager.I.m_transferPanel.Close();
        //        });
        //    });
        //});

    }
}

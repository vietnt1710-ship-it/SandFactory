using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class PopUpLose : PopUp
{
    [Header("Button Ingame")]
    public Button btn_Home;
    public Button btn_TryAgain;

    public void MiniSub()
    {
        UIManager.I.eventManager.Subscribe(EventManager.Event.close_lose_gameplay, base.Close);

        btn_Home.onClick.AddListener(BackHomeEvent);
        btn_TryAgain.onClick.AddListener(TryAgainEvent);
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

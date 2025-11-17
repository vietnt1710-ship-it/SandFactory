using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PopUpSetting : PopUp
{
    public EventManager.Event event_OpenGame;
    public EventManager.Event event_CloseGame;

    public GameObject homeButtonGroup;
    public GameObject ingameButtonGroup;

    [Header("Button Ingame")]
    public Button btn_Home;
    public Button btn_Replay;

    public void MiniSub()
    {
        UIManager.I.eventManager.Subscribe(event_OpenGame, OpenIngameButton);
        UIManager.I.eventManager.Subscribe(event_CloseGame, OpenHomeButton);
        UIManager.I.eventManager.Subscribe(EventManager.Event.close_setting, base.Close);

        btn_Home.onClick.AddListener(BackHomeEvent);
        btn_Replay.onClick.AddListener (ReplayEvent);
    }

    private void OpenHomeButton()
    {
        homeButtonGroup.gameObject.SetActive(true);
        ingameButtonGroup.gameObject.SetActive(false);
    }
    private void OpenIngameButton()
    {
        homeButtonGroup.gameObject.SetActive(false);
        ingameButtonGroup.gameObject.SetActive(true);
    }

    public void BackHomeEvent()
    {
        UIManager.I.eventManager.Active(EventManager.Event.close_setting);
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
    public void ReplayEvent()
    {
        UIManager.I.eventManager.Active(EventManager.Event.close_setting);
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

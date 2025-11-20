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
    public GridLayoutGroup grid;
    public override void Show()
    {
        if (bg != null)
        {
            bg.gameObject.SetActive(true);
        }

        this.gameObject.SetActive(true);

        TweenSpacing(-100, 50);
    }
    public override void Close()
    {
        TweenSpacing(50, -100);
        DOVirtual.DelayedCall(tweenDuration, () =>
        {
            if (bg != null)
            {
                bg.gameObject.SetActive(false);
            }

            this.gameObject.SetActive(false);
        });
    }
    void TweenSpacing(float from, float to)
    {
        Vector2 spacing = grid.spacing;
        spacing.y = from;
        grid.spacing = spacing;

        DOTween.To(
            () => grid.spacing,
            v => grid.spacing = v,
            new Vector2(grid.spacing.x, to),
           tweenDuration
        ).SetEase(Ease.OutQuad);
    }
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

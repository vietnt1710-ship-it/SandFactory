using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HomePanel : MainPanel
{
    [Header("RemoveAds")]
    public Button btn_OpenRemoveAds;

    [Header("Play")]
    public EventManager.Event openGame = EventManager.Event.open_gameplay;
    public Button btn_Play;

    public void Start()
    {
        RemoveAdsRequestEvent();
        PlayRequestEvent();
    }

    public void RemoveAdsRequestEvent()
    {
        if (GameData.PayedRemoveAds())
        {
            btn_OpenRemoveAds.gameObject.SetActive(false);
        }
        else
        {
            GameData.OnPayedRemoveAds += () =>
            {
                btn_OpenRemoveAds.gameObject.SetActive(false);
                btn_OpenRemoveAds.transform.DOScale(0, 0.5f).From(1).SetEase(Ease.InBack);
            };
        }
    }
    public void PlayRequestEvent()
    {
        btn_Play.onClick.AddListener(PlayEvent);
    }
    public void PlayEvent()
    {
        UIManager.I.m_transferPanel.Open(() =>
        {
            UIManager.I.eventManager.Active(openGame);
            UIManager.I.m_transferPanel.Close();
        });
    }
}

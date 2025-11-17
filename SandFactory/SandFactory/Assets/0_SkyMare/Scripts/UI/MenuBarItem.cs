using DG.Tweening;
using System;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.GraphicsBuffer;

public class MenuBarItem : MonoBehaviour
{
    [Serializable]
    public enum MenuType
    {
        other,
        shop,
        main,
    }

    [Serializable]
    public enum ItemStatus
    {
        _on,
        _off,
        _lock,
    }
    public MainPanel mainPanel;

    MenuBar menuBar;
    Button btn_Main;
    public MenuType type;
    public ItemStatus status;

    public Image bg_On;
    public Image ic_On;
    public Image ic_Off;

    public RectTransform small;
    public RectTransform big;

    [HideInInspector] public RectTransform rect;


    public void Start()
    {
        rect = GetComponent<RectTransform>();
        menuBar = GetComponentInParent<MenuBar>();
        switch (status)
        {
            case ItemStatus._on:
                TurnOn(0);
                menuBar.currentItem = this;
                break;
            case ItemStatus._off:
                TurnOff(0);
                break;
        }

        btn_Main = GetComponent<Button>();
        btn_Main.onClick.AddListener(() =>
        {
            if(status  == ItemStatus._off)
            {
                ChangeStatus(ItemStatus._on);
            }
            else if (status == ItemStatus._on)
            {
                transform.DOScale(1.05f, 0.05f).From(1).SetLoops(2, LoopType.Yoyo);
            }
        });
    }

    public void ChangeStatus(ItemStatus status)
    {
        if (status == this.status) return;

        this.status = status;
        switch (status) 
        {
            case ItemStatus._on:
                MainPanel.Dir dir = rect.anchoredPosition.x > menuBar.currentItem.rect.anchoredPosition.x? MainPanel.Dir.right: MainPanel.Dir.left;
                TurnOn(0.1f, dir);
                menuBar.currentItem.ChangeStatus(ItemStatus._off);
                menuBar.currentItem = this;
                break;
            case ItemStatus._off:
                TurnOff(0);
                break;
        }
    }

    void TurnOff(float duration)
    {
        mainPanel.Close();
        Sequence s = DOTween.Sequence();
        s.Join(bg_On.rectTransform.DOScale(0.9f, duration/2).From(1).SetEase(Ease.OutQuad));
        s.Join(ic_On.rectTransform.DOSizeDelta(small.sizeDelta, duration).From(big.localScale).SetEase(Ease.OutQuad));
        s.Join(ic_Off.rectTransform.DOSizeDelta(small.sizeDelta, duration).From(big.localScale).SetEase(Ease.OutQuad));
        s.Join(ic_On.rectTransform.DOAnchorPos(Vector2.zero, duration).From(big.anchoredPosition).SetEase(Ease.OutQuad));
        s.Join(ic_Off.rectTransform.DOAnchorPos(Vector2.zero, duration).From(big.anchoredPosition).SetEase(Ease.OutQuad));

        s.Join(bg_On.DOFade(0, duration).From(1).SetEase(Ease.OutQuad));
        s.Join(ic_On.DOFade(0, duration).From(1).SetEase(Ease.OutQuad));
        s.Join(ic_Off.DOFade(1, duration).From(0).SetEase(Ease.OutQuad));
    }
    void TurnOn(float duration, MainPanel.Dir dir = MainPanel.Dir.center)
    {
        mainPanel.Open(dir);
        Sequence s = DOTween.Sequence();
        s.Join(bg_On.rectTransform.DOScale(1, duration/2).From(0.9f).SetEase(Ease.OutQuad));
        s.Join(ic_On.rectTransform.DOSizeDelta(big.sizeDelta, duration).From(small.sizeDelta).SetEase(Ease.OutQuad));
        s.Join(ic_Off.rectTransform.DOSizeDelta(big.sizeDelta, duration).From(small.sizeDelta).SetEase(Ease.OutQuad));
        s.Join(ic_On.rectTransform.DOAnchorPos(big.anchoredPosition, duration).From(Vector2.zero).SetEase(Ease.OutQuad));
        s.Join(ic_Off.rectTransform.DOAnchorPos(big.anchoredPosition, duration).From(Vector2.zero).SetEase(Ease.OutQuad));

        s.Join(bg_On.DOFade(1, duration).From(0).SetEase(Ease.OutQuad));
        s.Join(ic_On.DOFade(1, duration).From(0).SetEase(Ease.OutQuad));
        s.Join(ic_Off.DOFade(0, duration).From(1).SetEase(Ease.OutQuad));
    }

}


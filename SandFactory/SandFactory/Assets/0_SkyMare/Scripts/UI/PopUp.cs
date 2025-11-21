using DG.Tweening;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(CanvasGroup))]
public class PopUp : MonoBehaviour
{
    public bool enableCloseByBg = true;
    public bool closeOtherPopUp = false;
    public EventManager.Event _event;
    public List<Button> closes;
    public void SubEvent()
    {
        BGTrigger();

        if (_event != EventManager.Event.nal)
        {
            UIManager.I.eventManager.Subscribe(_event, Show);
        }
        for(int i = 0; i < closes.Count; i++)
        {
            closes[i].onClick.AddListener(Close);
        }

        // Hủy call back của ShopItem nếu có để ShopItem không tự động tắt
        var si = GetComponent<ShopItem>();
        if (si != null && si.datas.isNoAdsPack )
        {
            //si.UnSubDisableIAP();
            GameData.OnPayedRemoveAds += Close;
        }
        MiniSub();
        //if (this is PopUpSetting st) st.MiniSub();
        //if (this is PopUpLose ls) ls.MiniSub();
        //if (this is PupUpWin wi) wi.MiniSub();
        //if (this is PopUpBoster bs) bs.MiniSub();
        //if (this is PopUpCoin co) co.MiniSub();
        //if (this is PopUpWinCoin cow) cow.MiniSub();
    }
    public virtual void MiniSub() { 
    }
    public void BGTrigger()
    {
        if (!enableCloseByBg) return;
        if (bg == null) return;
        // Thêm EventTrigger cho bg
        EventTrigger trigger = bg.gameObject.AddComponent<EventTrigger>();
        EventTrigger.Entry entry = new EventTrigger.Entry();
        entry.eventID = EventTriggerType.PointerClick;
        entry.callback.AddListener((data) => { Close(); });
        trigger.triggers.Add(entry);
    }
    public Image bg;
    public CanvasGroup cg => GetComponent<CanvasGroup>();
    public const float tweenDuration = 0.25f;
    public Sequence mainSeq;
    public virtual void Show()
    {
        if(bg!= null)
        {
            bg.gameObject.SetActive(true);
        }
      
        this.gameObject.SetActive(true);


        mainSeq.Kill();
        mainSeq = DOTween.Sequence();
        mainSeq.Append(transform.DOScale(1, tweenDuration).From(0.8f).SetEase(Ease.OutBack,2));
        mainSeq.Join(cg.DOFade(1, tweenDuration).From(0));


        if (!closeOtherPopUp) return;
        for (int i = 0; i < PopUpManger.I.popups.Count; i++)
        {
            if(PopUpManger.I.popups[i] != this && PopUpManger.I.popups[i].gameObject.activeSelf)
                PopUpManger.I.popups[i].Close();
        }
    }
    public virtual void Close()
    {
        mainSeq.Kill();
        mainSeq = DOTween.Sequence();
        mainSeq.Append(transform.DOScale(0.8f, tweenDuration).From(1).SetEase(Ease.InBack,2));
        mainSeq.Join(cg.DOFade(0, tweenDuration).From(1));
        mainSeq.OnComplete(() =>
        {
            if (bg != null)
            {
                 bg.gameObject.SetActive(false);
            }
          
            this.gameObject.SetActive(false);
        });
    }
}

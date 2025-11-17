using System;
using System.Collections.Generic;
using UnityEngine;

public class UIManager : Singleton<UIManager>
{
    public MenuBar m_bar;
    public HomePanel m_home;
    public HelperPanel m_helperPanel;
    public ShopPanel m_shop;
    public OtherPanel m_other;
    public TopPanel m_top;
    public GameObject backGround;

    public EventManager eventManager;

    public TransferPanel m_transferPanel;
    protected override void Awake()
    {
        base.Awake();
        eventManager = new EventManager();
    }
    private void Start()
    {
        eventManager.Subscribe(EventManager.Event.open_gameplay, CloseHome);
        eventManager.Subscribe(EventManager.Event.close_gameplay, OpenHome);

    }
    public void OpenHome()
    {
        m_bar.gameObject.SetActive(true);
        m_home.mainPanel.gameObject.SetActive(true);
        backGround.gameObject.SetActive(true);
        m_helperPanel.gameObject.SetActive(false);
    }
    public void CloseHome()
    {
        m_helperPanel.gameObject.SetActive(true);
        m_bar.gameObject.SetActive(false);
        m_home.mainPanel.gameObject.SetActive(false);
        backGround.gameObject.SetActive(false);
    }
}


public class EventManager
{
    [Serializable] 
    public enum Event
    {
        nal,
        open_setting,
        open_popupRemoveAds,
        music_changed,
        sound_changed,
        haptic_changed,
        open_gameplay,
        close_gameplay,
        open_heart,
        open_coin,
        reset_gameplay,
        close_setting,
        open_win_gameplay,
        close_win_gameplay,
        open_lose_gameplay,
        close_lose_gameplay,
    }

    private Dictionary<Event, Delegate> events = new Dictionary<Event, Delegate>();

    // ===== Subscribe =====
    public void Subscribe(Event e, Action callback)
    {
        if (events.ContainsKey(e))
            events[e] = Delegate.Combine(events[e], callback);
        else
            events[e] = callback;
    }

    public void Subscribe<T>(Event e, Action<T> callback)
    {
        if (events.ContainsKey(e))
            events[e] = Delegate.Combine(events[e], callback);
        else
            events[e] = callback;
    }

    // ===== Unsubscribe =====
    public void Unsubscribe(Event e, Action callback)
    {
        if (events.ContainsKey(e))
        {
            events[e] = Delegate.Remove(events[e], callback);
            if (events[e] == null) events.Remove(e);
        }
    }

    public void Unsubscribe<T>(Event e, Action<T> callback)
    {
        if (events.ContainsKey(e))
        {
            events[e] = Delegate.Remove(events[e], callback);
            if (events[e] == null) events.Remove(e);
        }
    }

    // ===== Active =====
    public void Active(Event e)
    {
        if (events.ContainsKey(e))
        {
            (events[e] as Action)?.Invoke();
        }
    }

    public void Active<T>(Event e, T param)
    {
        if (events.ContainsKey(e))
        {
            (events[e] as Action<T>)?.Invoke(param);
        }
    }
}

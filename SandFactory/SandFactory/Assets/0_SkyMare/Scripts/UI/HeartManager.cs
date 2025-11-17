using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeartManager : Singleton<HeartManager>
{
    public enum Status
    {
        none,
        limited,
        unlimited,
    }
    public Status _status;
    public ETC_Heart heart;
    public ETC_HeartUnlimited heartUnlimited;

    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.Y))
        {
            heartUnlimited.item.Value = 4;
        }
        if (Input.GetKeyDown(KeyCode.U))
        {
            heartUnlimited.time.AddHours(-3.99f);
        }
    }
    public void Start()
    {
        if (heartUnlimited.time.IsActive())
        {
            ChangeStatus(Status.unlimited);
        }
        else
        {
            ChangeStatus(Status.limited);
        }

        heartUnlimited.time.OnEndUnlimitedTime += () =>
        {
            ChangeStatus(Status.limited);
        };
        heartUnlimited.item.OnItemChanged += (int a) =>
        {
            ChangeStatus(Status.unlimited);
        };

        UIManager.I.eventManager.Subscribe(EventManager.Event.open_gameplay,  ()=> rect.anchoredPosition = ingamePos);
        UIManager.I.eventManager.Subscribe(EventManager.Event.close_gameplay, () => rect.anchoredPosition = homePos);

    }

    public RectTransform rect;
    public Vector2 ingamePos;
    public Vector2 homePos;
   
    public void ChangeStatus(Status s)
    {
        if (_status == s) return;
        _status = s;

        heart.gameObject.SetActive(s == Status.limited);
        heartUnlimited.gameObject.SetActive(s == Status.unlimited);
    }
}

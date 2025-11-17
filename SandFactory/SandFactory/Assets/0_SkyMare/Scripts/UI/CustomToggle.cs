using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CustomToggle : MonoBehaviour
{
    [Serializable]
    public enum Type
    {
        music,
        sound, 
        haptic
    }
    public Type type;
    public EventManager.Event _event;
    Toggle toggle;

    public Image i_on;
    public Image i_off;
    public RectTransform dot;

    float dotStartos;

    const string ToogleSource = "Source_";
    string toogleSource;
    private void Start()
    {
        toogleSource = $"{ToogleSource}{type.ToString().ToUpper()}";
        bool isOn = PlayerPrefs.GetString(toogleSource, "On") == "On";

        dotStartos = dot.anchoredPosition.x;
        toggle = GetComponent<Toggle>();
        Change(isOn, 0);
        toggle.onValueChanged.AddListener(OnChanged);
        toggle.isOn = isOn;
        
    }

    public void Change(bool isOn ,float duration = 0)
    {
        i_on.DOFade(isOn ? 1 : 0, duration/2).SetDelay(duration / 2).From(isOn ? 0 : 1);
        i_off.DOFade(!isOn ? 1 : 0, duration/2).SetDelay(duration / 2).From(!isOn ? 0 : 1);
        dot.DOAnchorPosX(isOn ? -dotStartos : dotStartos, duration);
    }

    public void OnChanged(bool isOn)
    {
        UIManager.I.eventManager.Active<bool>(_event, isOn);
        PlayerPrefs.SetString(toogleSource, isOn ? "On" : "Off");
        Change(isOn, 0.2f);
    }
}

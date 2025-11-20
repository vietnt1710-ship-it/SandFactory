using DG.Tweening;
using System;
using UnityEngine;
using UnityEngine.UI;

public class ItemSetting : MonoBehaviour
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

    public Image i_off;

    const string ToogleSource = "Source_";
    string toogleSource;
    private void Start()
    {
        toogleSource = $"{ToogleSource}{type.ToString().ToUpper()}";
        bool isOn = PlayerPrefs.GetString(toogleSource, "On") == "On";

        toggle = GetComponent<Toggle>();
        Change(isOn, 0);
        toggle.onValueChanged.AddListener(OnChanged);
        toggle.isOn = isOn;

    }

    public void Change(bool isOn, float duration = 0)
    {
        i_off.DOFade(!isOn ? 1 : 0, duration / 2).SetDelay(duration / 2).From(!isOn ? 0 : 1);
    }

    public void OnChanged(bool isOn)
    {
        UIManager.I.eventManager.Active<bool>(_event, isOn);
        PlayerPrefs.SetString(toogleSource, isOn ? "On" : "Off");
        Change(isOn, 0.2f);
    }
}

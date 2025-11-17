using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ButtonEventAuto : MonoBehaviour
{
    public bool AutoRequestEvent = true;
    public EventManager.Event _event;
    public List<EventManager.Event> minievent = new List<EventManager.Event>();

    Button thisBtn;

    private void Start()
    {

        this.thisBtn = GetComponent<Button>();

        thisBtn.onClick.AddListener(() =>
        {
            ButtonManager.I.DisableButtonsForSeconds(PopUp.tweenDuration);
        });
       
        if (!AutoRequestEvent) return;
      

        if(_event != EventManager.Event.nal)
        {
            thisBtn.onClick.AddListener(() =>
            {
                Debug.Log("Auto Button Clicked");
                UIManager.I.eventManager.Active(_event);
            });
        }
        
        for (int i = 0; i < minievent.Count; i++)
        {
            int idx = i;
            if (minievent[idx] != EventManager.Event.nal)
            {
                thisBtn.onClick.AddListener(() =>
                {
                    Debug.Log("Auto Button Clicked");
                    UIManager.I.eventManager.Active(minievent[idx]);
                });
            }
        }
    }
}

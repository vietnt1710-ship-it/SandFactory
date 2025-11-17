using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ButtonLifecycleTracker : MonoBehaviour
{
    public System.Action OnDestroyed;
    public Button main;
    private void Start()
    {
        main= GetComponent<Button>();
        ButtonManager.I.RegisterButton(main);
    }

    void OnDestroy()
    {
        OnDestroyed?.Invoke();
    }
}

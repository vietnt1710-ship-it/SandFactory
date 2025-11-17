using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using TMPro;
using UnityEngine;
    using UnityEngine.UI;

public class TransferPanel : MonoBehaviour
{
    Image panel;
    private void Start()
    {
        panel = GetComponent<Image>();
        Material mat = new(panel.material);
        panel.material = mat;
    }
    public void Open(Action actionDone = null)
    {
        MatTween(true, actionDone);
    }
    public void Close(Action actionDone = null)
    {
        MatTween(false, actionDone);
    }

    public void MatTween(bool open = true, Action actionDone = null, float duration = 0.5f)
    {
        float valueA = open? 1: 0;
        float targetA = open? 0 :1;

        Sequence seq = DOTween.Sequence();
        seq.Append(DOTween.To(() => valueA, x => valueA = x, targetA, duration)
          .OnUpdate(() => panel.material.SetFloat("_Process", valueA))
          .OnComplete(() =>
          {
              DOVirtual.DelayedCall(0.2f, () =>
              {
                  actionDone?.Invoke();
              });
          })
          .SetEase(Ease.InQuad));

    }
}

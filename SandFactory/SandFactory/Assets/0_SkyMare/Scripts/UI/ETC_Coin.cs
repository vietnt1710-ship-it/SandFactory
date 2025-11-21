using DG.Tweening;
using UnityEngine.UI;
using UnityEngine;
using data;

public class ETC_Coin : ETC
{
    public Image ic;
  
    Tween isTweening;
    public override void ActionStart()
    {

    }

    public void Update()
    {
        //if(Input.GetKeyDown(KeyCode.T))
        //{
        //    item.Value = 100;
        //}
    }
    public override void UpdateValue(int value)
    {
        System.Action a = () =>
        {
            float duration = 1f;
            int currentValue = 0;
            if (txt_Value != null)
            {
                currentValue = int.Parse(txt_Value.text);// = currentValue.ToString();
            }
            else if (txt_Value2 != null)
            {
                currentValue = int.Parse(txt_Value2.text);// = currentValue.ToString();
            }
            int targetValue = item.Value;

            isTweening.Kill();
            isTweening = DOTween.To(() => currentValue, x =>
            {
                currentValue = x;
                if (txt_Value != null)
                {
                    txt_Value.text = currentValue.ToString();
                }
                else if (txt_Value2 != null)
                {
                    txt_Value2.text = currentValue.ToString();
                }
            },
            targetValue, duration)
            .SetEase(Ease.InCubic).OnComplete(() =>
            {
                if (txt_Value != null)
                {
                    txt_Value.text = currentValue.ToString();
                }
                else if (txt_Value2 != null)
                {
                    txt_Value2.text = currentValue.ToString();
                }
            });
        };
        if(value > 0)
        {
            PopUpManger.I.m_claimToken.Play(ic.sprite, ic, 10, () =>
            {
                a?.Invoke();
            });
        }
        else
        {
            a?.Invoke();
        }
        

        
    }
}

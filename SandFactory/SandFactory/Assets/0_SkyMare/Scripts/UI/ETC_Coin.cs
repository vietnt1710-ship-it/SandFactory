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
            int currentValue = int.Parse(txt_Value.text);
            int targetValue = item.Value;

            isTweening.Kill();
            isTweening = DOTween.To(() => currentValue, x =>
            {
                currentValue = x;
                txt_Value.text = currentValue.ToString();
            },
            targetValue, duration)
            .SetEase(Ease.InCubic).OnComplete(() =>
            {
                txt_Value.text = targetValue.ToString();
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

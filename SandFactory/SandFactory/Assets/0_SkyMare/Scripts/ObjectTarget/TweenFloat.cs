using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using System;

public static class TweenFloat
{
    /// <summary>
    /// Tween giá trị float trong shader của Material theo thời gian.
    /// </summary>
    /// <param name="material">Material cần tween.</param>
    /// <param name="matValue">Tên property float trong shader (ví dụ "_FillAmount").</param>
    /// <param name="startValue">Giá trị bắt đầu.</param>
    /// <param name="targetValue">Giá trị kết thúc.</param>
    /// <param name="duration">Thời gian tween (giây).</param>
    public static Tween Tween(this Material material, string matValue , float targetValue, float duration , Action onComplete)
    {
        float startValue = material.GetFloat(matValue);
        // Tween giá trị fill
        return DOTween.To(() => startValue, x =>
        {
            startValue = x;
            material.SetFloat(matValue, startValue);
        },
        targetValue, duration)
        .SetEase(Ease.Linear)
        .OnComplete(() => { onComplete?.Invoke(); });
    }
}

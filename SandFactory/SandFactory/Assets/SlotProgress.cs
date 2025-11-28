using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlotProgress : MonoBehaviour
{
    public SpriteRenderer spriteRenderer;
    private Tweener fillTweener;

    public ParticleSystem effectFill;

    private void Awake()
    {
        effectFill.gameObject.SetActive(false);
    }
    int current = 0;
    public void ChangePocess(int count)
    {
        effectFill.gameObject.SetActive(true);
        int increaseCount = count - current;
        current = count;
        float targetFillAmount = (float)current / 3.0f;

        // Lấy giá trị hiện tại
        float currentFillAmount = spriteRenderer.material.GetFloat("_FillAmount");

        // Kill tween cũ nếu đang chạy
        if (fillTweener != null && fillTweener.IsActive())
        {
            fillTweener.Kill();
        }

        // Tween từ giá trị hiện tại đến giá trị mới trong 0.5s
        fillTweener = DOTween.To(
            () => currentFillAmount,                          // Getter: giá trị bắt đầu
            x => spriteRenderer.material.SetFloat("_FillAmount", x),  // Setter: cập nhật giá trị
            targetFillAmount,                                 // Giá trị đích
            0.4f * increaseCount                                             // Thời gian (0.5 giây)
        ).SetEase(Ease.OutCubic).OnComplete(() =>
        {
            effectFill.gameObject.SetActive(false);
        }); // Optional: thêm easing cho mượt mà hơn
    }
}

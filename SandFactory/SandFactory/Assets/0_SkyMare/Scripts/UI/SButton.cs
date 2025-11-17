using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using DG.Tweening;

[RequireComponent(typeof(ButtonEventAuto))]
[RequireComponent(typeof(ButtonLifecycleTracker))]
public class SButton : Button
{
    [Header("Scale Settings")]
    private const float pressedScale = 1.03f;
    private const float normalScale = 1f;
    private const float tweenDuration = 0.1f;
    private const Ease tweenEase = Ease.OutBack;

    private Vector3 originalScale;
    private Tween scaleTween;

    protected override void Awake()
    {
        base.Awake();
        // Lưu scale ban đầu
        originalScale = transform.localScale;
    }
    public override void OnPointerDown(PointerEventData eventData)
    {
        base.OnPointerDown(eventData);

        // Chỉ chạy animation nếu button interactable
        if (!IsInteractable())
            return;

        // Hủy tween cũ nếu đang chạy
        scaleTween?.Kill();

        // Tween scale lên pressedScale
        scaleTween = transform.DOScale(originalScale * pressedScale, tweenDuration)
            .SetEase(tweenEase)
            .SetUpdate(true);
    }

    public override void OnPointerUp(PointerEventData eventData)
    {
        base.OnPointerUp(eventData);

        // Hủy tween cũ nếu đang chạy
        scaleTween?.Kill();

        // Tween scale về normalScale
        scaleTween = transform.DOScale(originalScale * normalScale, tweenDuration)
            .SetEase(tweenEase)
            .SetUpdate(true);
    }

    protected override void OnDisable()
    {
        base.OnDisable();

        // Reset scale khi disable
        scaleTween?.Kill();
        transform.localScale = originalScale;
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();

        // Cleanup tween khi destroy
        scaleTween?.Kill();
    }
}
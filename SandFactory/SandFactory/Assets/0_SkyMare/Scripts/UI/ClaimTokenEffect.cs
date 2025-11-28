using DG.Tweening;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.U2D;
using UnityEngine.UI;
using Random = UnityEngine.Random;

/// <summary>
/// ClaimTokenEffect:
/// - Spawn các token (Image) từ icon nguồn bay về icon đích.
/// - Token bay Ease.InOutBack, cuối đường bay sẽ fade-out.
/// - Icon đích "giật" scale (to rồi nhỏ lại) mỗi lần có token chạm.
/// Yêu cầu: DOTween, Canvas chung cho nguồn & đích (có thể khác parent).
/// </summary>
public class ClaimTokenEffect : MonoBehaviour
{
    [Header("References")]
    public Image sourceImage;
    public Image targetImage;
    private RectTransform tokenLayer;
    private Canvas rootCanvas;

    [Header("Burst (khởi đầu)")]
    public float burstRadius = 80f;


    [Header("Target Pulse")]
    public float targetPulseScale = 1.1f;
    public float targetPulseTime = 0.08f;
    public int targetPulseLoops = 2;


    private Vector2 startPos;
    private Vector2 endPos;

    private void Start()
    {
        rootCanvas = GetComponentInParent<Canvas>();
        tokenLayer = GetComponent<RectTransform>();
    }

    [ContextMenu("Play Effect")]
    public void Play(Sprite sprite, Image target, int count, Action firstAction = null)
    {
        Debug.Log("Play Effect" + gameObject.name);
        if (target != null)
        {
            targetImage = target;
        }
        if (sprite != null)
        {
            sourceImage.sprite = sprite;
            sourceImage.SetNativeSize();
        }


        startPos = GetCenterIn(tokenLayer, sourceImage.rectTransform);
        Debug.Log("Play Effect" + tokenLayer.name);
        Debug.Log("Play Effect" + targetImage.name);
        endPos = GetCenterIn(tokenLayer, targetImage.rectTransform);

        SpawnBurstAndFly(count, firstAction);

    }

    private void SpawnBurstAndFly(int count, Action firstAction = null)
    {
        var delay = 0f;

        for (int i = 0; i < count; i++)
        {
            int idx = i;

            Image token = Instantiate(sourceImage, tokenLayer);
            token.gameObject.SetActive(true);
            RectTransform rt = token.rectTransform;
            Vector2 burstOffset = Random.insideUnitCircle.normalized * Random.Range(burstRadius * 0.5f, burstRadius);
            Vector2 burstPoint = startPos + burstOffset;

            rt.DOAnchorPos(burstPoint, delay + 0.3f).From(startPos).SetEase(Ease.OutBack);

            rt.DOScale(1f, 0.3f).From(0).SetDelay(delay).SetEase(Ease.OutBack);

            rt.DOAnchorPos(endPos, 0.8f).SetDelay(delay + 0.5f).SetEase(Ease.InBack).OnComplete(() =>
            {
                PulseTarget(targetImage.rectTransform);
                if (token) Destroy(token.gameObject);
                if(idx == 0)
                {
                    firstAction?.Invoke();
                }
            });


            rt.DORotate(Vector3.zero, 0.5f).From(new Vector3(0, 0, Random.Range(0, 180))).SetDelay(delay + 0.5f)
                .SetEase(Ease.Flash);


            rt.DOScale(0f, 0.3f).SetDelay(delay + 1.5f).SetEase(Ease.OutBack);

            delay += 0.1f;

        }
    }

    private void PulseTarget(RectTransform targetRt)
    {
        ObjectPoolManager.I.Spawn("CoinExp", targetRt.transform.position);
        targetRt.DOKill(true);
        targetRt.DOScale(targetPulseScale, targetPulseTime)
                .SetEase(Ease.InOutBack)
                .SetLoops(targetPulseLoops, LoopType.Yoyo);
    }



    private Vector2 GetCenterIn(RectTransform targetParent, RectTransform from)
    {
        Vector3 worldCenter = from.TransformPoint(GetRectCenterLocal(from));
        Camera cam = (rootCanvas.renderMode == RenderMode.ScreenSpaceOverlay) ? null : rootCanvas.worldCamera;
        Vector2 screenPt = RectTransformUtility.WorldToScreenPoint(cam, worldCenter);
        RectTransformUtility.ScreenPointToLocalPointInRectangle(targetParent, screenPt, cam, out Vector2 localPt);
        return localPt;
    }

    private Vector3 GetRectCenterLocal(RectTransform rt)
    {
        var rect = rt.rect;
        return new Vector3(rect.x + rect.width * 0.5f, rect.y + rect.height * 0.5f, 0f);
    }
}

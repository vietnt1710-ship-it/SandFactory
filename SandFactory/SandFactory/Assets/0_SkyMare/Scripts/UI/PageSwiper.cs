using DG.Tweening;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;

public class PageSwiper : MonoBehaviour
{
    [Header("References")]
    public ScrollRect scrollRect;
    public RectTransform content;

    [Header("Settings")]
    private int totalPages;
    private GridLayoutGroup gridLayout;
    public float distance;
    public List<Image> pageMaker = new List<Image>();

    public void SetUpPage()
    {
        if (scrollRect == null)
            scrollRect = GetComponent<ScrollRect>();

        gridLayout = content.GetComponent<GridLayoutGroup>();
        totalPages = content.childCount;

        distance = gridLayout.cellSize.x + gridLayout.spacing.x;
        SnapToCenter(0);
    }

    public void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            OnBeginDrag();
        }
        if (Input.GetMouseButtonUp(0))
        {
            OnEndDrag();
        }
    }
    public void OnBeginDrag()
    {
        snap.Kill();
        Debug.Log($"Snap to page on begin");
    }

    public void OnEndDrag()
    {
        int page = GetNearestPageToCenter();
        Debug.Log($"Snap to page {page}");
        SnapToCenter(page);
    }

    int GetNearestPageToCenter()
    {
        float contentPosX = content.anchoredPosition.x;

        float minX = float.MaxValue;
        int page = 0;
        for (int i = 0; i < totalPages; i++)
        {
            float dis = Mathf.Abs(Mathf.Abs(i * this.distance) - Mathf.Abs(contentPosX));
            if (dis < minX)
            {
                page = i;
                minX = dis;
            }
        }

        return page;
    }
    Tween snap;
    public void SnapToCenter(int page)
    {
        if (totalPages <= 0) return;

        for(int i = 0; i < totalPages; i++)
        {
            if(i == page) pageMaker[i].color = Color.white;
            else
            {
                var color = Color.white;
                color.a = 0.5f;
                pageMaker[i].color = color;
            }
        }
        float posX = -distance * page;
        snap = content.DOAnchorPosX(posX, 0.2f);
    }

}
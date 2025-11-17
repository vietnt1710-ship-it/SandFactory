using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class ShopPack : MonoBehaviour
{
    public RectTransform content;
    public Transform title;
    public List<ShopItemPack> item;

    public void LoadPack()
    {
        for (int i = item.Count -1 ; i >= 0; i--)
        {
           var go = Instantiate(item[i]);
            go.transform.SetParent(content, false);
            go.transform.localScale = Vector3.one;
            go.transform.SetSiblingIndex(IndexOfTitle() + 1);
        }
    }

    public int IndexOfTitle()
    {
        for (int i = 0; i < content.transform.childCount; i++)
        {
            if (content.transform.GetChild(i) == title) return i;
        }
        return 0;
    }
}

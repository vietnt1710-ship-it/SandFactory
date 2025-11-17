using data;
using DG.Tweening;
using UnityEngine;

public class ShopCoin : MonoBehaviour
{
    public RectTransform content;
    public Transform title;
    public ShopItemCoin items;
    public void Start()
    {
        float height = 0;
        for(int i = 0; i < content.childCount; i++)
        {
            if (content.GetChild(i).gameObject.activeSelf)
            {
                height += content.GetChild(i).GetComponent<RectTransform>().sizeDelta.y;
            }
            
        }
        Debug.Log($"LoadPack Pos {height}");
        height = height / 2; height = -height;
        content.DOAnchorPosY(height, 0);
    }
    public void LoadPack()
    {
        for (int i = GameManger.I.datas.goldPacks.Count - 1; i >= 0; i--)
        {
            var go = Instantiate(items);
            go.transform.SetParent(content, false);
            go.transform.localScale = Vector3.one;
            go.datas = GameManger.I.datas.goldPacks[i];
            go.transform.SetSiblingIndex(IndexOfTitle() + 1);
        }
    }

    public int IndexOfTitle()
    {
        for(int i = 0; i < content.transform.childCount; i++)
        {
            if(content.transform.GetChild(i)== title) return i;
        }
        return 0;
    }
}

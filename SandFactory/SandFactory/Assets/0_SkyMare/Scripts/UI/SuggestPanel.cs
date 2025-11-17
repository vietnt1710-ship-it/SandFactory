using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SuggestPanel : MonoBehaviour
{
    public List<ShopItemPack> packs;
    public Transform packContent;
    public Image pageMarker;
    public Transform markerContent;
    public PageSwiper swiper;

    private void Start()
    {
        LoadSuggest();
    }
    public void LoadSuggest()
    {
        for (int i = 0; i < packs.Count; i++)
        {
            var go = Instantiate(packs[i]);
            go.transform.SetParent(packContent, false);
            go.transform.localScale = Vector3.one;

            var mk = Instantiate(pageMarker);
            mk.gameObject.SetActive(true);
            mk.transform.SetParent(markerContent, false);
            swiper.pageMaker.Add(mk);
        }
        swiper.SetUpPage();
    }
}

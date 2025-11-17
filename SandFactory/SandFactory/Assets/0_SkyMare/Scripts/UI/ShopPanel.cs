using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShopPanel : MainPanel
{
    public ShopCoin shopCoin;
    public ShopPack shopPack;

    public void Start()
    {
        shopPack.LoadPack();
        shopCoin.LoadPack();
    }
}

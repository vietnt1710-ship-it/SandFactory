using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PopUpCoin : PopUp
{
    public ShopCoin ShopCoin;
  public override void MiniSub()
    {
        ShopCoin.LoadPack();
    }
}

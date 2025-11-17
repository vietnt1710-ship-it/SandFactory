using System;
using UnityEngine;
using data;

public class GameData
{

    private const string Level_Source = "Level";
    public static Action OnLevelChanged;

    public static int Level
    {
        get
        {
            return PlayerPrefs.GetInt(Level_Source, 1);
        }
        set
        {
            int level = PlayerPrefs.GetInt(Level_Source, 1);
            level++;
            PlayerPrefs.SetInt(Level_Source, level);

            OnLevelChanged?.Invoke();
        }
    }
    const string PayedRemoveAdsSource = "Payed_RemoveAds";
    public static Action OnPayedRemoveAds;
    public static void SetPayedRemoveAds()
    {
        PlayerPrefs.SetString(PayedRemoveAdsSource, "Payed");
        OnPayedRemoveAds?.Invoke();
    }
    public static bool PayedRemoveAds()
    {
      return  PlayerPrefs.GetString(PayedRemoveAdsSource) == "Payed";
    }

    //private const string Coin_Source = "Coin";
    //public static Action<int> OnCoinChanged ;
    //public static int Coin
    //{
    //    get
    //    {
    //        return PlayerPrefs.GetInt(Coin_Source, 0);
    //    }
    //    set
    //    {
    //        int coin = PlayerPrefs.GetInt(Coin_Source, 0);
    //        coin += value;
    //        PlayerPrefs.SetInt(Coin_Source, coin);

    //        OnCoinChanged?.Invoke(coin);
    //    }
    //}
    //private const string Heart_Source = "Heart";
    //public static Action<int> OnHeartChanged;
    //public static int Heart
    //{
    //    get
    //    {
    //        if(PlayerPrefs.GetInt(Heart_Source, 0) > intMaxHeart)
    //        {
    //            PlayerPrefs.SetInt(Heart_Source, intMaxHeart);
    //        }
    //        return PlayerPrefs.GetInt(Heart_Source, 0);
    //    }
    //    set
    //    {
    //        int heart = PlayerPrefs.GetInt(Heart_Source, 0);
    //        heart += value;
    //        PlayerPrefs.SetInt(Heart_Source, heart);

    //        OnHeartChanged?.Invoke(heart);
    //    }
    //}

}

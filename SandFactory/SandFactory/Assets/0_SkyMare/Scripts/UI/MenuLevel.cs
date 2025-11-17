using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuLevel : MonoBehaviour
{
    public List<MenuLevelItem> items;

    private void Start()
    {
        LoadLevel();
        GameData.OnLevelChanged += LoadLevel;
    }
    public void LoadLevel()
    {
        var config = GameManger.I.datas;

        int level = GameData.Level;

        for(int i = 0; i < items.Count; i++)
        {
            items[i].Init(level + i, i == 0);
        }
    }
}

using Data;
using System;
using System.Collections;
using System.Collections.Generic;
using ToolLevel;
using UnityEngine;

[CreateAssetMenu(fileName = "pic", menuName = "GameData/PicData")]
public class PicData : ScriptableObject
{
    [Serializable]
    public enum Difficulty
    {
        normal,
        hard,
        nightmare
    }
    public Difficulty difficulty;
    public List<SpriteInfor> currentSprites;
    public LevelData levelData;
}

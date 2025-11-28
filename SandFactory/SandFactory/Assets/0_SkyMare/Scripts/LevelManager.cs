using Data;
using DG.Tweening;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LevelManager : Singleton<LevelManager>
{
    public SandLevelData data;
    public StackManager m_stackManager;
    public Painting m_painting;
    public StackManager m_stack;
    public SlotsManager m_slots;

    public MazeGenerate m_mazeGenerate;

    public Tube tube;

    public List<SandLevelData> datas;
    public const string LevelSource = "Level";

    public Text levelText;

    public TMP_InputField levelField;
    public Button set;
    public Button next;

    public void SetLevel()
    {
        short lvl = 0;
        if (Int16.TryParse(levelField.text, out lvl))
        {
            lvl --;
            if (lvl > datas.Count - 1)
            {
                lvl = 0;
            }
            PlayerPrefs.SetInt(LevelSource, lvl);
            SceneManager.LoadScene(0);
        }
    }
    public void NextLevel()
    {
        Level = 1;
        SceneManager.LoadScene(0);
    }

    public int Level
    {
        get
        {
            return PlayerPrefs.GetInt(LevelSource, 0);
        }
        set
        {
            int level = PlayerPrefs.GetInt(LevelSource, 0);
            level++;
            if(level > datas.Count - 1)
            {
                level = 0;
            }
            PlayerPrefs.SetInt(LevelSource, level);
        }
    }
    public void Update()
    {
        //if (Input.GetKeyDown(KeyCode.O))
        //{
        //    Win();
        //}
        //if (Input.GetKeyDown(KeyCode.P))
        //{
        //    Lose();
        //}
    }
    public void Start()
    {
        next.onClick.AddListener(NextLevel);
        set.onClick.AddListener(SetLevel);
        UIManager.I.eventManager.Subscribe(EventManager.Event.open_gameplay, LoadLevel);
        UIManager.I.eventManager.Subscribe(EventManager.Event.close_gameplay, ClearLevel);
        UIManager.I.eventManager.Subscribe(EventManager.Event.reset_gameplay, Reset);

        LoadLevel();
    }
    public void LoadLevel()
    {
        levelText.text = $"LEVEL {Level + 1}";
        int slotCount = Level < 15 ? 5 : Level < 26 ? 6 : 7;
        m_slots.Unlock(slotCount);

        var data = datas[Level];
        tube.LoadLevelData(data);
        m_mazeGenerate.LoadLevel(data.TxTToGrid());
    }
    public void ClearLevel()
    {
        m_painting.Reset();
        m_mazeGenerate.Reset();
        m_stack.Reset();
    }
    public void Win()
    {
        Level = 1;
        UIManager.I.eventManager.Active(EventManager.Event.open_win_gameplay);
    }
    public void Lose()
    {
        m_slots.LoseAnim();

        DOVirtual.DelayedCall(2, () =>
        {
            UIManager.I.eventManager.Active(EventManager.Event.open_lose_gameplay);
        });
      
    }
    public void Reset()
    {
        ClearLevel();
        DOVirtual.DelayedCall(0.02f, () =>
        {
            LoadLevel();
        });
    }
}

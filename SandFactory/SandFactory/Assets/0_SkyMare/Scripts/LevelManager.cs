using Data;
using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelManager : Singleton<LevelManager>
{
    public SandLevelData data;
    public StackManager m_stackManager;
    public Painting m_painting;
    public StackManager m_stack;
    public SlotsManager m_slots;

    public MazeGenerate m_mazeGenerate;

    public Tube tube;

    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.O))
        {
            Win();
        }
        if (Input.GetKeyDown(KeyCode.P))
        {
            Lose();
        }
    }
    public void Start()
    {
        UIManager.I.eventManager.Subscribe(EventManager.Event.open_gameplay, LoadLevel);
        UIManager.I.eventManager.Subscribe(EventManager.Event.close_gameplay, ClearLevel);
        UIManager.I.eventManager.Subscribe(EventManager.Event.reset_gameplay, Reset);

        LoadLevel();
    }
    public void LoadLevel()
    {
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

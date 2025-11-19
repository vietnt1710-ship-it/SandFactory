using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public abstract class ObjectTile : Tile
{
    public Vector3 jarPos = new Vector3(0, -0.601999998f, -0.0039999485f);
     public SandJar jar;
     public int colorID;
     public int sandAmount;

    public abstract void Init();
    public abstract void Active();

    public abstract void ClickProcess(Slot item);

    private void Start()
    {
        //if(!(this as NormalPipeTile))
        //{
        //    float delayTime = (MazeManager.I.grid.GetLength(0) - row) * 0.1f + (MazeManager.I.grid.GetLength(0) - col) * 0.1f;
        //    jar.transform.DOScale(new Vector3(1,1,1), 0.75f).From(new Vector3(0, 1, 0)).SetDelay(delayTime).SetEase(Ease.OutBounce);
        //}
     
    }

    public void Initizal()
    {
        jar = GetComponentInChildren<SandJar>();
        Init();
    }
    
    public void ActiveTile()
    {
        status = CellStatus.active;
        Active();
        //sandbox.transform.DOScale(_jarStartScale * 1.5f, 0.5f);

    }
    public void OnClick()
    {
        Debug.Log($"Click on {row}, {col} _ isready:{ jar.jarAnimation.isReady}");
        if (status == CellStatus.active && jar.jarAnimation.isReady)
        {
            Debug.Log($"Click on Active {row}, {col}");
            HapticManager.PlayPreset(HapticManager.Preset.HeavyImpact);
            Slot item = LevelManager.I.m_slots.YoungestStackEmpty();
            if (item == null) return;
            status = CellStatus.empty;

            ClickProcess(item);

            MazeManager.I.AfterTileClick();
        }
        else if(status == CellStatus.watting)
        {
            Debug.Log($"Click on Watting {row}, {col}");
            HapticManager.PlayPreset(HapticManager.Preset.LightImpact);
            jar.jarAnimation.StartTilt(0.8f);
        }
       
    }

   
    public ColorWithID ColorWithID()
    {
        return MazeManager.I.colorData.colorWithIDs.FirstOrDefault(c => c.ID == colorID);
    }
}

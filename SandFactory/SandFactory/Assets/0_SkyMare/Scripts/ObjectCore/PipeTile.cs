using System;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class PipeTile : Tile
{
    public List<DataWaitingInPie> dataWaitings = new List<DataWaitingInPie>();  
    Tile target;
    public int targetRow;
    public int targetCol;

    public Transform gara;
    public TMP_Text countInGara;
    public SandJar sandJar;

  
    public void InitPipe(List<(int colorID, int sandAmount)> ims, string be)
    {
        for (int i = 0; i < ims.Count; i++)
        {
            DataWaitingInPie data = new DataWaitingInPie(ims[i].colorID, ims[i].sandAmount);
            dataWaitings.Add(data);
        }
        countInGara.text = dataWaitings.Count.ToString();

        int dirIdx = int.Parse(be[1].ToString());
        targetRow = row + MazeManager.DIRS[dirIdx - 1].dr;
        targetCol = col + MazeManager.DIRS[dirIdx - 1].dc;

        switch(dirIdx)
        {
            case 1: gara.transform.localEulerAngles = new Vector3(90, 0, 0); break;
            case 2: gara.transform.localEulerAngles = new Vector3(90, 0, 180); break;
            case 3: gara.transform.localEulerAngles = new Vector3(90, 0,-90); break;
            case 4: gara.transform.localEulerAngles = new Vector3(90, 0, 90); break;
        }
    }

    public void SpawmJar()
    {
        if (target == null)
        {
            if (MazeManager.I.grid[targetRow, targetCol] is ObjectTile o)
            {
                target = o;
            }
            else
            {
                Debug.LogError("Gara Error");
                return;
            }
        }
  
        if(target.status == CellStatus.empty && dataWaitings.Count > 0)
        {
            var newData = dataWaitings[0];
            dataWaitings.RemoveAt(0);

            //Spawm new object tile
            ObjectTile t = target.AddComponent<NormalPipeTile>();
            MazeManager.I.grid[target.row, target.col] = t;

            t.status = CellStatus.watting;
            t.row = target.row;
            t.col = target.col;
            t.colorID = newData.colorID;
            t.sandAmount = newData.sandAmount;

            var jar = Instantiate(sandJar, transform.position, sandJar.transform.rotation);
            jar.transform.SetParent(t.transform);

            jar.jarAnimation.quad = t.transform.GetChild(0).transform.GetChild(2).gameObject;

            jar.jarAnimation.ScaleToNormal(t.jarPos, 0.5f, () =>
            {
                countInGara.text = dataWaitings.Count.ToString();
                jar.ActivePipe();
            });
            jar.gameObject.SetActive(true);

            jar.ApplyNormalWaittingJar(newData.colorID, t.ColorWithID(), newData.sandAmount, t.row);
            t.jar = jar;
            Destroy(target);
            target = t;
        }
    }

}
[Serializable]
public struct DataWaitingInPie
{
    public int colorID;
    public int sandAmount;

    public DataWaitingInPie (int colorID , int sandAmount)
    {
        this.colorID = colorID;
        this.sandAmount = sandAmount;
    }
}

using DG.Tweening;
using System;
using System.Collections.Generic;
using UnityEngine;

public class Slot : MonoBehaviour
{
    public Transform stopPosition;

    public bool isEmpty;

    public MeshRenderer visualHolder;

    public SlotProgress progress;

    public SandJar jar;
    public bool isReady;

    int ColorID;

    public int progressValue = 0;
    public void StackOn(SandJar sandBox, int colorID, int sandValue, out Action onJumpDone, out Action onJumpComplete)
    {
        isEmpty = false;
        onJumpDone = () =>
        {
            Debug.Log($"Start check Lose");
            sandBox.transform.SetParent(visualHolder.transform);
            float y = visualHolder.transform.position.y;
            visualHolder.transform.DOMoveY(y - 0.075f, 0.1f);

           // Đặt color của base thành color của màu
            var color = sandBox.color.color;
            color.a = 0.5f;
            visualHolder.material.SetColor("_Color", color);
        };

        onJumpComplete = () =>
        {
            isReady = true;
            this.ColorID = colorID;
            jar = sandBox;
            progressValue = 0;

            FirstFindMatchingLiquid();

        };
    }
    public bool CheckMatchingLiquid(List<int> colorIDs)
    {
        if(!isReady) return false;

        if (colorIDs[0] == ColorID)
        {
            Debug.Log($"Matching tube {ColorID}");
            jar.jarAnimation.JumpToPouringPoint(LevelManager.I.tube.pouringPosition, () =>
            {
                Action fillDone = null;
                int getCount = 0;
               if(progressValue + colorIDs.Count > 3)
                {
                    getCount = 3 - progressValue;
                    progressValue = 3;
                    progress.ChangePocess(progressValue);
                }
                else
                {
                    getCount = colorIDs.Count;
                    progressValue += colorIDs.Count;
                    progress.ChangePocess(progressValue);
                }
           
               if(progressValue == 3)
                {
                    isReady = false;
                    fillDone += () =>
                    {
                        isEmpty = true;
                        float y = visualHolder.transform.position.y;
                        visualHolder.transform.DOMoveY(y + 0.075f, 0.1f);
                        visualHolder.material.SetColor("_Color", Color.white);
                        progress.ChangePocess(0);
                    };
                }

                float time = LevelManager.I.tube.duration * getCount;

                jar.jarAnimation.wobble.Fill(time, (float)progressValue / 3.0f);


                LevelManager.I.tube.Pour(getCount);

                DOVirtual.DelayedCall(time, () =>
                {
                    jar.jarAnimation.BackToBase(stopPosition, () =>
                    {
                        LevelManager.I.tube.PouringDone();
                    }, fillDone);
                });
            });
            return true;
        }
        else
        {
            return false;
        }
    }
    public void FirstFindMatchingLiquid()
    {
        Debug.Log($"Start find Matching tube {ColorID}");
        List<int> colorIDs = LevelManager.I.tube.FindColorGroup();
        Debug.Log($"Start find Matching tube group {colorIDs[0]}");
        if (colorIDs != null)
        {
            CheckMatchingLiquid(colorIDs);
        }
    
    }

}

using Data;
using DG.Tweening;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;


public class Tube : MonoBehaviour
{
    public float duration = 0.1f;
    private float startY = 0.2f;

    public float stepY = 0.4f;
    public float stepLiqiudX = 0.4f;

    public Material liquidMaterial;
    public MeshRenderer liquid;
    public ColorID colors;

    private List<int> passengerIndexs = new List<int>() { 9, 4, -4, -9, -4, -4, 6, 4, 6, 4, 6, 4, 9, 5, 9, 5, 5, 9, 9, 9, 4, 4, 10, 10, 5, 3, 3, 10, 3, 10, 6,6, 6, 10, 5,5,5,5,9,5,5,5,5,9,5,5, 5,10 };
    public List<MeshRenderer> liquids = new List<MeshRenderer>();

    public Transform pouringPosition;

    public MeshRenderer waterDrop;

    public SegmentedColoredPipe segmentedColoredPipe;
    public void Start()
    {
        pourFx.Stop();
    }
    public TMP_Text debugText;
    public void LoadLevelData(SandLevelData data)
    {
        passengerIndexs = new List<int>(data.tubes);
        debugText.text = "Tube log o cho nay nay " + data.tubes.Count + "   new " + passengerIndexs.Count;
        GeneratePassenger();
    }

    Tween waterTween;
    public ParticleSystem pourFx;

    public void StartPourFx(Color color, float duration)
    {
        var renderer = pourFx.GetComponent<ParticleSystemRenderer>();

        renderer.material.color = color;
        var main = pourFx.main;

        main.duration = duration;

        pourFx.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        pourFx.Play();
    }
    public void FillWater(float target, float duration, float delay = 0f)
    {
        // Kill tween cũ nếu đang chạy
        waterTween?.Kill();

        float currentFill = waterDrop.material.GetFloat("_Fill");

        waterTween = DOTween.To(
            () => currentFill,
            x => waterDrop.material.SetFloat("_Fill", x),
            target,
            duration
        )
        .SetEase(Ease.Linear)
        .SetDelay(delay)
        .OnComplete(() => Debug.Log("Fill completed!"));
    }

    public void StartDropWater(int count)
    {
        int index = passengerIndexs[0] >= 0 ? passengerIndexs[0] - 1 : passengerIndexs[0] + 1;

        float time = count == 1 ? 0.25f : count == 2 ? 0.6f : 1.1f;//(duration * count) * 0.6f;
        StartPourFx(colors.colorWithIDs[Mathf.Abs(index)].surfaceColor, time);
        waterDrop.material.color = colors.colorWithIDs[Mathf.Abs(index)].color;
        waterDrop.material.SetFloat("_Fill", 0);
        FillWater(0.5f, 0.16f * count);

        Debug.Log("DropWaterTime " + (duration * count) * 0.5f);

        DOVirtual.DelayedCall((duration * count) * 0.5f, () =>
        {
            FillWater(1, 0.5f);
        });
    }
    public void Pour(int count)
    {
        StartDropWater(count);

      

        float duration = this.duration * 0.7f;
        Sequence mainSeq = DOTween.Sequence();
        isPouring = true;
        segmentedColoredPipe.RemoveVertextList(duration, count);

        for (int i = 0; i < count; i++)
        {
            passengerIndexs.RemoveAt(0);
            //MeshRenderer liquid = liquids[0];
            //liquids.RemoveAt(0);

            //float targetY = liquid.gameObject.transform.position.y;
            //targetY -= stepY * i;
            //mainSeq.Join(liquid.transform.DOMoveY(targetY, duration * i).SetEase(Ease.Linear).OnComplete(() =>
            //{
            //    float valueA = liquid.material.GetFloat("_Fill");
            //    float targetA = 0;

            //    DOTween.To(() => valueA, x => valueA = x, targetA, duration)
            //      .SetEase(Ease.Linear).OnUpdate(() => liquid.material.SetFloat("_Fill", valueA));
            //}));
        }
        for (int i = 0; i < liquids.Count; i++)
        {
            //float targetY = liquids[i].gameObject.transform.position.y;
            //targetY -= stepY * count;
            //mainSeq.Join(liquids[i].transform.DOMoveY(targetY, duration * count).SetEase(Ease.Linear));
        }
        //DOVirtual.DelayedCall(duration * count, () =>
        //{
        //    isPouring = false;
        //});

    }

    [HideInInspector] public bool isPouring = false;


    public event Action<List<int>> OnPouringDone;
    public void PouringDone()
    {
        if (passengerIndexs.Count <= 0)
        {
            DOVirtual.DelayedCall(duration * 3, () =>
            {
                LevelManager.I.Win();
            });
           
            return;
        }

        isPouring = false;
        List<int> colorGroup = new List<int>();
        colorGroup.Add(Mathf.Abs(passengerIndexs[0]));
        for (int i = 1; i < passengerIndexs.Count; i++)
        {
            if (Mathf.Abs(passengerIndexs[i]) == Mathf.Abs(passengerIndexs[0])) colorGroup.Add(Mathf.Abs(i));
            else break;
        }

        OnPouringDone?.Invoke(colorGroup);
    }
    public List<int> FindColorGroup()
    {
        if(!isPouring)
        {
            List<int> colorGroup = new List<int>();
            colorGroup.Add(Mathf.Abs(passengerIndexs[0]));
            for (int i = 1; i < passengerIndexs.Count; i++)
            {
                if (Mathf.Abs(passengerIndexs[i]) == Mathf.Abs(passengerIndexs[0])) colorGroup.Add(Mathf.Abs(i));
                else break;
            }
            return colorGroup;
        }

        else
        {
            return null;
        }
    }
  
    public void GeneratePassenger()
    {
        segmentedColoredPipe.ClearAll();
        //List<Color> cls = new List<Color>()
        List<int> cls = new List<int>();
        for (int i = passengerIndexs.Count -1 ; i >= 0; i--)
        {
            //cls.Add(colors.colorWithIDs[passengerIndexs[i] - 1].liquidColor);
            int index = passengerIndexs[i] >= 0 ? passengerIndexs[i] - 1 : passengerIndexs[i] + 1;
            cls.Add(index);
            //var go_Li = Instantiate(liquid);
            //go_Li.transform.SetParent(transform, false);
            //go_Li.transform.localPosition = new Vector3(0, startY + ( (i+1) * stepY), 0);
            //ChangeColor(colors.colorWithIDs[passengerIndexs[i] -1], go_Li.material);
            //go_Li.gameObject.SetActive(true);
            //liquids.Add(go_Li);

            //if (i < passengerIndexs.Count - 1 )
            //    if (passengerIndexs[i] == passengerIndexs[i + 1])
            //        go_Li.material.SetFloat("_Fill", 0.45f);
        }
        segmentedColoredPipe.GenerateSegmentedPipe(cls);
    }
    public void ChangeColor(ColorWithID color, Material liquid)
    {
        //.material.SetColor("_LiquidColor" "_SurfaceColor" _PresenalColor _OutLine_Color)
        liquid.SetColor("_LiquidColor", color.liquidColor);
        liquid.SetColor("_SurfaceColor", color.surfaceColor);
        liquid.SetColor("_PresenalColor", color.gradiantColor);

    }
}

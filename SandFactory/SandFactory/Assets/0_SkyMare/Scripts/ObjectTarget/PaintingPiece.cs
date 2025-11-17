using DG.Tweening;
using DG.Tweening.Core.Easing;
using System;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
public class PaintingPiece : MonoBehaviour
{
    public List<PaintingPiece> neighbors = new List<PaintingPiece>();

    [Serializable]
    public enum PieceStatus
    {
        enable,
        disable,
        done
    }

    public PieceStatus status;

    //private SpriteRenderer sand;

    private SpriteRenderer spriteRenderer;
    private Material material;

    public SandPouringLocation sandPouringLocation;

    //private Material sandMaterial;
    public SpriteSandFill sandFill { get; set; }

    private TMP_Text sandValueTxt;

    public int totalSandValue;//{ get; private set; }
    public int currentSandValue { get; private set; }

    public int colorID;

    private Tween sandFillTween;
    private float currentProgress = 0;
    public bool isTwenning { get; private set; }

    public event Action<PaintingPiece> PaintingEnable;// xác nhận khi fill process = 1
    public event Action<PaintingPiece> PaintingDone;// xác nhận khi fill process = 1
    public event Action<PaintingPiece> PaintingFull; // xác nhận khi hoàn thành fill;
    public event Action FillDone; // xác nhận khi hoàn thành fill;

    public void LoadData(GameObject text, AnimationCurve edgeCurve, Vector2 textLocalPosition, int totalSand, int colorID)
    {
        var txt = Instantiate(text);
        txt.transform.SetParent(transform, false);
        txt.transform.localPosition = textLocalPosition;
        totalSandValue = totalSand;
        this.colorID = colorID;

        spriteRenderer = GetComponent<SpriteRenderer>();
        material = spriteRenderer.material;
        spriteRenderer.material = material;
        spriteRenderer.sortingOrder = 2;

        sandValueTxt = txt.GetComponentInChildren<TMP_Text>();
        sandValueTxt.text = totalSandValue.ToString();
        currentSandValue = totalSandValue;
   
        sandPouringLocation = this.AddComponent<SandPouringLocation>();
        sandPouringLocation.CalculateProcessFromPosition(txt.transform.position);

        //sandFill = GetComponentInChildren<SpriteSandFill>();
       
        sandFill.Reset();
        sandFill.Prepare();

        sandFill.edgeCurve = edgeCurve;
        sandFill.SetHighest(sandPouringLocation.process);

        //sand = GetComponentsInChildren<SpriteRenderer>()
        //.FirstOrDefault(sr => sr.gameObject != gameObject);

        //sandMaterial = new Material(sand.material);
        //sand.material = sandMaterial;
        //sandMaterial.SetFloat("_ApexX01", sandPouringLocation.process);
        //sandMaterial.name = "sandFillClone";

    }

    public void Fill(int sandValue, out int currentSand)
    {
        isTwenning = true;
        currentSandValue -= sandValue;
        currentSand = currentSandValue;
        if (currentSandValue <= 0)
        {
            PaintingDone?.Invoke(this);
        }
       
    }
    Tween waitting;
    public void FillAnimation(SandJar sandBox, int currentSand)
    {
        sandValueTxt.text = currentSandValue.ToString();

        if(currentSand <= currentSandValue)
        {
            waitting.Kill();
            waitting = DOVirtual.DelayedCall(0.75f, () =>
            {
                SandFill(currentSand);
            });
        }
        else
        {
            // Đã có sand khác với fill lớn hơn
        }
       
        DestroySandBox(sandBox);
    }
    private void OnDestroy()
    {
        for (int i = 0;i < jarings.Count; i++)
        {
            if(jarings != null) Destroy(jarings[i].gameObject);
        }
    }
    List<SandJar> jarings = new List<SandJar>();
    public void DestroySandBox(SandJar sandBox)
    {
        jarings.Add(sandBox);
        
       DOVirtual.DelayedCall(3f, () =>
        {
            ObjectPoolManager.I.Spawn("MiniExp", sandBox.jar.liquid.transform.position);
            jarings.Remove(sandBox);
            Destroy(sandBox.gameObject);
        });

    }
    public bool isFilling = false;
    private void SandFill(int currentSandValue)
    {
        Debug.Log($"Start sand Fill {gameObject.name} {currentSandValue}");
        spriteRenderer.sortingOrder = 0;
        currentProgress = (float)(totalSandValue - currentSandValue) / (float)totalSandValue;
        if (currentProgress >= 1)
        {
            sandValueTxt.text = "";
            Debug.Log("Painting Done");
        }
        else
        {
            currentProgress -= 0.1f;
            currentProgress = Mathf.Clamp(currentProgress, 0.1f, 1);
        }
        
        //sandFillTween.Kill();
        sandFill.StartFill(currentProgress, OnFillComplete);
        //sandFillTween = sandMaterial.Tween("_Progress01", currentProgress *0.8f, 1, OnFillComplete);
    }

    private void OnFillComplete()
    {
        isFilling = false;
        Debug.Log("Fill Done");
        isTwenning = false;
       
        if(currentProgress >= 1)
        {
            HapticManager.PlayPreset(HapticManager.Preset.MediumImpact);
            PaintingFull?.Invoke(this);
            EnableNeighbor();
            Debug.Log("Painting Done");
        }
        else
        {
            FillDone?.Invoke();
        }
    }

    private void EnableNeighbor()
    {
        for (int i = 0;  i  < neighbors.Count; i++)
        {
            if (neighbors[i].status == PieceStatus.enable) continue;
            neighbors[i].EnablePainting();
        }
    }
    public void ChangeStatus(PieceStatus status)
    {
        this.status = status;

        switch(status)
        {
            case PieceStatus.enable:
                EnablePainting();
                break;
            case PieceStatus.disable:
                DisablePainting();
                break;
            case PieceStatus.done:
                break;
        }
    }

    public void EnablePainting()
    {
        if (this.status == PieceStatus.enable) return;

        ObjectPoolManager.I.Spawn("ExpFx", transform.position);

        Debug.Log($"On Piece Painting Enable Color {gameObject.name}");

        this.status = PieceStatus.enable;

        PaintingEnable?.Invoke(this);
        sandValueTxt.gameObject.SetActive(true);
        // Giá trị ban đầu
        float startValue = 1f;

        // Giá trị đích
        float targetValue = 0f;

        // Tween giá trị fill
        DOTween.To(() => startValue, x =>
        {
            startValue = x;
            material.SetFloat("_FillAmount", startValue);
        },
        targetValue, 0.1f)
        .SetEase(Ease.Linear);
    }
    public void DisablePainting()
    {
        sandValueTxt.gameObject.SetActive(false);
        material.SetFloat("_FillAmount", 1);
    }

}

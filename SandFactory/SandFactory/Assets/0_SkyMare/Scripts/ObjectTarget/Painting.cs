using Data;
using DG.Tweening;
using System;
using System.Collections.Generic;
using System.Linq;
using ToolLevel;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.U2D;

public class Painting : MonoBehaviour
{
    public List<PaintingPiece> piecesEnable = new List<PaintingPiece>();
    List<PaintingPiece> allPieces = new List<PaintingPiece>();
    public List<PaintingPiece> piecesDone = new List<PaintingPiece> ();

    SpriteRenderer line;
    public Material a2Material;
    public event Action UnlockNewPainting;
    public GameObject text;
    public AnimationCurve edgeCurve = AnimationCurve.EaseInOut(0, 1, 1, 0);

    public void Reset()
    {
        piecesEnable.Clear();
        piecesDone.Clear();
        ClearPiece();
        Destroy(line.gameObject);

    }
    public void ClearPiece()
    {
        for(int i = 0; i < allPieces.Count; i++)
        {
            allPieces[i].PaintingDone -= OnPiecePaintingDone;
            allPieces[i].PaintingFull -= OnPiecePaintingFull;
            allPieces[i].FillDone -= OnPieceFillingDone;
            Destroy(allPieces[i].gameObject);
        }
        allPieces.Clear();
    }
    public void LoadPainting(PicData data )
    {
        if (line != null) Destroy(line.gameObject);

        for (int i = 0; i < allPieces.Count; i++)
        {
            Destroy(allPieces[i].gameObject);
        }
        allPieces.Clear();
        piecesEnable.Clear();
        piecesDone.Clear();

        for (int i = 0; i < data.currentSprites.Count; i++)
        {
            var ifr = data.currentSprites[i];
            Piece pieceData = data.levelData.painting.pieceList.FirstOrDefault( p =>  p.id == ifr.sprite.name);

            if (ifr.sprite.name == "Line")
            {
                GameObject lineObj = new GameObject("Line");
                line = lineObj.AddComponent<SpriteRenderer>();
                line.sortingOrder = 40;
                line.sprite = ifr.sprite;
                line.gameObject.transform.SetParent(transform, false);
                line.transform.localPosition = ifr.localPosition;
            }
            else if (ifr.sprite.name.Contains("base"))
            {
                GameObject gop = new GameObject();

                var p = gop.AddComponent<SpriteRenderer>();
                p.material = a2Material;
                p.sprite = ifr.sprite;
                p.transform.SetParent(transform, false);
                p.transform.localPosition = ifr.localPosition;
                gop.name = ifr.sprite.name;

                gop.AddComponent<PolygonCollider2D>();

                var pd = p.AddComponent<PaintingPiece>();

                if (data.levelData.painting.activeOnStart.Contains(gop.name))
                {
                    piecesEnable.Add(pd);
                }

                allPieces.Add(pd);

                GameObject gos = new GameObject();
                gos.name = ifr.sand.name;
                var s = gos.AddComponent<SpriteRenderer>();
                s.sprite = ifr.sand;
                s.sortingOrder = 5;
                gos.transform.SetParent(gop.transform, false);
                pd.sandFill = gos.AddComponent<SpriteSandFill>();
                //pd.sandFill.origin = s.sprite;

                pd.LoadData(text, edgeCurve, pieceData.textPosition, pieceData._amount , pieceData._colorID);
            
            }
        }
        LoadNeighBors(data.levelData.painting);
        StartActivePainting();
    }
    public void LoadNeighBors(Data.Painting data)
    {

        for (int i = 0; i < data.pieceList.Count; i++)
        {
            Piece pieceData = data.pieceList[i];
            PaintingPiece pieceManager = allPieces[i];
            pieceManager.neighbors.Clear();
            for (int j = 0; j < pieceData.neighbors.Count; j++)
            {
                pieceManager.neighbors.Add(allPieces.FirstOrDefault(pi => pi.name == pieceData.neighbors[j]));
            }

        }
    }
    public void StartActivePainting()
    {
        DOVirtual.DelayedCall(0.2f, () =>
        {
            EnablePeice();
        });
      
    }
    public void OnPiecePaintingDone(PaintingPiece p)
    {
        p.PaintingDone -= OnPiecePaintingDone;
        piecesEnable.Remove(p);
    }
    public void OnPiecePaintingFull(PaintingPiece p)
    {
        p.PaintingFull -= OnPiecePaintingFull;
        piecesDone.Add(p);

        if (piecesDone.Count == allPieces.Count)
        {
            LevelManager.I.Win();
        }
    }
    /// <summary>
    /// Nếu có 1 piece isFilling thì true, còn không thì false
    /// </summary>
    /// <returns></returns>
    public bool HavePieceFilling()
    {
        for (int i = 0; i < allPieces.Count; i++)
        {
            if(allPieces[i].isFilling)
                return true;
        }
        return false;
    }
    public void OnPieceFillingDone()
    {
        LevelManager.I.m_stack.Check(this.name);
    }

    public void OnPiecePaintingEnable(PaintingPiece p)
    {
        Debug.Log($"On Piece Painting Enable {p.name}");
        p.PaintingEnable -= OnPiecePaintingEnable;
        piecesEnable.Add(p);
        UnlockNewPainting?.Invoke();
    }

    public void EnablePeice()
    {
        for (int i = 0; i < allPieces.Count; i++)
        {
            allPieces[i].PaintingDone += OnPiecePaintingDone;
            allPieces[i].PaintingFull += OnPiecePaintingFull;
            allPieces[i].FillDone += OnPieceFillingDone;

            if (piecesEnable.Contains(allPieces[i]))
            {
                allPieces[i].ChangeStatus(PaintingPiece.PieceStatus.enable);
            }
            else
            {
                allPieces[i].PaintingEnable += OnPiecePaintingEnable;
                allPieces[i].ChangeStatus(PaintingPiece.PieceStatus.disable);

            }
        }
           
    }
}

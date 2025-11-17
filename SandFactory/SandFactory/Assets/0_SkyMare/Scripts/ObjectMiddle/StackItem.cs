using Data;
using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;
using static UnityEngine.Rendering.DebugUI;
using Sequence = DG.Tweening.Sequence;

public class StackItem : MonoBehaviour
{
    public StackManager m_stackManager;
    public bool isEmpty = true;
    public bool stackReady = false;

    public SandJar jar;
    public Transform jarPos;
    public int colorID { get; private set; }
    public int sandValue;// { get; private set; }

    public bool falseToFind;
    private void Start()
    {
        m_stackManager = GetComponentInParent<StackManager>();
    }
    public void Reset()
    {
        colorID = 0;
        sandValue = 0;
        isEmpty = true;
        stackReady = false;
        falseToFind = false;
        if(jar != null)
        {
            Destroy(jar.gameObject);
        }
      
    }
    public void StackOn(SandJar sandBox, int colorID, int sandValue, out System.Action done)
    {
        this.jar = sandBox;
        this.colorID = colorID;
        this.sandValue = sandValue;
        done = null;

        if (!HaveMatchPicOnStart())
        {
            done = () =>
            {
                stackReady = true;
                Debug.Log($"Start check Lose");
                m_stackManager.Check(this.name);
            };
        }
       

        //FindPaintingPiece();
    }
    private void Update()
    {
        //if (Input.GetKeyDown(KeyCode.A))
        //{
        //    sandValue -= 5;
        //    CloneNewJar(null, 5);
        //}

        if (!stackReady) return;
        FindPaintingPiece();

      
    }
    private bool HaveMatchPicOnStart()
    {
        var piece = Piece();
        if (piece == null) return false;

        if (sandValue <= piece.currentSandValue)
        {

            JumpToFill(piece, this.jar, sandValue, jar.gameObject);
            isEmpty = true;
            stackReady = false;
            jar = null;
            return true;
        }
        return false;
    } 
    private void FindPaintingPiece()
    {
        var piece = Piece();

        if (piece == null) return;

        if (sandValue > piece.currentSandValue)
        {
            sandValue -= piece.currentSandValue;

            int newSandValue = piece.currentSandValue;
            CloneNewJar(piece, sandValue, newSandValue);

            //jar.sandAmountTxt.text = sandValue.ToString();
        }
        else
        {
            JumpToFill(piece, this.jar, sandValue, jar.gameObject);

            stackReady = false;
            //jar = null;
        }

    }
    public List<SandJar> jarClones = new List<SandJar>();

    public Transform clonePosition;
    bool clonePossIsEmpty = true;
    public void CloneNewJar(PaintingPiece piece, int oldSandValue, int newSandValue)
    {
        int currentSand = 0;
        piece.Fill(newSandValue, out currentSand);
        piece.isFilling = true;

        Debug.Log("Spawn new jar");

        clonePossIsEmpty = false;

        SandJar sc = Instantiate(jar, jar.transform.position, jar.transform.rotation);

        sc.transform.localScale = Vector3.zero;

        jarClones.Add(sc);

        StartCoroutine(WaitingSpawming(sc, piece, oldSandValue, newSandValue, currentSand));

        //JumpToFill(piece, sc, piece.currentSandValue, sc.gameObject);


    }
    public IEnumerator WaitingSpawming(SandJar sc, PaintingPiece piece, int oldSandValue, int newSandValue, int currentSand)
    {
        yield return new WaitUntil(() => jarClones.IndexOf(sc) == 0);

        ObjectPoolManager.I.Spawn("ExpFx", sc.transform.position);

        sc.sandAmountTxt.text = "0";


        sc.jarAnimation.SpawnNewJar(clonePosition.position, 0.5f, () =>
        {
            TextTween(jar.sandAmountTxt, -1, oldSandValue, 0.5f);
            TextTween(sc.sandAmountTxt, 0, newSandValue, 0.5f);
        },
        () =>
        {
            jarClones.Remove(sc);
            sc.jarAnimation.JumpTopicture(piece, () =>
            {
                Debug.Log("Start Fill");
                piece.FillAnimation(sc, currentSand);
            });

        });
    }

    public void JumpToFill(PaintingPiece piece, SandJar jar, int sand, GameObject jarObject)
    {
        int currentSand = 0;
        piece.Fill(sand, out currentSand);
        piece.isFilling = true;// đặt flag filling luông không cần done animation
        StartCoroutine(WaitingSpawming(jar, piece, currentSand));
       
    }
    public IEnumerator WaitingSpawming(SandJar sc, PaintingPiece piece, int currentSand)
    {
        yield return new WaitUntil(() => jarClones.Count == 0);
        this.jar = null;
        isEmpty = true;
        

        sc.jarAnimation.JumpTopicture(piece, () =>
        {
            Debug.Log("Start Fill");
            piece.FillAnimation(sc, currentSand);
        });
    }
    public void TextTween(TMP_Text value, float valueA, float targetA, float duration)
    {
        if(valueA < 0)
        {
            valueA = float.Parse(value.text);
        }

        Sequence seq = DOTween.Sequence();
        seq.Append(DOTween.To(() => valueA, x => valueA = x, targetA, duration*0.8f)
          .OnUpdate(() => value.text = ((int)valueA).ToString())
          .OnComplete(()=> value.text = ((int)targetA).ToString())
          .SetEase(Ease.InQuad));

    }
    //private void FindPaintingPiece()
    //{

    //    var piece = Piece();
    //    if(piece == null)
    //    {
    //        if (!falseToFind)
    //        {
    //            // Không tìm nữa, đợi call back từ painting
    //            falseToFind = true;
    //            LevelManager.I.m_painting.UnlockNewPainting += FindPaintingPiece;
    //        }

    //    }
    //    else
    //    {

    //        if (isEmpty) return;

    //        if (sandValue > piece.currentSandValue)
    //        {

    //            GameObject sc = Instantiate(sandBox, sandBox.transform.position, sandBox.transform.rotation);
    //            sc.transform.DOMove(piece.sandPouringLocation.SandPouringPos(), 0.4f);
    //            sc.GetComponentInChildren<TMP_Text>().text = piece.currentSandValue.ToString();

    //            sandValue -= piece.currentSandValue;
    //            piece.Fill(piece.currentSandValue, sc);
    //            sandBox.GetComponentInChildren<TMP_Text>().text = sandValue.ToString();

    //            if (!falseToFind)
    //            {
    //                // Không tìm nữa, đợi call back từ painting
    //                falseToFind = true;
    //                LevelManager.I.m_painting.UnlockNewPainting += FindPaintingPiece;
    //            }
    //            //StackOn(sandBox, colorID, sandValue);
    //        }
    //        else
    //        {
    //            isEmpty = true;
    //            //Reset
    //            sandBox.transform.DOMove(piece.sandPouringLocation.SandPouringPos(), 0.4f);
    //            piece.Fill(sandValue, sandBox);

    //            sandBox = null;

    //            // Hủy call back khi clear
    //            if (falseToFind)
    //            {
    //                falseToFind = false;
    //                LevelManager.I.m_painting.UnlockNewPainting -= FindPaintingPiece;
    //            }

    //        }

    //    }

    //}

    public PaintingPiece Piece()
    {
       
        var p = LevelManager.I.m_painting;

        return p.piecesEnable.FirstOrDefault(pe =>  pe.colorID == colorID);

    }
    //public 
}

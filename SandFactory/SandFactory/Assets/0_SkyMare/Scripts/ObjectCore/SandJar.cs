using DG.Tweening;
using System;
using System.Collections;
using TMPro;
using UnityEngine;


public class SandJar : MonoBehaviour
{
    [Header("Setting")]
    public HolderColorManager jar;
    public GameObject frezzeMesh;
    public GameObject key;
    public GameObject _lock;
    public SpriteRenderer hiddenSprite;

    public TMP_Text sandAmountTxt;
    public int sandAmount;
    public int colorID;
    public ColorWithID color;

    private Vector3 jarStartScale;

    [Header("Animation")]
    public SanJarAnimation2 jarAnimation;
    public Transform target;
    public PaintingPiece painting;



    public void TransJar(Slot item, Action actionDone = null)
    {
        item.isEmpty = false;

        System.Action stackDone;
        System.Action stackComplete;
        item.StackOn(this, colorID, sandAmount, out stackDone, out stackComplete);

        jarAnimation.JumpToStack(item.stopPosition,stackDone, stackComplete, item.visualHolder.transform);


    }
    #region apply 
    public void ApplyNormalWaittingJar(int colorID, ColorWithID color, int sandAmoun, int row)
    {
        Invative(true, false,false,false, false);

        ApplyValue(colorID, color, sandAmount, row);

        jar.ChangeColor(color);

    }
    public void ApplyHiddenWaittingJar(int colorID, ColorWithID color, int sandAmount, int row)
    {
        Invative(true, false, false, false, true);

        ApplyValue(colorID, color, sandAmount, row);

        jar.ChangeColor(color);
        jar.cap.material.color = Color.gray;
        jar.cap_Border.material.color = Color.gray;

    }
    public void ApplyFrezzWaittingJar(int colorID, ColorWithID color, int sandAmount, int row)
    {
        Invative(true, true, false, false, false);

        ApplyValue(colorID, color, sandAmount, row);

        jar.ChangeColor(color);

    }
    public void ApplyKeyWaittingJar(int colorID, ColorWithID color, int sandAmount, int row)
    {
        Invative(true, false, true, false, false);

        ApplyValue(colorID, color, sandAmount, row);

        jar.ChangeColor(color);

    }
    public void ApplyLockWaittingJar(int colorID, ColorWithID color, int sandAmount, int row)
    {
        Invative(true, false, false, true, false);

        ApplyValue(colorID, color, sandAmount, row);

        jar.ChangeColor(color);

    }
    Tween unfrezze;
    Action unfrezzeDone;
    public void UnFrezze()
    {
        //unfrezze = frezzeMesh.transform.DOScale(0, 0.75f).SetEase(Ease.InBack).OnComplete(() =>
        //{
        //    frezzeMesh.gameObject.SetActive(false);
        //    unfrezzeDone?.Invoke();
        //    unfrezzeDone = null;
        //});
        unfrezze = jarAnimation.UnFrezze(() =>
        {
            unfrezzeDone?.Invoke();
            unfrezzeDone = null;
        });
    }
    public void ActiveFrezze()
    {
        if (unfrezze.IsActive() && unfrezze.IsPlaying())
        {
            unfrezzeDone = ActiveNormal;
        }
        else
        {
            ActiveNormal();
        }
    }
    Tween unLock;
    Action unLockDone;
    public void UnLock(Transform key)
    {
        key.transform.SetParent(transform);
        jarAnimation.key = key;
        unLock = jarAnimation.Unlock(() =>
        {
              unLockDone?.Invoke();
              unLockDone = null;
        });
    }
    public void ActiveLock()
    {
        if (unLock.IsActive() && unLock.IsPlaying())
        {
            unLockDone = ActiveNormal;
        }
        else
        {
            ActiveNormal();
        }
    }

    public void HiddenStackDone()
    {
        hiddenSprite.DOFade(0, 0.5f);
        jar.cap.material.DOColor(color.capColor, 0.5f);
        jar.cap_Border.material.DOColor(color.capColor, 0.5f);
    }
    public void KeyStackDone()
    {
        //key.DOFade(0, 0.3f);
    }

    public void ActivePipe()
    {
        if (jarAnimation.sacleToNormal.IsActive() && jarAnimation.sacleToNormal.IsPlaying())
        {
            StartCoroutine(WaitSpawnDone());
        }
        else
        {
            ActiveNormal();
        }
    }
    IEnumerator WaitSpawnDone()
    {
        yield return new WaitUntil(() => !jarAnimation.sacleToNormal.IsPlaying());
        ActiveNormal();
    }
    #endregion
    public void ActiveNormal()
    {
        //jarStartScale = mainMesh.transform.localScale;
        //mainMesh.transform.DOScale(jarStartScale * 1.2f, 0.3f).SetEase(Ease.InBack);
     
        jarAnimation.Active();
    }

    private void Update()
    {
#if UNITY_EDITOR
        if(Input.GetKeyDown(KeyCode.A))
        {
            JumpAnimation();
        }
        if (Input.GetKeyDown(KeyCode.S))
        {
            JumpAnimation2();
        }
        if (Input.GetKeyDown(KeyCode.D))
        {
            jarAnimation.Active();
        }
        if (Input.GetKeyDown(KeyCode.F))
        {
            jarAnimation.UnFrezze();
        }
        if (Input.GetKeyDown(KeyCode.G))
        {
            //jarAnimation.Unlock();
        }
#endif
    }

   
    public void JumpAnimation()
    {
        //jarAnimation.JumpToStack(target);
       
    }
    public void JumpAnimation2()
    {
        jarAnimation.JumpTopicture(painting);

    }
    private void ApplyValue(int colorID, ColorWithID color, int sandAmount, int row)
    {
        this.colorID = colorID;
        this.color = color;

        ApplyAmount(sandAmount);

        jarAnimation.jumpHeight += row * 0.3f;
    }
    public void ApplyAmount(int sandAmount)
    {
        this.sandAmount = sandAmount;
        sandAmountTxt.text = sandAmount.ToString();
    }

    private void Invative(bool main, bool frezze, bool key , bool lockk, bool hidden)
    {
        jar.gameObject.SetActive(main);
        frezzeMesh.gameObject.SetActive(frezze);
        this.key.gameObject.SetActive(key);
        _lock.gameObject.SetActive(lockk);
        hiddenSprite.gameObject.SetActive(hidden);
    }
}

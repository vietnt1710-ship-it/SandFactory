using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SanJarAnimation2 : MonoBehaviour
{
    [Header("Active")]
    public Transform jar;
    public Transform cap;
    public GameObject quad;

    public float squatTime = 0.1f;  // Thời gian nhún
    public float scaleTime = 0.1f;

    public AnimationCurve activeCurve;
    public Wobble wobble;

    [Header("Jump")]
    public GameObject text;
    public float jumpHeight = 2f;   // Độ cao parabol
    public float jumpPicHeight = 2f;   // Độ cao parabol
    public float jumpDuration = 1f; // Thời gian nhảy
    public float jumpPouringPointDuration = 1f;
    public float jumpPicDuration = 1f;
    public float gravityScale = 1; // 
    public AnimationCurve speedCurve;
    public AnimationCurve speedCurve2;
    public Transform small;
    public Transform normal;
    public Transform stack;
    public SpriteRenderer shadow;

    [Header("Frezze")]
    public Transform ice;

    [Header("Key and Lock")]
    public Transform key;
    public Transform refKey;
    public Animator _lockAnim;

    [Header("Tilt")]
    public float maxTiltDegrees = 20f;    // góc nghiêng lớn nhất
    float tiltDegress;

    public float maxTiltDuration = 0.5f;     // thời gian 1 lần tilt
    float tiltDuration;

    public Ease tiltEase = Ease.OutSine;
    public List<Transform> pointTransforms;// 8 point;
    int step = 0;

    public bool isReady = false;

    [Header("Fx Position")]
    public Transform iceEffect;
    public Transform jumpFxPos;
    public Transform activeFx;
    public Transform lockFx;

    public PaintingPiece paintingPiece;

    public Animation bottleManager;
    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.A))
        {
            Active();
        }
        if (Input.GetKeyDown(KeyCode.L))
        {
            CloseCap();
        }
    }
    public Tween Unlock(Action done = null)
    {
        StartCoroutine(Jump(key, refKey, 3, 0.5f));
        key.DOScale(key.localScale * 1.75f, 0.25f).SetLoops(2, LoopType.Yoyo);
        key.DORotate(refKey.localEulerAngles, 0.5f).OnComplete(() =>
        {
            key.gameObject.SetActive(false);
            refKey.gameObject.SetActive(true);
            _lockAnim.SetTrigger("open");
            DOVirtual.DelayedCall(0.7f, () =>
            {
                _lockAnim.transform.DOScale(0, 0.2f).SetEase(Ease.InBack);
                DOVirtual.DelayedCall(0.1f, () =>
                {
                    ObjectPoolManager.I.Spawn("JumpFx", lockFx.position);
                });

            });
        });

        return DOVirtual.DelayedCall(1.5f, () => done?.Invoke());
    }
    public Tween UnFrezze(Action actionDone = null)
    {
        ObjectPoolManager.I.Spawn("IceFx", iceEffect.position);
        return ice.DOScale(0, 0.2f).SetEase(Ease.InBack).OnComplete(() =>
        {
            jar.gameObject.SetActive(true);
            ice.gameObject.SetActive(false);
            actionDone?.Invoke();
        });
    }
    public void JumpTopicture(PaintingPiece paintingPiece, Action done = null)
    {
        StopTilt();
        Vector3 targetPos = paintingPiece.sandPouringLocation.SandPouringPos();
        Vector3 startPos = transform.position;

        Vector3 direction = targetPos - startPos;
        direction.y = 0;
        direction.Normalize();
        Debug.Log("direction" + direction);
        //

        //shadow.DOFade(0, 0.3f);
        text.gameObject.SetActive(false);

        Jump().OnComplete(() =>
        {
            this.transform.SetParent(null); 
            jar.forward = direction;
            ObjectPoolManager.I.Spawn("JumpFx", jumpFxPos.position);

            //jar.DOScale(normal.localScale * 0.8f, jumpPicDuration);

            var jarRotate = jar.transform.localEulerAngles + new Vector3(52f, 0f, 0f);
            jarRotate.y = 0;
            jar.transform.DOLocalRotate(jarRotate, jumpPicDuration).SetEase(Ease.OutQuad);

            DOVirtual.DelayedCall(jumpPicDuration * 0.95f, () =>
            {
                Debug.Log("Start Tween Cap When start Stack to Picture pharse 2");
                Sequence seq3 = DOTween.Sequence();
                seq3.Append(cap.DOLocalRotate(new Vector3(0, 0, 135), squatTime * 2));//.SetEase(Ease.Linear).SetLoops(2, LoopType.Yoyo);
                seq3.Append(cap.DOLocalRotate(new Vector3(0, 0, 165), squatTime * 2).SetEase(Ease.OutBounce, 3));
            });
            StartCoroutine(Jump(transform, targetPos, jumpPicHeight, jumpPicDuration, 1, () =>
            {
              
                //var posy = transform.position.y;
                //transform.DOMoveY(posy - 0.5f, squatTime * 2).SetLoops(2, LoopType.Yoyo);

                wobble.CutWobble();
                DOVirtual.DelayedCall(squatTime * 4, () =>
                {
                    var jarRotate = jar.transform.localEulerAngles + new Vector3(0, 0f, -135f);
                    jarRotate.y = 0;
                    jar.transform.DOLocalRotate(jarRotate, 3, RotateMode.FastBeyond360)
          .SetEase(Ease.OutQuad);

                    DOVirtual.DelayedCall(0.3f, () =>
                    {
                        done?.Invoke();
                    });
                });

            }));

        });
    }
  
    Sequence ScaleToSmall()
    {
        Sequence seq = DOTween.Sequence();
        seq.Append(jar.DOScale(small.localScale, squatTime).SetEase(Ease.InBack));
        seq.Join(jar.DOLocalMove(small.localPosition, squatTime));
        seq.Append(jar.DOScale(normal.localScale, squatTime).SetEase(Ease.InBack));
        seq.Join(jar.DOLocalMove(normal.localPosition, squatTime));

        return seq;
    }
  
    public void JumpToPouringPoint(Transform pouringPoint, Action jumpComplete)
    {
        ObjectPoolManager.I.Spawn("MiniExp2", jumpFxPos.position);

        float baseStartPosY = go_Base.transform.position.y;
        go_Base.DOMoveY(baseStartPosY + 0.075f, 0.1f);

        transform.SetParent(null);
        cap.DOLocalRotate(new Vector3(0, 0, 215), 0.1f);
        DOVirtual.DelayedCall(jumpPouringPointDuration * 0.95f, () =>
        {
            Debug.Log("Start Tween Cap When start Stack to Picture pharse 2");
            Sequence seq3 = DOTween.Sequence();
            seq3.Append(cap.DOLocalRotate(new Vector3(0, 0, 135), 0.15f));//.SetEase(Ease.Linear).SetLoops(2, LoopType.Yoyo);
            seq3.Append(cap.DOLocalRotate(new Vector3(0, 0, 165), 0.15f).SetEase(Ease.OutBounce, 3));
        });
        shadow.DOFade(0, 0.1f);
        StartCoroutine(Jump(transform, pouringPoint, jumpHeight /4, jumpPouringPointDuration, () =>
        {
            DOVirtual.DelayedCall(0.15f, () =>
            {
                jumpComplete?.Invoke();
            });
          
        }));
    }
    public void BackToBase(Transform basePoint, Action jumpComplete, Action fillDone = null)
    {
        //float baseStartPosY = go_Base.transform.position.y;
        //go_Base.DOMoveY(baseStartPosY + 0.075f, 0.1f);
        cap.DOLocalRotate(new Vector3(0, 0, 115), 0.05f);

        DOVirtual.DelayedCall(jumpPouringPointDuration * 0.8f, () =>
        {
            Debug.Log("Start Tween Cap When start Stack to Picture pharse 2");
            Sequence seq3 = DOTween.Sequence();
            seq3.Append(cap.DOLocalRotate(new Vector3(0, 0, 175), squatTime * 2));//.SetEase(Ease.Linear).SetLoops(2, LoopType.Yoyo);
            seq3.Append(cap.DOLocalRotate(new Vector3(0, 0, 165), squatTime * 2).SetEase(Ease.OutBounce, 3));
        });

        StartCoroutine(Jump(transform, basePoint, 0.2f, jumpPouringPointDuration * 0.85f, () =>
        {
            transform.SetParent(go_Base);
            float baseStartPosY = go_Base.transform.position.y;
            go_Base.DOMoveY(baseStartPosY - 0.075f, 0.1f).OnComplete(() =>
            {
                jumpComplete?.Invoke();
                if (fillDone != null)
                {
                    fillDone?.Invoke();
                    CompleteFill();
                }
            });
        }));
    }
    public ParticleSystem hightLight;
    public ParticleSystem explosion;
    public void CompleteFill()
    {
        hightLight.gameObject.SetActive(true);
        bottleManager.Play();
        DOVirtual.DelayedCall(0.55f, () =>
        {
            explosion.transform.SetParent(null);
            explosion.gameObject.SetActive(true);
        });
        DOVirtual.DelayedCall(2f, () =>
        {
            Destroy(gameObject);
        });
        //gameObject.SetActive(false);
    }
    public void JumpToStack(Transform stack, Action jumpDone, Action jumpComplete, Transform go_Base = null)
    {
        ObjectPoolManager.I.Spawn("JumpFx", jumpFxPos.position);

        this.go_Base = go_Base;
        Vector3 startPos = transform.position;

        Vector3 direction = stack.position - startPos;
        direction.y = 0;
        direction.Normalize();


        //text.transform.DOScale(0, 0.1f);
        ScaleToSmall().OnComplete(() =>
        {
            transform.forward = direction;
            transform.DORotate(transform.eulerAngles + new Vector3(360f, 0f, 0f), jumpDuration, RotateMode.FastBeyond360)
    .SetEase(Ease.OutQuad);

            StartCoroutine(Jump(transform, stack, jumpHeight, jumpDuration, () =>
            {
                jumpDone?.Invoke();
                //text.transform.DOScale(1.3f, 0.1f);
                transform.forward = Vector3.zero;
                transform.eulerAngles = new Vector3(0, 60, 0);
                OpenCap();

                ScaleToSmall().OnComplete(() =>
                {
                    jumpComplete?.Invoke();
                });
            }));

        });
    }
  
    Sequence OpenCap(int time = 5)
    {
        Sequence seq = DOTween.Sequence();
        seq.Join(cap.DOLocalRotate(new Vector3(0, 0, 165), scaleTime * time).SetEase(Ease.OutBack, 3));
        return seq;
    }
    [Header("MainBase")]
    public Transform go_Base;
  
    Sequence Jump()
    {
        Sequence seq = DOTween.Sequence();
        var pos1 = transform.position.y;
        seq.Append(transform.DOMoveY(pos1 - 0.25f, squatTime * 2).SetEase(Ease.InQuad));
        seq.Append(transform.DOMoveY(pos1, squatTime * 0.75f));


        Sequence seq2 = DOTween.Sequence();
        seq2.Append(cap.DOLocalRotate(new Vector3(0, 0, 165), squatTime * 2f));
        //seq2.Append(cap.DOLocalRotate(new Vector3(0, 0, 135), squatTime * 1));//.SetEase(Ease.Linear).SetLoops(2, LoopType.Yoyo);
        seq2.Append(cap.DOLocalRotate(new Vector3(0, 0, 215), squatTime * 1));


        return seq;
    }

    Sequence JumpToNormalPosition()
    {
        Sequence seq = DOTween.Sequence();

        seq.Join(jar.DOLocalMove(normal.localPosition, scaleTime * 5).SetEase(activeCurve));

        Sequence seq2 = DOTween.Sequence();
        seq2.Join(cap.DOLocalRotate(new Vector3(0, 0, 45), scaleTime * 2.5f).SetEase(Ease.OutBack));
        seq2.Append(cap.DOLocalRotate(new Vector3(0, 0, 0), scaleTime * 2.5f));
        //seq.Append(jar.DOLocalMove(activePosition, scaleTime));
        return seq;
    }
    public Sequence CloseCap()
    {
        Sequence seq = DOTween.Sequence();
        seq.Join(cap.DOLocalRotate(new Vector3(0, 0, 0), scaleTime *3 ).SetEase(Ease.InBack,2));
        seq.Join(jar.DOLocalMove(normal.localPosition + new Vector3 (0,2, 0), scaleTime * 3).SetEase(Ease.InQuad));
        seq.Append(jar.DOLocalMove(normal.localPosition, scaleTime * 5).SetEase(Ease.OutBounce));

        //seq.Append(jar.DOLocalMove(activePosition, scaleTime));
        return seq;
    }
    public Tween sacleToNormal;
    public Sequence ScaleToNormal(Vector3 position, float time, Action done = null)
    {
        quad.gameObject.SetActive(true);
        StopTilt();
        sacleToNormal = DOVirtual.DelayedCall(time, () =>
        {
            done?.Invoke();
        });
        Sequence seq = DOTween.Sequence();
        seq.Append(transform.DOScale(1, time).From(Vector3.zero)).SetEase(Ease.InBack);
        seq.Join(transform.DOLocalMove(position, time));

        return seq;
    }

    public void Active()
    {
        StopTilt();
        ObjectPoolManager.I.Spawn("ActiveFx", activeFx.position);
        JumpToNormalPosition().OnComplete(() =>
        {
            cap.GetComponent<MeshRenderer>().materials[1].SetFloat("_OutLine_Thickness", 0.02f); 
            quad.gameObject.SetActive(false);
            isReady = true;
            //StartTilt();
        });

    }
    public bool isClone { get; set; } = false;
    public void SpawnNewJar(Vector3 pos, float fillTime, Action fill, Action done)
    {
        wobble.SetEmpty();
        isClone = true;
        cap.DOLocalRotate(new Vector3(0, 0, 0), 0);
        Sequence seq = DOTween.Sequence();
        seq.Append( transform.DOScale(1, scaleTime * 3 ).From(0).SetEase(Ease.OutBack));
        seq.Join(transform.DOMove( pos, scaleTime * 3).SetEase(Ease.OutQuad));
        seq.Join(cap.DOLocalRotate(new Vector3(0, 0, 165), scaleTime * 6).SetEase(Ease.OutBounce, 3));
        seq.OnComplete(() =>
        {
            //wobble.Fill(fillTime);
            fill?.Invoke();

            DOVirtual.DelayedCall(fillTime, () =>
            {
                done?.Invoke();
            });
        });
    }

    Sequence tilt;
    void StopTilt()
    {
        tilt.Kill();
        jar.transform.SetParent(transform);
        jar.transform.localEulerAngles = new Vector3 (0, 0, 0);

    }

    public void StartTilt(float per = 1)
    {
        StopTilt();

        int steps = pointTransforms.Count;

        for (int i = 0; i < steps; i++)
        {
            Vector3 direction = jar.position - pointTransforms[i].position;
            direction.y = 0;
            direction.Normalize();
            Debug.Log("direction" + direction);
            pointTransforms[i].forward = direction;
        }
        int randomIdx = UnityEngine.Random.Range(0, steps);
        int oppositeIdx = (randomIdx + 4) % steps;

        step = 0;
        tiltDegress = maxTiltDegrees * per;
        tiltDuration = maxTiltDuration * per;

        StartTiltAtPoint(randomIdx, oppositeIdx);
    }
    void StartTiltAtPoint(int idx1, int idx2)
    {
        if (step > 10)
        {
            return;
        }
        int idx = step % 2 == 0 ? idx1 : idx2;

        Transform point = pointTransforms[idx];
        Vector3 startAngle = point.localEulerAngles;
        Vector3 targetAngle = step % 2 == 0 ? startAngle + point.right * tiltDegress : startAngle - point.right * tiltDegress;
        targetAngle.z = startAngle.z;
        tilt.Kill();
        tilt = DOTween.Sequence();

        tilt.Join(point.DOLocalRotate(targetAngle, tiltDuration).SetEase(tiltEase));
        tilt.Append(point.DOLocalRotate(startAngle, tiltDuration).SetEase(tiltEase));
        tilt.OnStart(() => { jar.transform.SetParent(point); });
        tilt.OnComplete(() =>
        {
            jar.transform.SetParent(transform);
            DownSizeAngle();
            StartTiltAtPoint(idx1, idx2);
        });
    }
    public void DownSizeAngle()
    {
        step++;
        tiltDegress = tiltDegress * 0.8f;
        tiltDuration = tiltDuration * 0.8f;

    }

    public IEnumerator Jump(Transform transform, Vector3 target, float height, float duration, float arcOffset = 1f, Action actionDone = null)
    {
        Vector3 normalScale = normal.transform.localScale;
        Vector3 maxScale = normal.transform.localScale * 1.3f;
        Vector3 minScale = normal.transform.localScale * 0.7f;
        Vector3 startPos = transform.position;
        float elapsed = 0f;

        // Tính độ chênh lệch Y giữa điểm bắt đầu và điểm đích
        float deltaY = target.y - startPos.y;

        // Tính toán lại các thông số vật lý
        float gravity = (8f * height) / (duration * duration) * gravityScale;
        float initialVelocityY = ((4f * height) / duration + deltaY / duration) * gravityScale;

        // Tính thời điểm đạt độ cao tối đa (khi vận tốc Y = 0)
        float timeAtMaxHeight = initialVelocityY / gravity;

        while (elapsed < duration)
        {
            float normalizedTime = elapsed / duration;
            float curveValue = speedCurve2.Evaluate(normalizedTime);
            elapsed += Time.deltaTime * curveValue;
            if (elapsed >= duration) break;

            float t = elapsed / duration;

            // Di chuyển theo phương ngang (X-Z) tuyến tính
            Vector3 currentPos = Vector3.Lerp(startPos, target, t);

            // Tính toán vị trí Y theo công thức parabol
            float currentTime = elapsed;
            currentPos.y = startPos.y + (initialVelocityY * currentTime) - (0.5f * gravity * currentTime * currentTime);

            transform.position = currentPos;

            // Tính toán scale dựa trên vị trí trong quỹ đạo
            Vector3 currentScale;
            if (elapsed < timeAtMaxHeight)
            {
                // Giai đoạn lên: normalScale -> maxScale
                float scaleProgress = elapsed / timeAtMaxHeight;
                currentScale = Vector3.Lerp(normalScale, maxScale, scaleProgress);
            }
            else
            {
                // Giai đoạn xuống: maxScale -> minScale
                float scaleProgress = (elapsed - timeAtMaxHeight) / (duration - timeAtMaxHeight);
                currentScale = Vector3.Lerp(maxScale, minScale, scaleProgress);
            }

            jar.transform.localScale = currentScale;

            yield return null;
        }

        // Đảm bảo vị trí và scale cuối chính xác
        transform.position = target;
        jar.transform.localScale = minScale;

        actionDone?.Invoke();
    }
    private float GetArcDirection(Transform transform, Vector3 target)
    {
        float deltaX = target.x - transform.position.x;
        float distance = Vector3.Distance(transform.position, target);
        // Trả về 1 nếu lệch phải, -1 nếu lệch trái
        // Nếu deltaX gần bằng 0, có thể random hoặc mặc định
        if (Mathf.Abs(deltaX * distance) < 0.1f)
        {
            // Nếu đi gần như thẳng, random lệch trái hoặc phải
            return 0.75f;
        }

        return -Mathf.Sign(deltaX * distance);
    }
    public IEnumerator Jump(Transform transform, Transform target, float height, float duration, Action actionDone = null)
    {
        Vector3 startPos = transform.position;
        float elapsed = 0f;

        // Tính độ chênh lệch Y giữa điểm bắt đầu và điểm đích
        float deltaY = target.position.y - startPos.y;

        // Tính toán vận tốc ban đầu theo phương Y để đạt độ cao mong muốn
        float gravity = (8f * height) / (duration * duration) * gravityScale;
        //float initialVelocityY = (4f * height) / duration * gravityScale;
        float initialVelocityY = ((4f * height) / duration + deltaY / duration) * gravityScale;

        while (elapsed < duration)
        {
            // Lấy giá trị speed từ curve (0-1)
            float normalizedTime = elapsed / duration;
            float curveValue = speedCurve.Evaluate(normalizedTime);

            // Áp dụng curve value vào deltaTime
            elapsed += Time.deltaTime * curveValue;
            float t = elapsed / duration;

            // Di chuyển theo phương ngang (X-Z) tuyến tính
            Vector3 currentPos = Vector3.Lerp(startPos, target.transform.position, t);

            // Tính toán vị trí Y theo công thức parabol với gia tốc
            // y = y0 + v0*t - 0.5*g*t^2
            float currentTime = elapsed;
            currentPos.y = startPos.y + (initialVelocityY * currentTime) - (0.5f * gravity * currentTime * currentTime);

            transform.position = currentPos;
            yield return null;
        }

        // Đảm bảo vị trí cuối chính xác
        transform.position = target.transform.position;
        actionDone?.Invoke();
    }

}

using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class SandJarAnimation : MonoBehaviour
{
    [Header("Jump Settings")]     // Vị trí nhảy tới
    public float jumpHeight = 2f;   // Độ cao parabol
    public float jumpPicHeight = 2f;   // Độ cao parabol
    public float jumpDuration = 1f; // Thời gian nhảy
    public float jumpPicDuration = 1f;
    public float gravityScale = 1; // 

    [Header("Reference Settings")]
    public Transform small;
    public Transform normal;
    public Transform big;

    public float squatTime = 0.1f;  // Thời gian nhún
    public float scaleTime = 0.1f;

    [Header("Object Settings")]
    public Transform jar;
    public Transform ice;
 
    public AnimationCurve speedCurve;
    public SpriteRenderer shadow;
    public GameObject text;
    public Transform centerAnchor;
  
    [Header("Tilt")]
    public float maxTiltDegrees = 20f;    // góc nghiêng lớn nhất
    float tiltDegress;
    public float maxTiltDuration = 0.5f;     // thời gian 1 lần tilt
    float tiltDuration;
    public Ease tiltEase = Ease.OutSine;
    public List<Transform> pointTransforms;// 8 point;
    int step = 0;
  

    [Header("Key and Lock")]
    public Transform key;
    public Transform refKey;
    public Animator _lockAnim;
 

    [Header("Fx Position")]
    public Transform iceEffect;
    public Transform jumpFxPos;
    public Transform activeFx;
    public Transform lockFx;

    public bool isReady = false;

    public void OnDestroy()
    {
        StopAllCoroutines();
    }
    public Tween Unlock(Action done = null)
    {
        StartCoroutine( Jump(key, refKey, 3, 0.5f));
        key.DOScale(key.localScale * 1.75f, 0.25f).SetLoops(2, LoopType.Yoyo);
        key.DORotate(refKey.localEulerAngles, 0.5f).OnComplete(() =>
        {
            key.gameObject.SetActive(false);
            refKey.gameObject.SetActive(true);
            _lockAnim.SetTrigger("open");
            DOVirtual.DelayedCall(0.7f, () =>
            {
                _lockAnim.transform.DOScale(0,0.2f).SetEase(Ease.InBack);
                DOVirtual.DelayedCall(0.1f, () =>
                {
                    ObjectPoolManager.I.Spawn("JumpFx", lockFx.position);
                });
                
            });
        });

        return DOVirtual.DelayedCall(1.5f, () => done?.Invoke());
    }
    public Tween sacleToNormal;
    public Sequence ScaleToNormal(Vector3 position, float time, Action done = null)
    {
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
    Sequence ScaleToSmall()
    {
        Sequence seq = DOTween.Sequence();
        seq.Append(jar.DOScale(small.localScale, squatTime).SetEase(Ease.InBack));
        seq.Join(jar.DOLocalMove(small.localPosition, squatTime));
        seq.Append(jar.DOScale(big.localScale, squatTime).SetEase(Ease.InBack));
        seq.Join(jar.DOLocalMove(big.localPosition, squatTime));

        return seq;
    }
    Sequence ScaleToBig()
    {
        Sequence seq = DOTween.Sequence();
        seq.Append(jar.DOScale(big.localScale, scaleTime).From(normal.localScale).SetEase(Ease.InOutBack, 2));
        seq.Join(jar.DOLocalMove(big.localPosition - new Vector3(0,0.5f,0), scaleTime*2).From(normal.localPosition));
        seq.Append(jar.DOLocalMove(big.localPosition, scaleTime));
        return seq;
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
        jar.forward = direction;

        shadow.DOFade(0, 0.3f);
        text.gameObject.SetActive(false);

        ScaleToSmall().OnComplete(() =>
        {
            ObjectPoolManager.I.Spawn("JumpFx", jumpFxPos.position);

            Vector3 targetAngle = transform.eulerAngles - transform.right * 412f;
            //targetAngle.x = -52;
            jar.DOLocalRotate(targetAngle, jumpPicDuration*0.95f, RotateMode.FastBeyond360).SetEase(Ease.OutQuad);

            jar.DOScale(normal.localScale * 0.8f, jumpPicDuration);

            StartCoroutine(Jump(transform, targetPos, jumpPicHeight, jumpPicDuration, 1, () =>
            {
                DOVirtual.DelayedCall(0.1f, () =>
                {
                    Vector3 targetAngle = jar.eulerAngles - jar.forward * 165;
                    targetAngle.x = 52;
                    targetAngle.y = 0;
                    jar.DORotate(targetAngle, 2);

                    DOVirtual.DelayedCall(0.3f, () =>
                    {
                        done?.Invoke();
                    });
                });

            }));

        });
    }
 
    public void JumpToStack(Transform stack, Action done = null)
    {
        StopTilt();
        Vector3 startPos = transform.position;

        Vector3 direction = stack.position - startPos;
        direction.y = 0;
        direction.Normalize();
        Debug.Log("direction" + direction);
        jar.forward = direction;

        text.transform.DOScale(0, 0.1f);
        ScaleToSmall().OnComplete(() =>
        {
            ObjectPoolManager.I.Spawn("JumpFx", jumpFxPos.position);
            jar.DOLocalRotate(transform.eulerAngles - transform.right * 360f, jumpDuration, RotateMode.FastBeyond360)
         .SetEase(Ease.OutQuad);

            StartCoroutine(Jump(transform, stack, jumpHeight, jumpDuration, () =>
            {
                text.transform.DOScale(1, 0.3f);
                ScaleToSmall().OnComplete(() =>
                {
                    done?.Invoke();
                });
            }));

        });
    }

    public Tween Unfrezze(Action actionDone = null)
    {
        ObjectPoolManager.I.Spawn("IceFx", iceEffect.position);
        return ice.DOScale(0, 0.2f).SetEase(Ease.InBack).OnComplete(() =>
        {
            ice.gameObject.SetActive(false);
            actionDone?.Invoke();
        });
       
    }

    // Start cyclic process
    public void Active()
    {
        StopTilt();
        ObjectPoolManager.I.Spawn("ActiveFx", activeFx.position);
        ScaleToBig().OnComplete(() =>
        {
            isReady = true;
            StartTilt();
        });
       
    }
    Sequence tilt;
    void StopTilt()
    {
        tilt.Kill();
        jar.transform.SetParent(transform);
        jar.transform.localEulerAngles = big.transform.localEulerAngles;

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
    void StartTiltAtPoint(  int idx1, int idx2)
    {
        if(step > 10)
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
        tilt.Append(point.DOLocalRotate(startAngle , tiltDuration).SetEase(tiltEase));
        tilt.OnStart(() => {jar.transform.SetParent(point); });
        tilt.OnComplete(() =>
        {
            jar.transform.SetParent(transform);
            DownSizeAngle();
            StartTiltAtPoint( idx1, idx2);
        });
    }
    public void DownSizeAngle()
    {
        step++;
        tiltDegress = tiltDegress * 0.8f;
        tiltDuration = tiltDuration * 0.8f;

    }
    public IEnumerator Jump(Transform transform, Transform target, float height, float duration, Action actionDone = null)
    {
        Vector3 startPos = transform.position;
        float elapsed = 0f;

        // Tính toán vận tốc ban đầu theo phương Y để đạt độ cao mong muốn
        float gravity = (8f * height) / (duration * duration) * gravityScale;
        float initialVelocityY = (4f * height) / duration * gravityScale;

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

    public IEnumerator Jump(Transform transform, Vector3 target, float height, float duration, Action actionDone = null)
    {
        Vector3 startPos = transform.position;
        float elapsed = 0f;

        // Tính độ chênh lệch Y giữa điểm bắt đầu và điểm đích
        float deltaY = target.y - startPos.y;

        // Tính toán lại các thông số vật lý để đạt được:
        // 1. Độ cao nhảy 'height' so với điểm cao hơn giữa start và target
        // 2. Đảm bảo đáp xuống đúng target.y
        float gravity = (8f * height) / (duration * duration) * gravityScale;
        float initialVelocityY = ((4f * height) / duration + deltaY / duration) * gravityScale;

        while (elapsed < duration)
        {
            float normalizedTime = elapsed / duration;
            float curveValue = speedCurve.Evaluate(normalizedTime);
            elapsed += Time.deltaTime * curveValue;

            if (elapsed >= duration) break; // Tránh vượt quá duration

            float t = elapsed / duration;

            // Di chuyển theo phương ngang (X-Z) tuyến tính
            Vector3 currentPos = Vector3.Lerp(startPos, target, t);

            // Tính toán vị trí Y theo công thức parabol
            float currentTime = elapsed;
            currentPos.y = startPos.y + (initialVelocityY * currentTime) - (0.5f * gravity * currentTime * currentTime);

            transform.position = currentPos;
            yield return null;
        }

        // Đảm bảo vị trí cuối chính xác
        transform.position = target;
        actionDone?.Invoke();
    }
    public IEnumerator JumpWithPerpendicularArc(Transform transform, Vector3 target, float height, float duration, float arcOffset = 1f, Action actionDone = null)
    {
        Vector3 startPos = transform.position;
        float elapsed = 0f;

        // Tính vector hướng di chuyển trên mặt phẳng XZ
        Vector3 direction = (target - startPos);
        direction.y = 0; // Bỏ qua trục Y

        // Tính vector vuông góc (perpendicular) với hướng di chuyển
        Vector3 perpendicular = Vector3.zero;
        if (direction.magnitude > 0.01f)
        {
            direction.Normalize();
            perpendicular = new Vector3(-direction.z, 0, direction.x); // Xoay 90 độ trên mặt phẳng XZ

            // Random lệch trái hoặc phải
            perpendicular *= (UnityEngine.Random.value > 0.5f ? 1f : -1f);
        }
        else
        {
            // Nếu đi gần như thẳng đứng, lệch theo trục X
            perpendicular = Vector3.right * (UnityEngine.Random.value > 0.5f ? 1f : -1f);
        }

        // Tính độ chênh lệch Y
        float deltaY = target.y - startPos.y;

        float gravity = (8f * height) / (duration * duration) * gravityScale;
        float initialVelocityY = ((4f * height) / duration + deltaY / duration) * gravityScale;

        while (elapsed < duration)
        {
            float normalizedTime = elapsed / duration;
            float curveValue = speedCurve.Evaluate(normalizedTime);
            elapsed += Time.deltaTime * curveValue;

            if (elapsed >= duration) break;

            float t = elapsed / duration;

            Vector3 currentPos = Vector3.Lerp(startPos, target, t);

            float currentTime = elapsed;
            currentPos.y = startPos.y + (initialVelocityY * currentTime) - (0.5f * gravity * currentTime * currentTime);

            // Thêm offset vòng cung theo hướng vuông góc
            float arcCurve = Mathf.Sin(t * Mathf.PI);
            currentPos += perpendicular * arcOffset * arcCurve;

            transform.position = currentPos;
            yield return null;
        }

        transform.position = target;
        actionDone?.Invoke();
    }


    // Hàm kiểm tra hướng lệch
    private float GetArcDirection(Transform transform, Vector3 target)
    {
        float deltaX = target.x - transform.position.x;
        float distance = Vector3.Distance(transform.position, target);
        // Trả về 1 nếu lệch phải, -1 nếu lệch trái
        // Nếu deltaX gần bằng 0, có thể random hoặc mặc định
        if (Mathf.Abs(deltaX* distance) < 0.1f)
        {
            // Nếu đi gần như thẳng, random lệch trái hoặc phải
            return 0.75f;
        }

        return -Mathf.Sign(deltaX* distance);
    }

    // Hàm Jump với hiệu ứng vòng cung ngang
    public IEnumerator Jump(Transform transform, Vector3 target, float height, float duration, float arcOffset = 1f, Action actionDone = null)
    {
        Vector3 startPos = transform.position;
        float elapsed = 0f;

        // Tính hướng lệch (trái hoặc phải)
        float arcDirection = GetArcDirection(transform, target);

        // Tính độ chênh lệch Y giữa điểm bắt đầu và điểm đích
        float deltaY = target.y - startPos.y;

        // Tính toán lại các thông số vật lý
        float gravity = (8f * height) / (duration * duration) * gravityScale;
        float initialVelocityY = ((4f * height) / duration + deltaY / duration) * gravityScale;

        while (elapsed < duration)
        {
            float normalizedTime = elapsed / duration;
            float curveValue = speedCurve.Evaluate(normalizedTime);
            elapsed += Time.deltaTime * curveValue;

            if (elapsed >= duration) break;

            float t = elapsed / duration;

            // Di chuyển theo phương ngang (X-Z) tuyến tính
            Vector3 currentPos = Vector3.Lerp(startPos, target, t);

            // Tính toán vị trí Y theo công thức parabol
            float currentTime = elapsed;
            currentPos.y = startPos.y + (initialVelocityY * currentTime) - (0.5f * gravity * currentTime * currentTime);

            // Thêm offset vòng cung theo trục X (dùng Sin để tạo đường cong mượt)
            // Sin(t * PI) tạo đường cong từ 0 -> 1 -> 0
            float arcCurve = Mathf.Sin(t * Mathf.PI);
            //float arcCurve = t * (2f - t);
            //float sinValue = Mathf.Sin(t * Mathf.PI);
            //float arcCurve = Mathf.Pow(sinValue, 0.6f); 
                currentPos.x += arcDirection * arcOffset * arcCurve;

            transform.position = currentPos;
            yield return null;
        }

        // Đảm bảo vị trí cuối chính xác
        transform.position = target;
        actionDone?.Invoke();
    }

}

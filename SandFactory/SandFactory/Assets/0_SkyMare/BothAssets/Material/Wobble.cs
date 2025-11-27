using DG.Tweening;
using System;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI;

[RequireComponent(typeof(Renderer))]
public class Wobble : MonoBehaviour
{
    [Header("Wobble Settings")]
    public float MaxWobble = 0.03f;
    public float WobbleSpeed = 1f;
    public float Recovery = 1f;

    private Renderer rend;
    private Vector3 lastPos;
    private Vector3 lastRot;
    private Vector3 velocity;
    private Vector3 angularVelocity;

    private float wobbleAmountX;
    private float wobbleAmountZ;
    private float wobbleToAddX;
    private float wobbleToAddZ;
    private float time;
    public float wobberCutterZ;
    bool cutted = false;

    private float highestFill = 0.35f;
    public float lowestFill = -0.4f;
    public float startPourWaterAngle = -52;
    public float endPourWaterAngle = -95;

    public HolderColorManager colorManager;

    public bool isEmptyLiquid = true;
    public void Pour()
    {
        float currentAngle = transform.eulerAngles.z;
        if (currentAngle > 180) currentAngle -= 360;
        float progress = Mathf.InverseLerp(startPourWaterAngle, endPourWaterAngle, currentAngle);
        //if(progress > 0 && !colorManager.cloneFX.gameObject.activeSelf) colorManager.cloneFX.gameObject.SetActive(true);


            float fillLevel = Mathf.Lerp(highestFill, lowestFill, progress);

        rend.material.SetFloat("_Fill", fillLevel);
    }
    public void CutWobble()
    {
        //cutted = true;
        //MaxWobble = 0.003f;
        //WobbleSpeed = 0.05f;
        //Recovery = 10f;
    }
    void Start()
    {
        rend = GetComponent<Renderer>();
        lastPos = transform.position;
        lastRot = transform.rotation.eulerAngles;
    }

    public void SetEmpty()
    {
        rend = GetComponent<Renderer>();
        rend.material.SetFloat("_Fill", -0.6f);
    }

    public void Fill(float duration,float process )
    {
        Debug.Log($"Start Fill {process}");
        isEmptyLiquid = false;

        float valueA = rend.material.GetFloat("_Fill");

        float value = highestFill - lowestFill;
        float targetA = lowestFill  + value * process;


        //DOVirtual.DelayedCall(duration * 0.75f, () =>
        //{
        //    wobbleToAddX = 0.25f;
        //    wobbleToAddZ = 0.25f;
        //});

        Sequence seq = DOTween.Sequence();
        seq.Append(DOTween.To(() => valueA, x => valueA = x, targetA, duration)
          .OnUpdate(() => rend.material.SetFloat("_Fill", valueA))
          .SetEase(Ease.Linear));
      ;
    }
   
    void Update()
    {

        if (isEmptyLiquid) return;
        //if (cutted)
        //{
        //    rend.material.SetFloat("_WobbleZ", wobberCutterZ);
        //    Pour();
        //}
        time += Time.deltaTime;

        // Giảm dần độ rung theo thời gian
        wobbleToAddX = Mathf.Lerp(wobbleToAddX, 0, Time.deltaTime * Recovery);
        wobbleToAddZ = Mathf.Lerp(wobbleToAddZ, 0, Time.deltaTime * Recovery);

        // Tạo sóng sin cho hiệu ứng rung
        float pulse = 2f * Mathf.PI * WobbleSpeed;
        wobbleAmountX = wobbleToAddX * Mathf.Sin(pulse * time);
        wobbleAmountZ = wobbleToAddZ * Mathf.Sin(pulse * time);

        // Gửi giá trị vào shader
        rend.material.SetFloat("_WobbleX", wobbleAmountX);
        if (!cutted)
        {
            rend.material.SetFloat("_WobbleZ", wobbleAmountZ);
        }
       

        // Tính vận tốc tuyến tính và góc
        velocity = (transform.position - lastPos) / Time.deltaTime;
        angularVelocity = transform.rotation.eulerAngles - lastRot;

        // Cộng thêm giá trị rung dựa vào vận tốc
        wobbleToAddX += Mathf.Clamp((velocity.x + angularVelocity.z * 0.2f) * MaxWobble, -MaxWobble, MaxWobble);
        wobbleToAddZ += Mathf.Clamp((velocity.z + angularVelocity.x * 0.2f) * MaxWobble, -MaxWobble, MaxWobble);

        // Lưu lại trạng thái cuối
        lastPos = transform.position;
        lastRot = transform.rotation.eulerAngles;
    }
}

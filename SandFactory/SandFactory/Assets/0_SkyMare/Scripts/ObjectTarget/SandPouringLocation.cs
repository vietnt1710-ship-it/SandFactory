using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SandPouringLocation : MonoBehaviour
{
    private SpriteRenderer targetSprite;   // Sprite nền (World Space) 

    [Header("Process từ 0 → 1 theo trục X")]
    [Range(0f, 1f)]
    public float process = 0.5f; // 0: mép trái, 1: mép phải
    public float xOffset = -0.9f;

    [Header("Y")]
    public float yOffset = 0.8f;         // cộng thêm nếu muốn nhô lên 1 chút

    [Header("Z")]
    public float zOffset = 0f;

    private void Start()
    {
        targetSprite = GetComponent<SpriteRenderer>();
    }


    /// <summary>
    /// Cập nhật vị trí follower dựa trên process (0→1) theo trục X,
    /// và (tuỳ chọn) khoá Y = mép trên cùng của Sprite.
    /// </summary>
    public Vector3 SandPouringPos()
    {
        Bounds b = targetSprite.bounds;

        // Nội suy theo X từ trái → phải
        float newX = Mathf.Lerp(b.min.x, b.max.x, process) + xOffset;

        // Y = mép trên sprite (có offset)
        float newY = b.max.y + yOffset;

        // Góc xoay theo trục X
        float rotationX = 52f;

        // Khoảng cách Y từ tâm sprite
        float distanceFromCenterY = newY - transform.position.y;

        // Tính Z dựa trên rotation X
        // Khi xoay quanh trục X, Y và Z có quan hệ: Z = Y * sin(rotX)
        float newZ = transform.position.z + distanceFromCenterY * Mathf.Sin(rotationX * Mathf.Deg2Rad) + zOffset;

        Vector3 pos = new Vector3(newX, newY, newZ);
        return pos;
    }
    public void CalculateProcessFromPosition(Vector3 pos)
    {
        if(targetSprite == null)
            targetSprite = GetComponent<SpriteRenderer>();


        Bounds b = targetSprite.bounds;

        // Loại bỏ offset từ vị trí
        float xWithoutOffset = pos.x;// - xOffset;

        // Giải ngược công thức Lerp:
        // newX = Mathf.Lerp(b.min.x, b.max.x, process)
        // => newX = b.min.x + (b.max.x - b.min.x) * process
        // => process = (newX - b.min.x) / (b.max.x - b.min.x)

        float calculatedProcess = (xWithoutOffset - b.min.x) / (b.max.x - b.min.x);

        // Clamp về khoảng [0, 1] để đảm bảo hợp lệ
        process = Mathf.Clamp01(calculatedProcess);
    }

}

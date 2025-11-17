using System;
using UnityEngine;

public class ParticleManager : Pooled
{
    [Serializable]
    public enum CompleteAction
    {
        Destroy,      // Hủy object
        Deactivate,   // Ẩn object (SetActive false)
        DoNothing     // Không làm gì
    }

    [Header("Settings")]
    [Tooltip("Tự động xử lý khi particle hoàn thành")]
    public bool autoHandle = true;

    [Tooltip("Hành động khi particle hoàn thành")]
    public CompleteAction onComplete = CompleteAction.Destroy;

    [Tooltip("Thời gian delay trước khi thực hiện hành động (seconds)")]
    public float delayTime = 0f;

    private ParticleSystem ps;
    private bool isCompleted = false;

    void Awake()
    {
        ps = GetComponent<ParticleSystem>();

        if (ps == null)
        {
            Debug.LogError("ParticleManager: Không tìm thấy ParticleSystem component!");
            enabled = false;
            return;
        }
    }

    void Update()
    {
        if (!autoHandle || isCompleted) return;

        // Kiểm tra nếu particle đã phát xong
        if (!ps.IsAlive())
        {
            isCompleted = true;

            if (delayTime > 0f)
            {
                Invoke(nameof(HandleComplete), delayTime);
            }
            else
            {
                HandleComplete();
            }
        }
    }

    void HandleComplete()
    {
        switch (onComplete)
        {
            case CompleteAction.Destroy:
                Destroy(gameObject);
                break;

            case CompleteAction.Deactivate:
                ObjectPoolManager.I.Despawn(poolName, gameObject);
                break;

            case CompleteAction.DoNothing:
                // Không làm gì, có thể xử lý bằng event hoặc code khác
                break;
        }
    }

    // Phương thức công khai để play particle
    public void PlayParticle()
    {
        if (ps != null)
        {
            gameObject.SetActive(true);
            isCompleted = false;
            ps.Play();
        }
    }

    // Dừng particle ngay lập tức
    public void StopParticle(bool clearParticles = true)
    {
        if (ps != null)
        {
            if (clearParticles)
                ps.Clear();
            ps.Stop();
        }
    }

    // Restart particle
    public void RestartParticle()
    {
        if (ps != null)
        {
            ps.Clear();
            ps.Play();
            isCompleted = false;
        }
    }

    // Thay đổi hành động khi hoàn thành từ code
    public void SetCompleteAction(CompleteAction action)
    {
        onComplete = action;
    }
}
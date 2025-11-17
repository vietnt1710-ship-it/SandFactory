using System;
using UnityEditor;
using UnityEngine;

public class ETC_HeartUnlimited : ETC
{
    public override void UpdateValue(int value)
    {
        time.AddHours(value);
    }

    public void Update()
    {
        if (time == null) return;
        if (!time.IsActive()) return;

        txt_Value.text = time.GetRemainingTimeFormatted();

    }
    public UnlimitedItemManager time;
    public override void ActionStart()
    {
        time = new UnlimitedItemManager();
        time.LoadData();

#if UNITY_EDITOR
        EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
#endif
    }
#if UNITY_EDITOR
    private void OnPlayModeStateChanged(PlayModeStateChange state)
    {
        if (state == PlayModeStateChange.ExitingPlayMode)
        {
            Debug.Log("Sắp thoát Play Mode!");
        }
        else if (state == PlayModeStateChange.EnteredEditMode)
        {
            time.SaveData();
        }
    }
#endif
    private void OnApplicationQuit()
    {
        time.SaveData();
    }
}
public class UnlimitedItemManager
{
    private const string saveKey = "UnlimitedItemEndTime";
    private DateTime endTime;
    public Action OnEndUnlimitedTime;

    // Thêm số giờ sử dụng
    public void AddHours(float hours)
    {
        DateTime currentTime = DateTime.Now;

        // Nếu còn thời gian, cộng thêm vào thời gian hiện tại
        if (GetRemainingSeconds() > 0)
        {
            endTime = endTime.AddHours(hours);
        }
        else
        {
            // Nếu hết thời gian, tính từ thời điểm hiện tại
            endTime = currentTime.AddHours(hours);
        }

        SaveData();
    }

    // Lấy số giây còn lại
    public double GetRemainingSeconds()
    {
        DateTime currentTime = DateTime.Now;
        TimeSpan remaining = endTime - currentTime;

        return remaining.TotalSeconds > 0 ? remaining.TotalSeconds : 0;
    }

    // Kiểm tra còn thời gian không
    bool isActive = false;
    public bool IsActive()
    {
        if( GetRemainingSeconds() <= 0 && isActive)
        {
            OnEndUnlimitedTime?.Invoke();
        }
        isActive = GetRemainingSeconds() > 0;
        Debug.Log($"Time get {isActive}");
        return isActive;
    }

    // Lấy thời gian còn lại dạng string (h:m:s)
    public string GetRemainingTimeFormatted()
    {
        double seconds = GetRemainingSeconds();

        if (seconds <= 0)
        {
         
            return "0:00:00";
        }
           

        TimeSpan timeSpan = TimeSpan.FromSeconds(seconds);
        return string.Format("{0}:{1:D2}:{2:D2}",
            (int)timeSpan.TotalHours,
            timeSpan.Minutes,
            timeSpan.Seconds);
    }

    // Lưu dữ liệu
    public void SaveData()
    {
        PlayerPrefs.SetString(saveKey, endTime.ToBinary().ToString());
        PlayerPrefs.Save();
    }

    // Load dữ liệu
    public void LoadData()
    {
        if (PlayerPrefs.HasKey(saveKey))
        {
            long binary = long.Parse(PlayerPrefs.GetString(saveKey));
            endTime = DateTime.FromBinary(binary);
        }
        else
        {
            endTime = DateTime.Now;
        }
    }

    // Reset thời gian (xóa item)
    public void ResetTime()
    {
        endTime = DateTime.Now;
        SaveData();
    }

    // Xóa toàn bộ dữ liệu
    public void ClearData()
    {
        PlayerPrefs.DeleteKey(saveKey);
        endTime = DateTime.Now;
    }
}
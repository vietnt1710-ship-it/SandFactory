using Data;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using ToolLevel;
using UnityEditor;
using UnityEngine;

public class LevelDataLoader : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TMP_Dropdown levelDropdown;

    [Header("Level Data Settings")]
    [SerializeField] private string levelDataFolderPath = "Assets/0_SkyMare/Data/New Data";

    [Header("Current Data")]
    public List<string> levelDataNames = new List<string>();
    public string selectedLevelName;
    public SandLevelData selectedLevelData;

    private Dictionary<string, SandLevelData> levelDataDictionary = new Dictionary<string, SandLevelData>();

    void Awake()
    {
        LoadLevelDataList();
        SetupDropdown();
    }

    // Load danh sách tất cả SandLevelData ScriptableObjects
    void LoadLevelDataList()
    {
        levelDataNames.Clear();
        levelDataDictionary.Clear();

#if UNITY_EDITOR
        // Tìm tất cả SandLevelData assets trong thư mục
        string[] guids = AssetDatabase.FindAssets("t:SandLevelData", new[] { levelDataFolderPath });

        foreach (string guid in guids)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(guid);
            SandLevelData levelData = AssetDatabase.LoadAssetAtPath<SandLevelData>(assetPath);

            if (levelData != null)
            {
                string levelName = levelData.name; // Lấy tên asset (ví dụ: LEVEL_1)
                levelDataNames.Add(levelName);
                levelDataDictionary.Add(levelName, levelData);
            }
        }

        // Sắp xếp theo tên (tùy chọn)
        levelDataNames.Sort();

        Debug.Log($"Tìm thấy {levelDataNames.Count} Level Data");
#endif
    }

    // Setup dropdown với danh sách Level Data
    void SetupDropdown()
    {
        if (levelDropdown == null)
        {
            Debug.LogError("Chưa gán Dropdown!");
            return;
        }

        levelDropdown.ClearOptions();
        levelDropdown.AddOptions(levelDataNames);

        // Lắng nghe sự kiện thay đổi
        levelDropdown.onValueChanged.AddListener(OnLevelDataSelected);

        // Tự động chọn level đầu tiên nếu có
        if (levelDataNames.Count > 0)
        {
            OnLevelDataSelected(0);
        }
    }

    // Khi chọn Level Data từ dropdown
    public void OnLevelDataSelected(int index)
    {
        if (index < 0 || index >= levelDataNames.Count) return;

        selectedLevelName = levelDataNames[index];

        if (levelDataDictionary.TryGetValue(selectedLevelName, out SandLevelData levelData))
        {
            selectedLevelData = levelData;
            Debug.Log($"Đã chọn Level: {selectedLevelName}");

            // Gọi hàm xử lý load level ở đây
            // ToolManager.I.LoadLevel(selectedLevelData);
        }
    }

    // Refresh lại danh sách (gọi khi có thay đổi trong thư mục)
    public void RefreshLevelDataList()
    {
        LoadLevelDataList();
        SetupDropdown();
    }
}
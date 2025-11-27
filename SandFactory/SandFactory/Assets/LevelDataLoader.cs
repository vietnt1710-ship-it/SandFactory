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
        SetupDropdown(true);
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
                string levelName = levelData.name;
                levelDataNames.Add(levelName);
                levelDataDictionary.Add(levelName, levelData);
            }
        }

        // Sắp xếp theo số level thực sự (LEVEL_1, LEVEL_2, ..., LEVEL_10, LEVEL_11)
        levelDataNames = levelDataNames
            .OrderBy(name => ExtractLevelNumber(name))
            .ToList();

        Debug.Log($"Tìm thấy {levelDataNames.Count} Level Data");
#endif
    }

    // Trích xuất số level từ tên (VD: "LEVEL_10" -> 10)
    int ExtractLevelNumber(string levelName)
    {
        // Tìm vị trí của dấu gạch dưới cuối cùng
        int lastUnderscoreIndex = levelName.LastIndexOf('_');

        if (lastUnderscoreIndex >= 0 && lastUnderscoreIndex < levelName.Length - 1)
        {
            string numberPart = levelName.Substring(lastUnderscoreIndex + 1);

            if (int.TryParse(numberPart, out int levelNumber))
            {
                return levelNumber;
            }
        }

        // Nếu không parse được, trả về 0 (sẽ đứng đầu)
        return 0;
    }

    // Setup dropdown với danh sách Level Data
    void SetupDropdown(bool first = false)
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

        if (first)
        {
            if (levelDataNames.Count > 0)
            {
                OnLevelDataSelected(0);
            }
        }
        // Tự động chọn level đầu tiên nếu có
      
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
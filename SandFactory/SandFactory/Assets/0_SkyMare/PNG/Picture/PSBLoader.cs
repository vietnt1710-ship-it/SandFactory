#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using System;
using Object = UnityEngine.Object;

namespace ToolLevel
{
    [Serializable]
    public class SpriteInfor
    {
        public string id;
        public Sprite sprite;
        public Sprite sand;
        public Vector2 localPosition = new Vector2(-100, -100);

        public bool IsFetched()
        {
            return sand! != null && sprite != null && localPosition.x != -100 && localPosition.y != -100;
        }
    }

    public class PSBLoader : MonoBehaviour
    {
        
        [Header("UI References")]
        [SerializeField] private TMP_Dropdown psbDropdown; // Hoặc dùng Dropdown nếu không có TextMeshPro

        [Header("PSB Settings")]
        [SerializeField] private string psbFolderPath = "Assets/0_SkyMare/PNG/Picture";

        [Header("Current Data")]
        public List<string> psbFileNames = new List<string>();
        public List<SpriteInfor> currentSprites = new List<SpriteInfor> { new SpriteInfor() };
        public string selectedPSBName;

        private Dictionary<string, string> psbPathDictionary = new Dictionary<string, string>();

        void Start()
        {
            LoadPSBList();
            SetupDropdown();
        }

        // Load danh sách tất cả PSB files
        void LoadPSBList()
        {
            psbFileNames.Clear();
            psbPathDictionary.Clear();

            // Tìm tất cả file .psb trong thư mục
            string[] allFiles = System.IO.Directory.GetFiles(psbFolderPath, "*.psb");

            foreach (string filePath in allFiles)
            {
                string fileName = System.IO.Path.GetFileNameWithoutExtension(filePath);
                string assetPath = filePath.Replace("\\", "/");

                psbFileNames.Add(fileName);
                psbPathDictionary.Add(fileName, assetPath);
            }

            Debug.Log($"Tìm thấy {psbFileNames.Count} file PSB");
        }

        // Setup dropdown với danh sách PSB
        void SetupDropdown()
        {
            if (psbDropdown == null)
            {
                Debug.LogError("Chưa gán Dropdown!");
                return;
            }

            psbDropdown.ClearOptions();
            psbDropdown.AddOptions(psbFileNames);

            // Lắng nghe sự kiện thay đổi
            psbDropdown.onValueChanged.AddListener(OnPSBSelected);

            // Load PSB đầu tiên
            //if (psbFileNames.Count > 0)
            //{
            //    OnPSBSelected(0);
            //}
        }

        // Khi chọn PSB từ dropdown
        public void OnPSBSelected(int index)
        {
#if UNITY_EDITOR
            if (index < 0 || index >= psbFileNames.Count) return;

            selectedPSBName = psbFileNames[index];
            LoadSpritesFromPSB(selectedPSBName);

            //ToolManager.I.LoadNewPicture(currentSprites, selectedPSBName);
#endif
        }

        // Load tất cả sprites từ PSB đã chọn
        void LoadSpritesFromPSB(string psbName)
        {
#if UNITY_EDITOR
            currentSprites = new List<SpriteInfor>(1);

            if (!psbPathDictionary.ContainsKey(psbName))
            {
                Debug.LogError($"Không tìm thấy PSB: {psbName}");
                return;
            }

            string path = psbPathDictionary[psbName];
            Object[] objects = AssetDatabase.LoadAllAssetsAtPath(path);

            // 1) Lưu tất cả GameObject positions theo tên (nếu có)
            //var positions = new Dictionary<string, Vector2>();
            //var spriteList = new List<Sprite>();

            foreach (Object obj in objects)
            {
                if (obj.name == psbName) continue;

                string name = "";
                SpriteInfor spi = null;

                if (obj is GameObject go)
                {
                    name = go.name.Replace("_base", "");
                    spi = currentSprites.FirstOrDefault(s => s.id == name);

                    if (spi == null)
                    {
                        spi = new SpriteInfor { id = name };
                        currentSprites.Add(spi);
                    }

                    spi.localPosition = go.transform.localPosition;
                }
                else if (obj is Sprite sp)
                {
                    if (sp.name.Contains("base") || sp.name == "Line")
                    {
                        name = sp.name.Replace("_base", "");
                        spi = currentSprites.FirstOrDefault(s => s.id == name);

                        if (spi == null)
                        {
                            spi = new SpriteInfor { id = name };
                            currentSprites.Add(spi);
                        }

                        spi.sprite = sp;
                    }
                    else
                    {
                        name = sp.name;
                        spi = currentSprites.FirstOrDefault(s => s.id == name);

                        if (spi == null)
                        {
                            spi = new SpriteInfor { id = name };
                            currentSprites.Add(spi);
                        }

                        spi.sand = sp;
                    }
                }
            }


            Debug.Log($"Loaded {currentSprites.Count} sprites từ {psbName}");
#endif
        }


        public void RefreshPSBList()
        {
            LoadPSBList();
            SetupDropdown();
        }
    }

}

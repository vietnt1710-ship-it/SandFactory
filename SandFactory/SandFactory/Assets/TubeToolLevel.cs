using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using UnityEngine;
using UnityEngine.UI;

namespace ToolLevel
{
    public class TubeToolLevel : MonoBehaviour
    {
        public List<Image> tubeItems;
        public Sprite hiddenSprite;
        public Button Btn_gen;

        public StorgeManager storgeManager;
        public ColorID colorIDs;
        public Button btn_RandomTube;

        private void Start()
        {
            Btn_gen.onClick.AddListener(Gen);
            btn_RandomTube.onClick.AddListener(RandomTube);
        }


        [Range(0f, 7f)]
        public int hardLevel;

        public int numberOfColorsInLevel = 4; // Số màu có ID <= giá trị này sẽ được sử dụng

        public void RandomTube()
        {
            // Reset tất cả tube items
            for (int i = 0; i < tubeItems.Count; i++)
            {
                tubeItems[i].sprite = null;
                tubeItems[i].name = "None";
                tubeItems[i].color = new Color(55f / 255f, 55f / 255f, 55f / 255f);
            }

            // Lọc các màu có ID <= numberOfColorsInLevel
            List<ColorWithID> availableColors = new List<ColorWithID>();
            foreach (var color in colorIDs.colorWithIDs)
            {
                if (color.ID <= numberOfColorsInLevel)
                {
                    availableColors.Add(color);
                }
            }

            // Nếu không có màu nào phù hợp, return
            if (availableColors.Count == 0)
            {
                Debug.LogWarning("Không có màu nào có ID <= " + numberOfColorsInLevel);
                return;
            }

            // Tính số màu sẽ thực sự được sử dụng dựa vào hardLevel
            int actualNumberOfColors;

            if (hardLevel <= 1)
            {
                actualNumberOfColors = Mathf.Min(UnityEngine.Random.Range(2, 4), availableColors.Count);
            }
            else if (hardLevel <= 3)
            {
                actualNumberOfColors = Mathf.Min(UnityEngine.Random.Range(3, 5), availableColors.Count);
            }
            else if (hardLevel <= 5)
            {
                actualNumberOfColors = Mathf.Min(UnityEngine.Random.Range(4, 7), availableColors.Count);
            }
            else
            {
                actualNumberOfColors = Mathf.Min(UnityEngine.Random.Range(6, availableColors.Count + 1), availableColors.Count);
            }

            actualNumberOfColors = Mathf.Max(2, actualNumberOfColors);

            // Chọn ngẫu nhiên các màu từ availableColors
            List<ColorWithID> selectedColors = new List<ColorWithID>();
            List<ColorWithID> tempAvailable = new List<ColorWithID>(availableColors);

            for (int i = 0; i < actualNumberOfColors && tempAvailable.Count > 0; i++)
            {
                int randomIndex = UnityEngine.Random.Range(0, tempAvailable.Count);
                selectedColors.Add(tempAvailable[randomIndex]);
                tempAvailable.RemoveAt(randomIndex);
            }

            // Xác định số lần lặp cho mỗi màu dựa vào hardLevel
            int repeatsPerColor;

            if (hardLevel <= 2)
            {
                int[] options = { 12, 15, 18 };
                repeatsPerColor = options[UnityEngine.Random.Range(0, options.Length)];
            }
            else if (hardLevel <= 4)
            {
                int[] options = { 9, 12 };
                repeatsPerColor = options[UnityEngine.Random.Range(0, options.Length)];
            }
            else if (hardLevel <= 6)
            {
                int[] options = { 6, 9 };
                repeatsPerColor = options[UnityEngine.Random.Range(0, options.Length)];
            }
            else
            {
                int[] options = { 3, 6 };
                repeatsPerColor = options[UnityEngine.Random.Range(0, options.Length)];
            }

            // Tạo colorPool - mỗi màu xuất hiện repeatsPerColor lần (chia hết cho 3)
            List<ColorWithID> colorPool = new List<ColorWithID>();
            foreach (var color in selectedColors)
            {
                for (int j = 0; j < repeatsPerColor; j++)
                {
                    colorPool.Add(color);
                }
            }

            // Nếu colorPool nhiều hơn tubeItems, bỏ bớt từng nhóm 3 (loại màu hoàn toàn)
            while (colorPool.Count > tubeItems.Count && selectedColors.Count > 1)
            {
                // Chọn 1 màu ngẫu nhiên để loại bỏ
                int colorIndexToRemove = UnityEngine.Random.Range(0, selectedColors.Count);
                ColorWithID colorToRemove = selectedColors[colorIndexToRemove];

                // Xóa tất cả instances của màu đó khỏi colorPool
                colorPool.RemoveAll(c => c.ID == colorToRemove.ID);

                // Xóa màu khỏi selectedColors
                selectedColors.RemoveAt(colorIndexToRemove);
            }

            // Trộn ngẫu nhiên colorPool
            for (int i = 0; i < colorPool.Count; i++)
            {
                int randomIndex = UnityEngine.Random.Range(i, colorPool.Count);
                var temp = colorPool[i];
                colorPool[i] = colorPool[randomIndex];
                colorPool[randomIndex] = temp;
            }

            // Gán màu cho tubeItems
            int itemsToFill = Mathf.Min(colorPool.Count, tubeItems.Count);
            for (int i = 0; i < itemsToFill; i++)
            {
                tubeItems[i].sprite = null;
                tubeItems[i].name = colorPool[i].ID.ToString();
                tubeItems[i].color = colorPool[i].color;
            }
        }
        public void LoadNewTube(List<int>ids)
        {
            for (int i = 0; i < tubeItems.Count; i++)
            {
                tubeItems[i].sprite = null;
                tubeItems[i].name = "None";
                tubeItems[i].color = new Color(55f / 255f, 55f / 255f, 55f / 255f);
            }
            for (int i = 0; i < ids.Count; i++)
            {
                tubeItems[i].name = ids[i].ToString();
                if (ids[i] < 0)
                {
                    tubeItems[i].sprite = hiddenSprite;
                }
                tubeItems[i].color = colorIDs.colorWithIDs[Mathf.Abs(ids[i]) -1].color;
            }
        }
        public void Clear()
        {
            for (int i = 0; i < tubeItems.Count; i++)
            {
                tubeItems[i].sprite = null;
                tubeItems[i].name = "None";
                tubeItems[i].color = new Color(55f / 255f, 55f / 255f, 55f / 255f);
            }
        }
        public List<int> GetList()
        {
            List<int> list = new List<int>();

            for (int i = 0; i < tubeItems.Count; i++)
            {
                int id = 0;
                if (Int32.TryParse(tubeItems[i].name, out id))
                {
                    list.Add(id);
                }
                else
                {
                    break;
                }
            }
            return list;
        }
        public void Gen()
        {
            Dictionary<int, int > _amounts = new Dictionary<int, int>();
            for (int i = 0; i < tubeItems.Count; i++)
            {
                int id = 0;
                if (Int32.TryParse(tubeItems[i].name, out id))
                {
                    id = Mathf.Abs(id);

                    if (_amounts.ContainsKey(id))
                        _amounts[id] += 1;
                    else
                        _amounts[id] = 1;
                }
                else
                {
                    break; 
                }
            } 

            bool hasError = false;
            List<int> invalidIds = new List<int>();

            // Duyệt qua tất cả các cặp key-value trong dictionary
            foreach (var pair in _amounts)
            {
                if (pair.Value % 3 != 0)
                {
                    // Value không chia hết cho 3 → ghi nhận lỗi
                    hasError = true;
                    invalidIds.Add(pair.Key);
                }
            }

            // Kiểm tra và báo lỗi nếu có
            if (hasError)
            {
                Debug.LogError($"Lỗi: Các ID sau có số lượng không chia hết cho 3: {string.Join(", ", invalidIds)}");
                // Hoặc throw exception nếu cần dừng chương trình
            }
            else
            {
                // Nếu tất cả đều chia hết cho 3 → chia tất cả value cho 3
                var keys = new List<int>(_amounts.Keys);
                foreach (int key in keys)
                {
                    _amounts[key] = _amounts[key] / 3;
                }

                Debug.Log("Đã chuyển đổi thành công tất cả các giá trị!");

                storgeManager.GenData(_amounts);
            }  
        }
    }
}

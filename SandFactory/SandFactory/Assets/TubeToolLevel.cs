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
            List<ColorWithID> selectedColors = new List<ColorWithID>();
            foreach (var color in colorIDs.colorWithIDs)
            {
                if (color.ID <= numberOfColorsInLevel)
                {
                    selectedColors.Add(color);
                }
            }

            // Nếu không có màu nào phù hợp, return
            if (selectedColors.Count == 0)
            {
                Debug.LogWarning("Không có màu nào có ID <= " + numberOfColorsInLevel);
                return;
            }

            // Tính số lần lặp lại cho mỗi màu dựa vào hardLevel
            // Độ khó thấp: mỗi màu lặp lại nhiều lần
            // Độ khó cao: mỗi màu lặp lại ít lần hơn
            int minRepeatsPerColor;
            int maxRepeatsPerColor;

            if (hardLevel <= 2)
            {
                minRepeatsPerColor = 9;   // Level dễ: 9-15 lần
                maxRepeatsPerColor = 15;
            }
            else if (hardLevel <= 4)
            {
                minRepeatsPerColor = 6;   // Level trung bình: 6-12 lần
                maxRepeatsPerColor = 12;
            }
            else if (hardLevel <= 6)
            {
                minRepeatsPerColor = 3;   // Level khó: 3-9 lần
                maxRepeatsPerColor = 9;
            }
            else
            {
                minRepeatsPerColor = 3;   // Level rất khó: 3-6 lần
                maxRepeatsPerColor = 6;
            }

            // Tính toán số lần lặp cho mỗi màu sao cho tổng gần bằng tubeItems.Count
            int targetTotal = tubeItems.Count;
            int[] repeatsForEachColor = new int[selectedColors.Count];

            // Khởi tạo mỗi màu với số lần tối thiểu
            int currentTotal = 0;
            for (int i = 0; i < selectedColors.Count; i++)
            {
                repeatsForEachColor[i] = minRepeatsPerColor;
                currentTotal += minRepeatsPerColor;
            }

            // Thêm dần cho đến khi đạt gần targetTotal (và vẫn chia hết cho 3)
            int colorIndex = 0;
            while (currentTotal < targetTotal)
            {
                // Kiểm tra xem có thể thêm 3 không
                if (currentTotal + 3 <= targetTotal && repeatsForEachColor[colorIndex] + 3 <= maxRepeatsPerColor)
                {
                    repeatsForEachColor[colorIndex] += 3;
                    currentTotal += 3;
                }

                colorIndex = (colorIndex + 1) % selectedColors.Count;

                // Nếu không thể thêm nữa, break
                bool canAddMore = false;
                for (int i = 0; i < selectedColors.Count; i++)
                {
                    if (repeatsForEachColor[i] + 3 <= maxRepeatsPerColor && currentTotal + 3 <= targetTotal)
                    {
                        canAddMore = true;
                        break;
                    }
                }

                if (!canAddMore)
                {
                    break;
                }
            }

            // Tạo colorPool với số lần lặp đã tính
            List<ColorWithID> colorPool = new List<ColorWithID>();
            for (int i = 0; i < selectedColors.Count; i++)
            {
                for (int j = 0; j < repeatsForEachColor[i]; j++)
                {
                    colorPool.Add(selectedColors[i]);
                }
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
            for (int i = 0; i < colorPool.Count && i < tubeItems.Count; i++)
            {
                tubeItems[i].sprite = null;
                tubeItems[i].name = colorPool[i].ID.ToString();
                tubeItems[i].color = colorPool[i].color;
            }

            // Nếu còn thiếu (rất hiếm khi xảy ra), fill bằng màu ngẫu nhiên từ selectedColors
            for (int i = colorPool.Count; i < tubeItems.Count; i++)
            {
                var randomColor = selectedColors[UnityEngine.Random.Range(0, selectedColors.Count)];
                tubeItems[i].sprite = null;
                tubeItems[i].name = randomColor.ID.ToString();
                tubeItems[i].color = randomColor.color;
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

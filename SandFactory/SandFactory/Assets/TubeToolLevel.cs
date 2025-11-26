using DG.Tweening;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace ToolLevel
{
    public class TubeToolLevel : MonoBehaviour
    {
        public ColorToolLevel colorToolLevel;
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

        public int maxColorUse = 0;

        public void RandomTube()
        {
            // Reset tất cả tube items
            for (int i = 0; i < tubeItems.Count; i++)
            {
                tubeItems[i].sprite = null;
                tubeItems[i].name = "None";
                tubeItems[i].color = new Color(55f / 255f, 55f / 255f, 55f / 255f);
            }

            // TÍNH TOÁN SỐ MÀU DỰA VÀO ĐỘ KHÓ - TĂNG SỐ LƯỢNG MÀU
            // Level 0: 2-3 màu, Level 7: 6-8 màu (hoặc tối đa có thể)
            int minColors = Mathf.Clamp(2 + Mathf.FloorToInt(hardLevel * 0.7f), 2, colorIDs.colorWithIDs.Count);
            int maxColors = Mathf.Clamp(5 + Mathf.FloorToInt(hardLevel * 1.4f), 5, colorIDs.colorWithIDs.Count);

            // Đảm bảo không vượt quá số màu có sẵn
            maxColors = Mathf.Min(maxColors, colorIDs.colorWithIDs.Count);

            int numberOfColors = UnityEngine.Random.Range(minColors, maxColors + 1);

            // Chọn ngẫu nhiên các màu sẽ được sử dụng
            List<ColorWithID> selectedColors = new List<ColorWithID>();
            List<ColorWithID> availableColors = new List<ColorWithID>(colorIDs.colorWithIDs);

            for (int i = 0; i < numberOfColors && availableColors.Count > 0; i++)
            {
                int randomIndex = UnityEngine.Random.Range(0, availableColors.Count);
                selectedColors.Add(availableColors[randomIndex]);
                availableColors.RemoveAt(randomIndex);
            }

            // Tính toán để sử dụng tối đa số tubeItems có thể
            int maxUsableSlots = tubeItems.Count;

            // Tính độ chênh lệch dựa trên hardLevel (giảm chênh lệch)
            float disparity = 0.7f - (hardLevel / 10f); // Giảm từ 1.0 xuống 0.7

            // Tạo danh sách số lượng cho mỗi màu với độ chênh lệch vừa phải
            Dictionary<ColorWithID, int> colorCounts = new Dictionary<ColorWithID, int>();
            int totalColorCount = 0;

            // Phân bổ số lượng cơ bản cho mỗi màu (ít nhất 3)
            foreach (var color in selectedColors)
            {
                colorCounts[color] = 3;
                totalColorCount += 3;
            }

            // Tính số slot còn lại có thể phân bổ
            int remainingSlots = maxUsableSlots - totalColorCount;

            // Phân bổ các slot còn lại với độ chênh lệch vừa phải
            if (remainingSlots >= 3)
            {
                int chunksToDistribute = remainingSlots / 3; // Số nhóm 3 có thể phân bổ

                for (int i = 0; i < chunksToDistribute; i++)
                {
                    // Tạo xác suất chênh lệch vừa phải
                    float randomValue = UnityEngine.Random.value;

                    // Giảm chênh lệch: chỉ ưu tiên màu có nhiều khi randomValue thấp
                    if (randomValue < disparity * 0.6f) // Giảm hệ số từ 1.0 xuống 0.6
                    {
                        // Chọn màu có số lượng lớn nhất
                        var maxColor = colorCounts.Aggregate((x, y) => x.Value > y.Value ? x : y).Key;
                        colorCounts[maxColor] += 3;
                        totalColorCount += 3;
                    }
                    else // Phân bổ ngẫu nhiên
                    {
                        // Chọn ngẫu nhiên một màu
                        var randomColor = selectedColors[UnityEngine.Random.Range(0, selectedColors.Count)];
                        colorCounts[randomColor] += 3;
                        totalColorCount += 3;
                    }
                }
            }

            // Tạo danh sách màu đã được phân bổ
            List<ColorWithID> colorPool = new List<ColorWithID>();
            foreach (var kvp in colorCounts)
            {
                for (int j = 0; j < kvp.Value; j++)
                {
                    colorPool.Add(kvp.Key);
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

            // Hạn chế 3 màu giống liên tiếp
            colorPool = AvoidThreeConsecutiveColors(colorPool);

            // Gán màu cho tubeItems - fill càng nhiều càng tốt
            for (int i = 0; i < tubeItems.Count; i++)
            {
                if (i < colorPool.Count)
                {
                    tubeItems[i].sprite = null;
                    tubeItems[i].name = colorPool[i].ID.ToString();
                    tubeItems[i].color = colorPool[i].color;
                }
                else
                {
                    // Phần còn lại giữ nguyên (màu xám - trống)
                    tubeItems[i].sprite = null;
                    tubeItems[i].name = "None";
                    tubeItems[i].color = new Color(55f / 255f, 55f / 255f, 55f / 255f);
                }
            }

            // Debug log để kiểm tra số lượng màu
            Debug.Log($"Hard Level: {hardLevel}, Number of Colors: {numberOfColors}, Min: {minColors}, Max: {maxColors}");
        }

        // Phương thức để tránh 3 màu giống liên tiếp
        private List<ColorWithID> AvoidThreeConsecutiveColors(List<ColorWithID> colorPool)
        {
            if (colorPool.Count < 3) return colorPool;

            // Tạo bản sao để không ảnh hưởng đến list gốc
            List<ColorWithID> result = new List<ColorWithID>(colorPool);

            int maxAttempts = result.Count * 2; // Giới hạn số lần thử
            int attempts = 0;

            // Kiểm tra và sửa cho đến khi không còn 3 màu giống liên tiếp
            bool hasThreeConsecutive;
            do
            {
                hasThreeConsecutive = false;

                for (int i = 2; i < result.Count; i++)
                {
                    if (result[i].ID == result[i - 1].ID && result[i].ID == result[i - 2].ID)
                    {
                        hasThreeConsecutive = true;

                        // Tìm vị trí thích hợp để hoán đổi (cách ít nhất 2 vị trí)
                        int swapIndex = -1;
                        for (int j = 0; j < result.Count; j++)
                        {
                            // Đảm bảo không tạo ra chuỗi mới khi hoán đổi
                            if (Math.Abs(j - i) > 2 &&
                                result[j].ID != result[i].ID &&
                                (j == 0 || result[j - 1].ID != result[i].ID) &&
                                (j == result.Count - 1 || result[j + 1].ID != result[i].ID))
                            {
                                swapIndex = j;
                                break;
                            }
                        }

                        // Nếu tìm thấy vị trí phù hợp, hoán đổi
                        if (swapIndex != -1)
                        {
                            var temp = result[i];
                            result[i] = result[swapIndex];
                            result[swapIndex] = temp;
                        }

                        break; // Thoát vòng lặp for và kiểm tra lại từ đầu
                    }
                }

                attempts++;
            } while (hasThreeConsecutive && attempts < maxAttempts);

            return result;
        }

        // Phiên bản đơn giản với nhiều màu hơn
        public void RandomTubeSimple()
        {
            // Reset
            for (int i = 0; i < tubeItems.Count; i++)
            {
                tubeItems[i].sprite = null;
                tubeItems[i].name = "None";
                tubeItems[i].color = new Color(55f / 255f, 55f / 255f, 55f / 255f);
            }

            // TĂNG SỐ LƯỢNG MÀU - Level 0: 2 màu, Level 7: 7-8 màu
            int minColors = Mathf.Clamp(2 + Mathf.FloorToInt(hardLevel * 0.8f), 2, colorIDs.colorWithIDs.Count);
            int maxColors = Mathf.Clamp(3 + Mathf.FloorToInt(hardLevel * 1.2f), 3, colorIDs.colorWithIDs.Count);
            maxColors = Mathf.Min(maxColors, colorIDs.colorWithIDs.Count);

            int numberOfColors = UnityEngine.Random.Range(minColors, maxColors + 1);

            // Chọn màu ngẫu nhiên
            List<ColorWithID> selectedColors = new List<ColorWithID>();
            List<ColorWithID> availableColors = new List<ColorWithID>(colorIDs.colorWithIDs);

            for (int i = 0; i < numberOfColors; i++)
            {
                if (availableColors.Count == 0) break;

                int randomIndex = UnityEngine.Random.Range(0, availableColors.Count);
                selectedColors.Add(availableColors[randomIndex]);
                availableColors.RemoveAt(randomIndex);
            }

            // Tính số lượng tối đa có thể fill (vẫn chia hết cho 3)
            int maxFillSlots = (tubeItems.Count / 3) * 3;

            // Phân bổ số lượng cho mỗi màu với chênh lệch nhỏ
            // Với nhiều màu hơn, mỗi màu có ít slot hơn
            int baseSlotsPerColor = Mathf.Max(3, (maxFillSlots / numberOfColors / 3) * 3);

            // Tạo color pool với chênh lệch nhỏ
            List<ColorWithID> colorPool = new List<ColorWithID>();
            foreach (var color in selectedColors)
            {
                // Thêm biến thể nhỏ cho số lượng mỗi màu (±0 hoặc +3)
                int variation = UnityEngine.Random.Range(0, 2) * 3; // 0 hoặc 3
                int slotsForThisColor = baseSlotsPerColor + variation;
                slotsForThisColor = Mathf.Max(3, slotsForThisColor);

                for (int j = 0; j < slotsForThisColor; j++)
                {
                    colorPool.Add(color);
                }
            }

            // Điều chỉnh tổng số lượng
            while (colorPool.Count > maxFillSlots && colorPool.Count > 3)
            {
                // Xóa 3 phần tử của màu có nhiều nhất
                var colorGroups = colorPool.GroupBy(c => c.ID)
                                         .OrderByDescending(g => g.Count())
                                         .ToList();

                if (colorGroups.Count > 0 && colorGroups[0].Count() > 3)
                {
                    // Xóa 3 phần tử của màu có nhiều nhất
                    int removed = 0;
                    for (int i = colorPool.Count - 1; i >= 0 && removed < 3; i--)
                    {
                        if (colorPool[i].ID == colorGroups[0].Key)
                        {
                            colorPool.RemoveAt(i);
                            removed++;
                        }
                    }
                }
                else
                {
                    break;
                }
            }

            // Thêm cho đến khi đầy (vẫn đảm bảo chia hết cho 3)
            while (colorPool.Count + 3 <= tubeItems.Count)
            {
                // Thêm 3 ô của một màu ngẫu nhiên
                var randomColor = selectedColors[UnityEngine.Random.Range(0, selectedColors.Count)];
                for (int j = 0; j < 3; j++)
                {
                    colorPool.Add(randomColor);
                }
            }

            // Trộn ngẫu nhiên
            for (int i = 0; i < colorPool.Count; i++)
            {
                int randomIndex = UnityEngine.Random.Range(i, colorPool.Count);
                var temp = colorPool[i];
                colorPool[i] = colorPool[randomIndex];
                colorPool[randomIndex] = temp;
            }

            // Hạn chế 3 màu giống liên tiếp
            colorPool = AvoidThreeConsecutiveColors(colorPool);

            // Gán màu - fill càng nhiều càng tốt
            for (int i = 0; i < tubeItems.Count; i++)
            {
                if (i < colorPool.Count)
                {
                    tubeItems[i].name = colorPool[i].ID.ToString();
                    tubeItems[i].color = colorPool[i].color;
                }
                // Các slot còn lại đã được reset màu trống
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

                for (int i = 0; i < invalidIds.Count; i++)
                {
                    Button a = colorToolLevel.buttonColors.FirstOrDefault(b => invalidIds[i].ToString() == b.name);
                    a.image.DOFade(0, 0.2f).SetLoops(2, LoopType.Yoyo).OnComplete(() =>
                    {
                        a.image.DOFade(1, 0);
                    });
                }

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

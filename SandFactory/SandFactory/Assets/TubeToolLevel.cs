using System;
using System.Collections.Generic;
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
        private void Start()
        {
            Btn_gen.onClick.AddListener(Gen);
        }
        public void LoadNewTube(List<int>  ids)
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

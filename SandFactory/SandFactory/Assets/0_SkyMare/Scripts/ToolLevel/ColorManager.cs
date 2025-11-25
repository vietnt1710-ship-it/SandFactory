using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace ToolLevel
{
    public class ColorManager : MonoBehaviour
    {
        public HorizontalSpriteGrid grid;
        public ColorID data;
        public ColorItem colorItem;
        public List<ColorItem> items = new List<ColorItem>();

        
        public TMP_InputField inputField;

        public PaintingData painting;
        public void LoadColor()
        {
            Clear();

            for (int i = 0; i < data.colorWithIDs.Count; i++)
            {
                var c = Instantiate(colorItem);
                c.transform.SetParent(transform, false);
                c.OnInitialize(data.colorWithIDs[i]);
                c.gameObject.SetActive(true);
                items.Add(c);
            }
            grid.Fit();

            Debug.Log("Colors is Loaded");
        }
        public void Clear()
        {
            for (int i = 0;i < items.Count; i++)
            {
                DestroyImmediate(items[i].gameObject);
            }
            items.Clear();
        }
        ColorItem clone;

        private void Update()
        {
            // Chỉ xử lý 1 piece trong 1 thời điểm
            if (painting.handling != null) return;

            if (Input.GetMouseButtonDown(0))
            {
                ColorItem ic = ToolManager.I.CastItem();
                if (ic != null)
                {
                    clone = Instantiate(ic);
                    clone.transform.localScale = Vector3.one * 0.5f;
                    clone.GetComponent<Collider2D>().enabled = false;
                }

            }
            if (Input.GetMouseButtonUp(0))
            {
                if (clone != null)
                {
                    PieceData pa = ToolManager.I.CastPiece();
                    Debug.Log($"PieceData..{pa}");
                    if (pa != null)
                    {
                        Vector2 worldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                        pa.InitInfor( clone.colorID, clone.color, worldPos);
                        painting.Handler(pa);
                    }
                    Destroy(clone.gameObject);
                    clone = null; 
                }
            }
            if (clone != null)
            {
                Vector3 worldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                worldPos.z = 0;
                clone.transform.position = worldPos;
            }
        }
    }
}


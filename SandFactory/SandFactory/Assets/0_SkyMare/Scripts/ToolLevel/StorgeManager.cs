using Data;
using DG.Tweening;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ToolLevel
{
    public class StorgeManager : MonoBehaviour
    {
        public HorizontalSpriteGrid grid;
        public TileData tileData;
        public List<TileData> tiles = new List<TileData>();
        public ColorID datas;
        bool isFetched=false;

        public GameObject buttonExport;

        private void Start()
        {
            CheckCanExport();
        }
        public void CheckCanExport()
        {
            if (!isFetched)
            {
                buttonExport.SetActive(false);
                return;
            }
            for (int i = 0; i < tiles.Count; i++)
            {
                if (tiles[i].amount != 0)
                {
                    buttonExport.SetActive(false);
                    return;
                }
            }
            buttonExport.SetActive(true);
        }
        public void GenData(Storge storge)
        {
            Clear();
            DOVirtual.DelayedCall(0.2f, () =>
            {
                foreach (var item in storge.storgeItems)
                {
                    var t = tiles.FirstOrDefault(t => t.colorID == item._colorID);
                    if (t == null)
                    {
                        t = Instantiate(tileData, Vector3.zero, tileData.transform.rotation);
                        tiles.Add(t);
                        t.transform.SetParent(transform);
                        t.gameObject.SetActive(true);
                        t.Init(item._colorID, item.storgeValue, datas.colorWithIDs[item._colorID - 1].color);
                        LoadParent(t);
                    }

                    t.UpdateStorge(item.storgeValue);

                }
                grid.Fit();
                isFetched = true;
            });
          
        }

        public void LoadParent(TileData tile)
        {
            SlotTile[,] slots = ToolManager.I.tileManager.gridTile;
            int rows = slots.GetLength(0);
            int cols = slots.GetLength(1);
            tile.tiles.Clear();
            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < cols; j++)
                {
                    SlotTile slot = slots[i, j];
                    Debug.Log($"Slot data in {i} , {j} : {slot.type} , {slot.data.colorID}");
                    if(slot.type != Type._default && slot.type != Type.pipe)
                    {
                        if(slot.data.colorID == tile.colorID)
                        {
                            slot.data.parent = tile;
                            tile.tiles.Add(slot);
                        }
                    }
                    else if(slot.type == Type.pipe)
                    {
                        tile.tiles.Add(slot);
                        for (int k = 0; k < slot.pipeData.Count; k++)
                        {
                            if (slot.pipeData[k].colorID == tile.colorID)
                            {
                                slot.pipeData[k].parent = tile;
                            }
                        }
                    }
                }
            }
            tile.UpdateAmount();
        }

        public void GenData(Dictionary<int, int> _amounts)
        {
            //Clear();

            // Danh sách các tiles cần xóa
            List<TileData> tilesToRemove = new List<TileData>();

            // Kiểm tra các tiles hiện có
            foreach (var t in tiles)
            {
                if (!_amounts.ContainsKey(t.colorID))
                {
                    tilesToRemove.Add(t);
                }
            }

            // Xóa các tiles không còn trong _amounts
            foreach (var t in tilesToRemove)
            {
                t.UpdateStorge(0);
            }

            // Cập nhật hoặc tạo mới tiles
            foreach (var item in _amounts)
            {
                var t = tiles.FirstOrDefault(t => t.colorID == item.Key);
                if (t == null)
                {
                    t = Instantiate(tileData, Vector3.zero, tileData.transform.rotation);
                    tiles.Add(t);
                    t.transform.SetParent(transform);
                    t.gameObject.SetActive(true);
                    t.Init(item.Key, item.Value, datas.colorWithIDs[item.Key - 1].color);
                }
                t.UpdateStorge(item.Value);
            }

            grid.Fit();
            isFetched = true;
        }

        public void Clear()
        {
            for (int i = 0; i < tiles.Count; i++)
            {
                Destroy(tiles[i].gameObject);
            }
            tiles.Clear();
        }
    }
}


using Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;


namespace ToolLevel
{
    public class ToolManager : Singleton<ToolManager>
    {

        [Header("Settings")]
        public PaintingData paintingData;
        public TileManager tileManager;
        public StorgeManager storgeManager;
        public PaintingInfor paintingInfor;
        public ColorID colorID;

        public Color ColorWithID(int colorID)
        {
            return this.colorID.colorWithIDs.FirstOrDefault(c => c.ID == colorID).color;
        }
#if UNITY_EDITOR
        public string levelDataPath = "Assets/0_SkyMare/Data/Config/Level";
        public string picDataPath = "Assets/0_SkyMare/Data/Config/Picture";

        public string fileName;
        List<SpriteInfor> spriteInfors;

        public PicData picData;
        public Button reloadData;

       
        public void LoadNewPicture( List <SpriteInfor> infors, string psBName)
        {
            spriteInfors = new List<SpriteInfor>(infors);
            fileName = psBName;
            // Ghép đường dẫn đầy đủ đến file asset
            string fullPath = Path.Combine(picDataPath, psBName + ".asset");

            // Kiểm tra file có tồn tại trong project không
            Object obj = AssetDatabase.LoadAssetAtPath<Object>(fullPath);
            if (obj == null)
            {
                Debug.LogWarning($"❌ Không tìm thấy asset: {fullPath}");
                picData = null;
            }
            else
            {
                PicData so = obj as PicData;
                if (so != null)
                {
                    picData = so;
                }
            }
            reloadData.gameObject.SetActive(picData != null);
            // Kiểm tra có phải ScriptableObject không


            paintingData.LoadNewPicture(infors);
            keyLock.Clear();
            //tileManager.ReLoad(ExpandGrid(levelData.grid));
            storgeManager.Clear();
            tileManager.Clear();
        }

        public void LoadDataToTool()
        {
            if (picData.levelData == null) return;

            paintingData.LoadPaintingByData(picData.levelData.painting);

            tileManager.ReLoad(ExpandGrid(picData.levelData.grid));

            LoadPrePareToCouple();

            storgeManager.GenData(picData.levelData.storge);
        }

        private void Export()
        {
            string timestamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
            // 1️⃣ Đảm bảo folder tồn tại
            Directory.CreateDirectory(levelDataPath);
            Directory.CreateDirectory(picDataPath);

            // 2️⃣ Tạo ScriptableObjectA mới và gán text
            LevelData newData = ScriptableObject.CreateInstance<LevelData>();
            newData.storge = new Data.Storge(storgeManager);
            newData.painting = new Data.Painting(paintingData);
            newData.grid = TrimGrid(tileManager.Grid());
            SaveGrid(newData.grid, newData);


            string assetName = $"{fileName.ToUpper()}_save_at_{timestamp}";
            string assetAPath = Path.Combine(levelDataPath, assetName + ".asset");
            AssetDatabase.CreateAsset(newData, assetAPath);
            Debug.Log("Đã tạo ScriptableObjectA tại: " + assetAPath);

            // 3️⃣ Kiểm tra ScriptableObjectB đã tồn tại chưa
            string assetBPath = Path.Combine(picDataPath, fileName + ".asset");
            //ScriptableObjectB dataB = AssetDatabase.LoadAssetAtPath<ScriptableObjectB>(assetBPath);

            if (picData == null)
            {
                // 4️⃣ Nếu chưa có → tạo mới
                picData = ScriptableObject.CreateInstance<PicData>();
                picData.levelData = newData;
                picData.currentSprites = spriteInfors;
                AssetDatabase.CreateAsset(picData, assetBPath);
                Debug.Log("Đã tạo ScriptableObjectB mới tại: " + assetBPath);
            }
            else
            {
                // 5️⃣ Nếu có rồi → gán A mới vào
                picData.levelData = newData;
                picData.currentSprites = spriteInfors;
                EditorUtility.SetDirty(newData);
                Debug.Log("Đã cập nhật ScriptableObjectB tại: " + assetBPath);
            }

            // 6️⃣ Lưu lại database
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("✅ Export hoàn tất!");
        }
        public void CreateScriptableObjectInEditor()
        {
#if UNITY_EDITOR
            Export();

#else
    Debug.LogWarning("Chỉ có thể tạo ScriptableObject trong Unity Editor!");
#endif
        }
        public async void SaveGrid(string[,] grid, LevelData levelData)
        {
            // Tạo timestamp tránh ký tự không hợp lệ
            string timestamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
            string fullPath = $"{levelDataPath}/{fileName}_TextGrid_{timestamp}.txt";

            StringBuilder sb = new StringBuilder();

            int rows = grid.GetLength(0);
            int cols = grid.GetLength(1);

            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < cols; j++)
                {
                    sb.Append(grid[i, j]);
                    if (j < cols - 1)
                        sb.Append(" ");
                }
                if (i < rows - 1)
                    sb.AppendLine();
            }

            await Task.Run(() =>
            {
                string directory = Path.GetDirectoryName(fullPath);

                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }
                else if (string.IsNullOrEmpty(directory))
                {
                    // Nếu không có thư mục, dùng thư mục hiện tại
                    directory = Directory.GetCurrentDirectory();
                    fullPath = Path.Combine(directory, Path.GetFileName(fullPath));
                }

                File.WriteAllText(fullPath, sb.ToString());
            });

            // Làm mới AssetDatabase để nhận diện file mới
            AssetDatabase.Refresh();

            // Load lại file vừa tạo thành TextAsset
            TextAsset textAsset = AssetDatabase.LoadAssetAtPath<TextAsset>(fullPath);
            levelData.gridDataFile = textAsset;

            Debug.Log($"✅ Đã tạo file mới tại: {fullPath}");
        }
        /// <summary>
        /// Loại bỏ các hàng và cột chỉ chứa toàn số 0 từ mảng 2 chiều
        /// </summary>
        /// <param name="grid">Mảng 2 chiều string[,]</param>
        /// <returns>Mảng 2 chiều đã được trim</returns>
        public static string[,] TrimGrid(string[,] grid)
        {
            if (grid == null || grid.Length == 0)
                return new string[0, 0];

            int rows = grid.GetLength(0);
            int cols = grid.GetLength(1);

            // Tìm hàng đầu tiên không toàn 0
            int firstRow = -1;
            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < cols; j++)
                {
                    if (grid[i, j] != "0")
                    {
                        firstRow = i;
                        break;
                    }
                }
                if (firstRow != -1) break;
            }

            // Nếu toàn grid là 0
            if (firstRow == -1)
                return new string[0, 0];

            // Tìm hàng cuối cùng không toàn 0
            int lastRow = -1;
            for (int i = rows - 1; i >= 0; i--)
            {
                for (int j = 0; j < cols; j++)
                {
                    if (grid[i, j] != "0")
                    {
                        lastRow = i;
                        break;
                    }
                }
                if (lastRow != -1) break;
            }

            // Tìm cột đầu tiên không toàn 0
            int firstCol = -1;
            for (int j = 0; j < cols; j++)
            {
                for (int i = 0; i < rows; i++)
                {
                    if (grid[i, j] != "0")
                    {
                        firstCol = j;
                        break;
                    }
                }
                if (firstCol != -1) break;
            }

            // Tìm cột cuối cùng không toàn 0
            int lastCol = -1;
            for (int j = cols - 1; j >= 0; j--)
            {
                for (int i = 0; i < rows; i++)
                {
                    if (grid[i, j] != "0")
                    {
                        lastCol = j;
                        break;
                    }
                }
                if (lastCol != -1) break;
            }

            // Tạo mảng mới với kích thước đã trim
            int newRows = lastRow - firstRow + 1;
            int newCols = lastCol - firstCol + 1;
            string[,] trimmedGrid = new string[newRows, newCols];

            // Copy dữ liệu
            for (int i = 0; i < newRows; i++)
            {
                for (int j = 0; j < newCols; j++)
                {
                    trimmedGrid[i, j] = grid[firstRow + i, firstCol + j];
                }
            }

            return trimmedGrid;
        }
        /// <summary>
        /// Đưa grid đã trim vào lại grid có kích thước cố định (mặc định 10x10)
        /// </summary>
        /// <param name="trimmedGrid">Grid đã được trim</param>
        /// <param name="targetRows">Số hàng mong muốn (mặc định 10)</param>
        /// <param name="targetCols">Số cột mong muốn (mặc định 10)</param>
        /// <param name="offsetRow">Vị trí hàng bắt đầu đặt grid (mặc định 0)</param>
        /// <param name="offsetCol">Vị trí cột bắt đầu đặt grid (mặc định 0)</param>
        /// <returns>Grid mới với kích thước cố định</returns>
        public string[,] ExpandGrid(string[,] trimmedGrid, int targetRows = 10, int targetCols = 10)
        {
            if (trimmedGrid == null || trimmedGrid.Length == 0)
            {
                trimmedGrid = picData.levelData.TxTToGrid();
            }

            int trimRows = trimmedGrid.GetLength(0);
            int trimCols = trimmedGrid.GetLength(1);

            // Tính toán offset để căn giữa trimmedGrid
            int offsetRow = 0;// (targetRows - trimRows) / 2;
            int offsetCol = (targetCols - trimCols) / 2;

            // Tạo grid mới với toàn bộ là "0"
            string[,] expandedGrid = CreateEmptyGrid(targetRows, targetCols);

            // Copy dữ liệu từ trimmed grid vào vị trí trung tâm
            for (int i = 0; i < trimRows && (offsetRow + i) < targetRows; i++)
            {
                for (int j = 0; j < trimCols && (offsetCol + j) < targetCols; j++)
                {
                    expandedGrid[offsetRow + i, offsetCol + j] = trimmedGrid[i, j];
                    Debug.Log($"Value of ExpandGrid Tile({offsetRow + i}, {offsetCol + j}): {expandedGrid[offsetRow + i, offsetCol + j]}");
                }
            }

            return expandedGrid;
        }
    
        /// <summary>
        /// Tạo grid rỗng với toàn bộ giá trị là "0"
        /// </summary>
        private static string[,] CreateEmptyGrid(int rows, int cols)
        {
            string[,] grid = new string[rows, cols];
            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < cols; j++)
                {
                    grid[i, j] = "0";
                }
            }
            return grid;
        }



#endif

        private Dictionary<SlotTile, SlotTile> keyLock = new Dictionary<SlotTile, SlotTile>();
        public List<SlotTilePipe> slotTilePipes = new List<SlotTilePipe>();
        public List<SlotTile> prepareSlot = new List<SlotTile>();

        public void LoadPrePareToCouple()
        {
            keyLock.Clear();
            keyLock = FindKeyLockPairs();
            prepareSlot.Clear();
        }
        public Dictionary<SlotTile, SlotTile> FindKeyLockPairs()
        {
            Dictionary<SlotTile, SlotTile> keyLock = new Dictionary<SlotTile, SlotTile>();

            for (int i = 0; i < prepareSlot.Count; i++)
            {
                // Bỏ qua nếu đã được ghép cặp hoặc không có keylockID
                if (keyLock.ContainsKey(prepareSlot[i]) || string.IsNullOrEmpty(prepareSlot[i].keylockID))
                    continue;

                // Tìm cặp của nó
                for (int j = i + 1; j < slotTilePipes.Count; j++)
                {
                    if (prepareSlot[i].keylockID == prepareSlot[j].keylockID)
                    {
                        keyLock[prepareSlot[i]] = prepareSlot[j];
                        keyLock[prepareSlot[j]] = prepareSlot[i];
                        break;
                    }
                }
            }

            return keyLock;
        }
        public void SubcriseKeyLock(SlotTile key, SlotTile _lock)
        {
            List<SlotTile> keysToClear = new List<SlotTile>();
            List<SlotTile> keysToDelete = new List<SlotTile>();

            foreach (var tile in keyLock)
            {
                if (tile.Value == _lock && tile.Key != key) // nếu đà là lock của key khác thì xét null key
                {
                    keysToClear.Add(tile.Key);
                }
                if(tile.Key == _lock) // nếu lock mới là 1 key đã tồn tại thì xét null key và chuyển lock của nó thành normal
                {
                    keysToDelete.Add(tile.Key);
                    //keyLock[tile.Value].ChangeType(Type.normal);
                }
            }

            foreach (var k in keysToClear)
            {
                keyLock.Remove(k);
            }
            foreach (var k in keysToDelete)
            {
                keyLock[k].ChangeType(Type.normal);
                keyLock.Remove(k);
            }
            keyLock[key] = _lock;
            key.UpdateValue();
            _lock.UpdateValue();
        }
        public SlotTile GetLock(SlotTile key)
        {
           
            if (keyLock.ContainsKey(key))
            {
                return keyLock[key];
            }
            return null;
        }

        public int KeyLockID(SlotTile key)
        {
            int count = 0;
            foreach(var tile in keyLock)
            {
                if(tile.Key == key)
                {
                    return count;
                }
                count++;
            }
            return -1;
        }
        public int LockKeyID(SlotTile _lock)
        {
            int count = 0;
            foreach (var tile in keyLock)
            {
                if (tile.Value == _lock)
                {
                    return count;
                }
                count++;
            }
            return -1;
        }
        public void ReMoveKey(SlotTile key)
        {
            if (keyLock.ContainsKey(key))
            {
                if (keyLock[key] != null)
                {
                    keyLock[key].ChangeType(Type.normal);
                }
                keyLock.Remove(key);
            }
        }
        public void ReMoveKeyLock(SlotTile _lock)
        {
            List<SlotTile> keysToClear = new List<SlotTile>();

            foreach (var tile in keyLock)
            {
                if (tile.Value == _lock ) // nếu đà là lock của key khác thì xét remove key
                {
                    keysToClear.Add(tile.Key);
                }
            }

            foreach (var k in keysToClear)
            {
                keyLock.Remove(k);
            }
        }
        public TileData CastTileData()
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            RaycastHit hit;
            Physics.Raycast(ray,out hit,1000);

            if (hit.collider != null)
            {
                return hit.collider.gameObject.GetComponent<TileData>();
            }
            return null;
        }
        public SlotTile CastSlot()
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            RaycastHit hit;
            Physics.Raycast(ray, out hit, 1000);

            if (hit.collider != null)
            {
                return hit.collider.gameObject.GetComponent<SlotTile>();
            }
            return null;
        }
        public SlotTilePipe CastSlotPipe()
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            RaycastHit hit;
            Physics.Raycast(ray, out hit, 1000);

            if (hit.collider != null)
            {
                return hit.collider.gameObject.GetComponent<SlotTilePipe>();
            }
            return null;
        }
        public ColorItem CastItem()
        {
            Vector2 worldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            RaycastHit2D hit = Physics2D.Raycast(worldPos, Vector2.zero);

            if (hit.collider != null)
            {
                return hit.collider.gameObject.GetComponent<ColorItem>();
            }
            return null;
        }
        public PieceData CastPiece()
        {
            Vector2 worldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            RaycastHit2D hit = Physics2D.Raycast(worldPos, Vector2.zero);

            if (hit.collider != null)
            {
                return hit.collider.gameObject.GetComponent<PieceData>();
            }
            return null;
        }

    }
}



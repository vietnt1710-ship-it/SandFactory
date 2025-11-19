
using DG.Tweening;
using DG.Tweening.Plugins.Options;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MazeGenerate : MonoBehaviour
{
    [Header("Prefabs Setting")]
    public GameObject objectTile;
    public WallTile wallTile;
    public PipeTile pipe;

    [Header("Instantiate")]
    public MeshCombine wallCombine;
    public List<WallTile> walls;

    [Header("Size")]
    [Tooltip("Khoảng cách X")]
    public float cellSizeX = 1f;
    [Tooltip("Khoảng cách Z")]
    public float cellSizeZ = 1f;
    public float maxZ = 6;
    private void Start()
    {
        Application.targetFrameRate = 120;
    }
    public void LoadLevel(string[,] grid)
    {
        StringWrapper[,] wrapperGrid = WrapLeftRightBottom(grid, 4, "0");
        GenerateGrid(grid, wrapperGrid);

        grid = null;
        wrapperGrid = null;

        MazeManager.I.StartActiceLevel();
    }
    public void Replay()
    {
        SceneManager.LoadScene(0);
    }
    public void Reset()
    {
        while (wallCombine.transform.childCount > 0)
        {
            DestroyImmediate(wallCombine.transform.GetChild(0).gameObject);
        }
        MazeManager.I.Reset();
    }

    void GenerateGrid(string[,] grid, StringWrapper[,] wrapperGrid)
    {
        int rows = grid.GetLength(0);
        int cols = grid.GetLength(1);
        MazeManager.I.grid = new Tile[rows, cols];


        int wrows = wrapperGrid.GetLength(0);
        int wcols = wrapperGrid.GetLength(1);
        //
        // int X = 0; size = 9 thì row 0 ở 4 , x = ? row = 0
        // Gốc tọa độ (0,0) tại TÂM lưới
        float originX = -(wcols - 1) * 0.5f * cellSizeX;
        float originY =  - (wrows - 1) * 0.5f * cellSizeZ;
        float maxZOffset = maxZ - (originY + (wrows - 1 - 0) * cellSizeZ);
        var kl = new KeyLockRegistry();

        for (int row = 0; row < wrows; row++)
        {
            for (int col = 0; col < wcols; col++)
            {
                // X: trái -> phải ; Y: trên -> dưới (row 0 ở trên)
                float worldX = originX + col * cellSizeX;
                float worldY = originY + (wrows - 1 - row) * cellSizeZ ;
                worldY += maxZOffset;

                string cell = wrapperGrid[row, col].content;

                int realRow = wrapperGrid[row, col].realRow;
                int realCol = wrapperGrid[row, col].realCol;


               
                int type = GridParse.TypeOf(cell);

                if(realRow != -1 && realCol != -1)
                {
                    Debug.Log($"Real in [{row} ,{col}] is [{realRow} ,{realCol}]");
                    if (type == 0)
                    {
                        CreateWall(worldX, worldY, realRow, realCol, row, col);
                        continue;
                    }

                    // tách be/af 1 lần, dùng lại cho các case
                    GridParse.OnSplitBeAf(cell, out string be, out string af);

                    if (type == 6)
                    {
                        CreatePipe(worldX, worldY, realRow, realCol, be, af);
                        continue;
                    }

                    CreateObjectTile(worldX, worldY, realRow, realCol, type, be, af, kl);
                }
                else
                {
                    CreateHardWall(worldX, worldY, row, col);
                }
               
            }
        }
        GenConner(wrapperGrid);
        MazeManager.I.FindCorners();
      
        CombineWall();
    }

    /// <summary>
    /// Creater wall
    /// </summary>
    void CreateHardWall(float worldX, float worldY,int wrow , int wcol)
    {
        var t = Instantiate(wallTile, new Vector3(worldX, 0f, worldY), wallTile.transform.rotation, transform);
        t.transform.localEulerAngles = Vector3.zero;
        t.transform.localPosition = new Vector3(worldX, -0.91f, worldY);
        t.status = Tile.CellStatus.wall;
        t.wrow = wrow;
        t.wcol = wcol;
        t.row = -1;
        t.col = -1;
        walls.Add(t);
        t.transform.SetParent(wallCombine.transform);
    }
    /// <summary>
    /// Creater wall
    /// </summary>
    void CreateWall(float worldX, float worldY, int row , int col,int wrow , int wcol)
    {
        var t = Instantiate( wallTile, new Vector3(worldX, 0f, worldY), wallTile.transform.rotation, transform);

        t.transform.localEulerAngles = Vector3.zero;
        t.transform.localPosition = new Vector3(worldX, -0.91f, worldY);

        t.status = Tile.CellStatus.wall;
        t.row = row;
        t.col = col;
        t.wrow = wrow;
        t.wcol = wcol;
        MazeManager.I.grid[row, col] = t;

        walls.Add(t);
        t.transform.SetParent(wallCombine.transform);
    }
    /// <summary>
    /// Create Object tile
    /// </summary>
    /// <param name="type">Loại tile</param>
    /// <param name="be">String phân loại</param>
    /// <param name="af">String nội dung</param>
    /// <param name="kl">Đăng ký key _lock</param>
    void CreateObjectTile(float worldX, float worldY, int row, int col, int type, string be, string af, KeyLockRegistry kl)
    {
        GameObject go = Instantiate(objectTile, new Vector3(worldX, 0f, worldY), objectTile.transform.rotation, transform);

        go.transform.localEulerAngles = Vector3.zero;
        go.transform.localPosition = new Vector3(worldX, 0f, worldY);
        ObjectTile t;
        switch (type)
        {
            case 1: // Normal
                t = go.AddComponent<NormalTile>();
                t.type = TileType.normal;
                break;

            case 2: //Hidden
                t = go.AddComponent<HiddenTile>();
                t.type = TileType.hiden;
                break;

            case 3: // Frezze
                t = go.AddComponent<Frezze>();
                t.type = TileType.frezee;
                if (t is Frezze f) MazeManager.I.frezzeTiles.Add(f);
                break;

            case 4: // Key
                var keyId = GridParse.IdAfter1Char(be);
                var k = go.AddComponent<KeyTile>();
                t = k;
                t.type = TileType.key;
                k.keyID = keyId;
                kl.RegisterKey(k);
                break;

            case 5: // Lock
                var lockId = GridParse.IdAfter1Char(be);
                var l = go.AddComponent<LockTile>();
                t = l;
                t.type = TileType._lock;
                l.lockID = lockId;
                kl.RegisterLock(l);
                break;

            default:
                // fallback an toàn 
                t = go.AddComponent<NormalTile>();
                t.type = TileType.normal;
                break;
        }

        // parse màu & cát
        (int colorID, int sandAmount) im = GridParse.OnSplitID(af);
        t.colorID = im.colorID;
        t.sandAmount = im.sandAmount;

        // gán grid
        t.row = row;
        t.col = col;
        t.status = Tile.CellStatus.watting;
        MazeManager.I.grid[row, col] = t;

        t.Initizal();
    }

    /// <summary>
    /// Tạo đường ống chứa nhiều tile
    /// </summary>
    /// <param name="af">String nội dung</param>
    void CreatePipe(float worldX, float worldY, int row, int col, string be, string af)
    {
        PipeTile t = Instantiate(pipe, new Vector3(worldX, 0f, worldY), pipe.transform.rotation, transform);
        t.transform.localEulerAngles = Vector3.zero;
        t.transform.localPosition = new Vector3(worldX, 0f, worldY);

        t.row = row;
        t.col = col;
        t.type = TileType.pipe;

        var listIm = GridParse.OnSplitPipe(af);
        t.InitPipe(listIm, be);
        

        MazeManager.I.grid[row, col] = t;
        MazeManager.I.pipeTiles.Add(t);
    }
    /// <summary>
    /// Tạo góc sau khi gắn asset
    /// </summary>
    public void GenConner(StringWrapper[,] wrapperGrid)
    {
        for (int i = 0; i < walls.Count; i++)
        {
            walls[i].CheckCorner( wrapperGrid);
        }

    }
    public void CombineWall()
    {
        wallCombine.CombineNow();

        walls.Clear();
        walls.TrimExcess();
        //DOVirtual.DelayedCall(0.04f, () =>
        //{
        //    wallCombine.CombineNow();

        //    walls.Clear();
        //    walls.TrimExcess();

        //});
    }
   

    [SerializeField] private TextAsset matrixText;
    /// <summary>
    /// Load nội dung từ file csv vào grid
    /// </summary>
    /// <returns></returns>
    string[,] LoadDataTest()
    {
        string[] lines = matrixText.text.Split('\n');
        int c_row = lines.Length;

        string[] col_0 = lines[0].Split(',');

        int c_col = col_0.Length;

        string[,] grid = new string[c_row, c_col];

        for (int row = 0; row < c_row ; row++)
        {
            string[] cols = lines[row].Split(',');

            for (int col = 0; col < cols.Length; col++)
            {
                try
                {
                    grid[row, col] = cols[col];
                }
                catch
                {
                    Debug.Log($"LogIndex{row} + {col} : {cols[col]}");
                }
               
            }
        }
        return grid;
    }
    /// <summary>
    /// Wrap grid nội dung bằng wall
    /// </summary>
    /// <param name="raw"></param>
    /// <returns></returns>
    StringWrapper[,] WrapLeftRightBottom(string[,] innerGrid, int wallThickness = 2, string wallToken = "0")
    {
        int innerRows = innerGrid.GetLength(0);
        int innerCols = innerGrid.GetLength(1);

        // Kích thước mới sau khi bọc
        int outRows = innerRows + wallThickness *2 ;       // thêm tường dưới
        int outCols = innerCols + wallThickness * 2;   // thêm tường trái + phải

        StringWrapper[,] wrapped = new StringWrapper[outRows, outCols];

        for (int r = 0; r < outRows; r++)
        {
            for (int c = 0; c < outCols; c++)
            {
                bool isLeftWall = c < wallThickness;
                bool isRightWall = c >= outCols - wallThickness;
                bool isBottomWall = r >= innerRows; // chỉ bọc phía dưới

                if (isLeftWall || isRightWall || isBottomWall)
                {
                    // Ô là tường → không có vị trí thật
                    wrapped[r, c] = new StringWrapper(wallToken);
                }
                else
                {
                    // Ô trong core → lưu cả tọa độ thật
                    int innerR = r;
                    int innerC = c - wallThickness;

                    wrapped[r, c] = new StringWrapper(innerGrid[innerR, innerC], innerR, innerC);
                }
            }
        }

        return wrapped;
    }
    public struct StringWrapper
    {
        public string content;
        public int realRow;
        public int realCol;

        public StringWrapper (string value, int realRow = -1, int realCol = -1)
        {
            this.content = value;
            this.realRow = realRow;
            this.realCol = realCol;
        }
    }
}




using DG.Tweening;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class MazeManager : Singleton<MazeManager>
{
    [Header("Settings")]
    int rows;
    int cols;

    public Tile[,] grid;

    public ColorID colorData;

    public List<Frezze> frezzeTiles;

    public List<PipeTile> pipeTiles;

    public static readonly (int dr, int dc)[] DIRS =
    {
     (-1, 0), // up
     ( 1, 0), // down
     ( 0, 1), // right
     ( 0,-1), // left
    };

    public void Reset()
    {
        ClearGrid();
        frezzeTiles.Clear();
        pipeTiles.Clear();
    }
   

    public void StartActiceLevel()
    {
        DOVirtual.DelayedCall(0.5f, () =>
        {
            float delayTime = (MazeManager.I.grid.GetLength(0)) * 0.1f + (MazeManager.I.grid.GetLength(0)) * 0.1f;
            DOVirtual.DelayedCall(delayTime, () =>
            {
                CheckAvtive();
            });
        });
    }
    public void FindCorners()
    {
        int rows = grid.GetLength(0);
        int cols = grid.GetLength(1);

        //for (int i = 0; i < rows; i++)
        //{
        //    for (int j = 0; j < cols; j++)
        //    {
        //        if (grid[i, j].status != Tile.CellStatus.wall)
        //        {
        //            CheckCorner(i, j);
        //        }
        //    }
        //}
        CombineAllCornersToWallCombine();
    }
    void CombineAllCornersToWallCombine()
    {
        int rows = grid.GetLength(0);
        int cols = grid.GetLength(1);
        Transform wallCombineTransform = LevelManager.I.m_mazeGenerate.wallCombine.transform;

        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < cols; j++)
            {
                if (grid[i, j].status != Tile.CellStatus.wall)
                {
                    Transform cellTransform = grid[i, j].transform;

                    // Set parent cho children từ 0 đến 3
                    int count = 0;
                    while (count < 4)
                    {
                        count++;
                        Transform child = cellTransform.GetChild(0);
                        child.SetParent(wallCombineTransform, true);
                    }
                }
            }
        }

        Debug.Log($"<color=green>✓ Hoàn thành combine corners vào wallCombine</color>");
    }
    void CheckCorner(int row, int col)
    {
        int rows = grid.GetLength(0);
        int cols = grid.GetLength(1);

        // Định nghĩa 4 loại góc với các hướng cần check
        var cornerTypes = new[]
        {
            new { Name = "TOP-LEFT", Dirs = new[] { (-1, 0), (0, -1), (-1, -1) } },
            new { Name = "TOP-RIGHT", Dirs = new[] { (-1, 0), (0, 1), (-1, 1) } },
            new { Name = "BOTTOM-LEFT", Dirs = new[] { (1, 0), (0, -1), (1, -1) } },
            new { Name = "BOTTOM-RIGHT", Dirs = new[] { (1, 0), (0, 1), (1, 1) } }
        };

        foreach (var corner in cornerTypes)
        {
            int wallCount = 0;

            foreach (var dir in corner.Dirs)
            {
                int newRow = row + dir.Item1;
                int newCol = col + dir.Item2;

                if ( newRow >= rows || newCol < 0 || newCol >= cols)
                {
                    wallCount++;
                }
                else if(newRow < 0)
                {
                    // Điểm tiếp nối
                }
                else if (grid[newRow, newCol].status == Tile.CellStatus.wall)
                {
                    wallCount++;
                }
            }

            if (wallCount == 3)
            {
                GameObject cornerObject = null;

                // Xử lý cho từng loại góc
                switch (corner.Name)
                {
                    case "TOP-LEFT":
                        cornerObject = grid[row, col].transform.GetChild(0).gameObject;
                        cornerObject.SetActive(true);
                        break;

                    case "TOP-RIGHT":
                        cornerObject = grid[row, col].transform.GetChild(1).gameObject;
                        cornerObject.SetActive(true);
                        break;

                    case "BOTTOM-LEFT":
                        cornerObject = grid[row, col].transform.GetChild(2).gameObject;
                        cornerObject.SetActive(true);
                        break;

                    case "BOTTOM-RIGHT":
                        cornerObject = grid[row, col].transform.GetChild(3).gameObject;
                        cornerObject.SetActive(true);
                        break;
                }

                // Raycast từ cornerObject theo trục Y để tìm Base_Straight
                if (cornerObject != null)
                {
                    DOVirtual.DelayedCall(0.02f, () =>
                    {
                        RaycastCornerToBaseStraight(cornerObject);
                    });
                  
                }
            }
        }
        void RaycastCornerToBaseStraight(GameObject cornerObject)
        {
            Transform cornerTransform = cornerObject.transform;

            // Sử dụng local position và local direction
            Vector3 localOrigin = cornerTransform.position;
            Vector3 localDirection = new Vector3 (0, -0.7660f, 0.6428f);

            float maxDistance = 100f;

           // Debug.DrawRay(localOrigin, localDirection * maxDistance, Color.yellow, 100f);

            RaycastHit[] hits = Physics.RaycastAll(localOrigin, localDirection, maxDistance);

            foreach (RaycastHit hit in hits)
            {

               /// Kiểm tra xem object bị hit có tên chứa "Base_Straight" không
                if (hit.collider.gameObject.name.Contains("Base_Straight"))
                {
                    GameObject baseStraight = hit.collider.gameObject;

                    // Ẩn Base_Straight đi
                    baseStraight.SetActive(false);

                    // Debug để kiểm tra
                    Debug.Log($"<color=cyan>Raycast hit:</color> {baseStraight.name} tại vị trí {hit.point}");
                }
            }
        }
    }

    public void UnFrezze()
    {
        if (frezzeTiles.Count == 0) return;
        if (frezzeTiles[0].UnFezze())
        {
            frezzeTiles.RemoveAt(0);
        }
    }
    public void PipeAtice()
    {
        for (int i = 0; i < pipeTiles.Count; i++)
        {
            pipeTiles[i].SpawmJar();
        }
    }
    public void AfterTileClick()
    {
        PipeAtice();
        UnFrezze();
        CheckAvtive();
    }
    public void CheckAvtive()
    {
         rows = grid.GetLength(0);   
         cols = grid.GetLength(1);   

        for (int row = 0; row < rows; row++)
        {
            for (int col = 0; col < cols; col++)
            {
                if (grid[row, col] is ObjectTile ot)
                {
                    if (NeedCheck(ot))
                    {
                        if (HavePath(row, col))
                        {
                            ot.ActiveTile();
                        }
                    }
                }
            }
        }
    }
    bool NeedCheck(ObjectTile ot)
    {
        if (ot.status  != Tile.CellStatus.watting) return false;
        if(ot.type != TileType.normal && ot.type != TileType.hiden && ot.type != TileType.key)  return false;

        return true;
    }
    public bool HavePath(int row, int col)
    {
        ResetVisited();
        return DFS((row, col));
    }
    private bool DFS((int row, int col) node)
    {
        int row = node.row, col = node.col;

        grid[row, col].visited = true;

        for (int t = 0; t < 4; t++)
        {
            int nrow = row + DIRS[t].dr;
            int ncol = col + DIRS[t].dc;

            if (nrow < 0) return true; // cần tìm

            if (ncol < 0 || nrow >= rows || ncol >= cols) continue;

            if (grid[nrow, ncol].type == TileType.pipe) continue;

            if (grid[nrow, ncol].status != Tile.CellStatus.empty) continue;

            if (grid[nrow, ncol].visited) continue;
          
            if (DFS((nrow, ncol))) return true;

        }

        return false;
    }
    public void ResetVisited()
    {
        for (int row = 0; row < rows; row++)
        {
            for (int col = 0; col < cols; col++)
            {
                if (grid[row, col] is ObjectTile)

                    grid[row, col].visited = false;
            }
        }
    }
    public void ClearGrid()
    {
        if (grid == null) return;

        int width = grid.GetLength(0);
        int height = grid.GetLength(1);

        // Destroy toàn bộ Tile trong grid
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (grid[x, y] != null)
                {
                    Destroy(grid[x, y].gameObject);

                    grid[x, y] = null;
                }
            }
        }

        // Clear tham chiếu
        grid = null;
    }


}

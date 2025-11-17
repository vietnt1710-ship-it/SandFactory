using UnityEngine;
using System.Collections.Generic;

public class CornerDetector : MonoBehaviour
{
    [Header("Matrix Configuration")]
    [TextArea(10, 20)]
    public string matrixDebugOutput = "Kết quả sẽ hiển thị ở đây khi chạy...";

    void Start()
    {
        // Ma trận mẫu
        int[,] matrix = {
            {0, 1, 1, 1, 0},
            {1, 0, 0, 1, 0},
            {1, 0, 1, 1, 1},
            {1, 1, 1, 1, 1},
            {1, 0, 1, 1, 1},
            {1, 1, 1, 1, 1},
            {0, 0, 1, 1, 0}
        };

        string output = "Ma trận:\n";
        output += PrintMatrix(matrix);
        output += "\n" + new string('=', 50) + "\n";
        output += "KẾT QUẢ PHÂN TÍCH CÁC CELL GÓC:\n";
        output += new string('=', 50) + "\n";
        
        output += FindCorners(matrix);

        matrixDebugOutput = output;
        Debug.Log(output);
    }

    string FindCorners(int[,] matrix)
    {
        int rows = matrix.GetLength(0);
        int cols = matrix.GetLength(1);
        int cornerCount = 0;
        string result = "";

        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < cols; j++)
            {
                if (matrix[i, j] == 1) // Chỉ check cell, không check tường
                {
                    result += CheckCorner(matrix, i, j, ref cornerCount);
                }
            }
        }

        if (cornerCount == 0)
        {
            result += "\nKhông tìm thấy cell góc nào!";
        }
        else
        {
            result += $"\n{new string('=', 50)}\n";
            result += $"TỔNG CỘNG: Tìm thấy {cornerCount} góc\n";
        }

        return result;
    }

    string CheckCorner(int[,] matrix, int row, int col, ref int cornerCount)
    {
        int rows = matrix.GetLength(0);
        int cols = matrix.GetLength(1);
        string result = "";

        // Định nghĩa 4 loại góc với các hướng cần check
        var cornerTypes = new[]
        {
            // Góc Trên-Trái: check Trên, Trái, và chéo Trên-Trái
            new { Name = "TOP-LEFT", Dirs = new[] { (-1, 0), (0, -1), (-1, -1) } },
            
            // Góc Trên-Phải: check Trên, Phải, và chéo Trên-Phải
            new { Name = "TOP-RIGHT", Dirs = new[] { (-1, 0), (0, 1), (-1, 1) } },
            
            // Góc Dưới-Trái: check Dưới, Trái, và chéo Dưới-Trái
            new { Name = "BOTTOM-LEFT", Dirs = new[] { (1, 0), (0, -1), (1, -1) } },
            
            // Góc Dưới-Phải: check Dưới, Phải, và chéo Dưới-Phải
            new { Name = "BOTTOM-RIGHT", Dirs = new[] { (1, 0), (0, 1), (1, 1) } }
        };

        foreach (var corner in cornerTypes)
        {
            int wallCount = 0;
            List<string> debugInfo = new List<string>();

            foreach (var dir in corner.Dirs)
            {
                int newRow = row + dir.Item1;
                int newCol = col + dir.Item2;

                string direction = GetDirectionName(dir.Item1, dir.Item2);
                
                if (newRow < 0 || newRow >= rows || newCol < 0 || newCol >= cols)
                {
                    wallCount++;
                    debugInfo.Add($"  {direction}: VƯỢT BIÊN");
                }
                else if (matrix[newRow, newCol] == 0)
                {
                    wallCount++;
                    debugInfo.Add($"  {direction}: TƯỜNG (0)");
                }
                else
                {
                    debugInfo.Add($"  {direction}: Cell (1)");
                }
            }

            // Nếu cả 3 hướng đều là tường/vượt biên thì đây là góc
            if (wallCount == 3)
            {
                cornerCount++;
                result += $"\n🔴 CORNER #{cornerCount} - Góc {corner.Name}\n";
                result += $"   Vị trí: [{row}, {col}]\n";
                result += $"   Kiểm tra các hướng:\n";
                foreach (var info in debugInfo)
                {
                    result += info + "\n";
                }
                result += $"   ✓ Kết luận: Đây là góc {corner.Name} (3/3 hướng là tường/biên)\n";

                // Debug với màu sắc trong Unity Console
                Debug.Log($"<color=red>CORNER #{cornerCount}</color> - <color=yellow>{corner.Name}</color> tại [{row}, {col}]");
            }
        }

        return result;
    }

    string GetDirectionName(int dr, int dc)
    {
        if (dr == -1 && dc == 0) return "TRÊN     ";
        if (dr == 1 && dc == 0) return "DƯỚI     ";
        if (dr == 0 && dc == -1) return "TRÁI     ";
        if (dr == 0 && dc == 1) return "PHẢI     ";
        if (dr == -1 && dc == -1) return "CHÉO T-T ";
        if (dr == -1 && dc == 1) return "CHÉO T-P ";
        if (dr == 1 && dc == -1) return "CHÉO D-T ";
        if (dr == 1 && dc == 1) return "CHÉO D-P ";
        return "???";
    }

    string PrintMatrix(int[,] matrix)
    {
        int rows = matrix.GetLength(0);
        int cols = matrix.GetLength(1);
        string result = "";

        result += "    ";
        for (int j = 0; j < cols; j++)
        {
            result += $"[{j}] ";
        }
        result += "\n";

        for (int i = 0; i < rows; i++)
        {
            result += $"[{i}] ";
            for (int j = 0; j < cols; j++)
            {
                result += $" {matrix[i, j]}  ";
            }
            result += "\n";
        }

        return result;
    }

    // Hàm public để test với ma trận khác
    public void TestMatrix(int[,] customMatrix)
    {
        string output = "Ma trận custom:\n";
        output += PrintMatrix(customMatrix);
        output += "\n" + new string('=', 50) + "\n";
        output += "KẾT QUẢ PHÂN TÍCH CÁC CELL GÓC:\n";
        output += new string('=', 50) + "\n";
        
        output += FindCorners(customMatrix);

        matrixDebugOutput = output;
        Debug.Log(output);
    }

    // Hàm trả về danh sách tọa độ các góc
    public List<Vector2Int> GetCornerPositions(int[,] matrix)
    {
        List<Vector2Int> corners = new List<Vector2Int>();
        int rows = matrix.GetLength(0);
        int cols = matrix.GetLength(1);

        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < cols; j++)
            {
                if (matrix[i, j] == 1 && IsCorner(matrix, i, j))
                {
                    corners.Add(new Vector2Int(j, i)); // x=col, y=row
                }
            }
        }

        return corners;
    }

    bool IsCorner(int[,] matrix, int row, int col)
    {
        int rows = matrix.GetLength(0);
        int cols = matrix.GetLength(1);

        var cornerTypes = new[]
        {
            new[] { (-1, 0), (0, -1), (-1, -1) },
            new[] { (-1, 0), (0, 1), (-1, 1) },
            new[] { (1, 0), (0, -1), (1, -1) },
            new[] { (1, 0), (0, 1), (1, 1) }
        };

        foreach (var dirs in cornerTypes)
        {
            int wallCount = 0;

            foreach (var dir in dirs)
            {
                int newRow = row + dir.Item1;
                int newCol = col + dir.Item2;
                
                if (newRow < 0 || newRow >= rows || newCol < 0 || newCol >= cols || matrix[newRow, newCol] == 0)
                {
                    wallCount++;
                }
            }

            if (wallCount == 3) return true;
        }

        return false;
    }
}
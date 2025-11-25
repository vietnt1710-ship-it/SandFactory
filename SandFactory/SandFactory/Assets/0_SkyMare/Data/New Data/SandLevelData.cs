using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
namespace Data
{
    [CreateAssetMenu(fileName = "LevelData_", menuName = "GameData/SandLevelData")]
    public class SandLevelData : ScriptableObject
    {
        public string[,] grid;
        public List<int> tubes;
        public Storge storge;
        public TextAsset gridDataFile;


        public string[,] TxTToGrid()
        {
            string gridData = gridDataFile.text;

            string[] rows = gridData.Split('\n');

            string[] lends = rows[0].Split(' ');

            string[,] gridS = new string[rows.Length, lends.Length];

            for (int i = 0; i < rows.Length; i++)
            {
                string[] cells = rows[i].Split(' ');
                for (int j = 0; j < cells.Length; j++)
                {
                    // Xử lý từng cell: cells[j]
                    gridS[i, j] = cells[j].Trim();
                    Debug.Log($"Row {i}, Col {j}: {cells[j]}");
                }
            }
#if UNITY_EDITOR
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
#endif
            return gridS;
        }
    }
}

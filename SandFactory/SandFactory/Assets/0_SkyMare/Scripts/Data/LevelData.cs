using System;
using System.Collections.Generic;
using ToolLevel;
using UnityEditor;
using UnityEngine;
namespace Data
{
    [CreateAssetMenu(fileName = "LevelData_", menuName = "GameData/LevelData")]
    public class LevelData : ScriptableObject
    {
        public string[,] grid;
        public Painting painting;
        public Storge storge;
        public TextAsset gridDataFile;


        public string[,] TxTToGrid()
        {
            string gridData = gridDataFile.text;

            string[] rows = gridData.Split('\n');

            string[] lends = rows[0].Split(' ');

            string[,] gridS = new string[rows.Length,lends.Length];  

            for (int i = 0; i < rows.Length; i++)
            {
                string[] cells = rows[i].Split(' ');
                for (int j = 0; j < cells.Length; j++)
                {
                    // Xử lý từng cell: cells[j]
                    gridS[i,j] = cells[j].Trim();
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
    [Serializable]
    public class Storge
    {
        public List<StorgeItem> storgeItems = new List<StorgeItem>();   

        public Storge (StorgeManager data)
        {
            for (int i = 0; i < data.tiles.Count; i++)
            {
                storgeItems.Add(new(data.tiles[i]));
            }
        }
    }
    [Serializable]
    public class StorgeItem
    {
        public int _colorID;
        public int storgeValue;

        public StorgeItem(TileData data)
        {
            this._colorID = data.colorID;
            this.storgeValue = data.storgeValue;
        }
    }
    [Serializable]
    public class Painting
    {
        public Sprite line;
        public Vector3 linePosition;
        public List<string> activeOnStart = new List<string>();
        public List<Piece> pieceList = new List<Piece>();

        public Painting(PaintingData data)
        {
            line = data.line.sprite;
            linePosition = data.line.transform.localPosition;
            for(int i = 0; i < data.enablePiece.Count; i++)
            {
                activeOnStart.Add(data.enablePiece[i].name);
            }
            for (int i = 0; i < data.pieces.Count; i++)
            {
                pieceList.Add(new Piece(data.pieces[i]));
            }

        }

    }
    [Serializable]
    public class Piece
    {
        public string id;
        public List<string> neighbors = new List<string>();
        public int _colorID;
        public int _amount;
        public Vector3 piecePosition;
        public Vector3 textPosition;

        public Sprite raw;
        public Sprite sand;

        public Piece (PieceData data)
        {
            id = data.gameObject.name;
            for(int i = 0; i < data.neighbors.Count;i++)
            {
                if (data.neighbors[i] == null) continue;
                neighbors.Add(data.neighbors[i].gameObject.name);
              
            }
            _colorID = data._colorID;
            _amount = data._amount;

            piecePosition = data.transform.localPosition;
            if(data.infor != null)
            {
                textPosition = data.infor.transform.localPosition;
            }
          

            raw = data.GetComponent<SpriteRenderer>().sprite;
            //sand = data.transform.GetChild(0).GetComponent<SpriteRenderer>().sprite;

        }

    }
}


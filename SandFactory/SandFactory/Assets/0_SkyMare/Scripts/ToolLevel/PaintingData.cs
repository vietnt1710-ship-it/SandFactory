using Data;
using DG.Tweening;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

namespace ToolLevel
{
    public class PaintingData : MonoBehaviour
    {
        public SpriteRenderer line;
        public List<PieceData> enablePiece;
        public List<PieceData> pieces;
        public StorgeManager storgeManager;
        public Button add;
        public Button remove;
        private void Start()
        {
            line = GetComponentInChildren<SpriteRenderer>();
            pieces = GetComponentsInChildren<PieceData>().ToList();
        }

        public void LoadNewPicture(List<SpriteInfor> infors)
        {
            if(line != null) Destroy(line.gameObject);

            for (int i = 0; i < pieces.Count; i++)
            {
                Destroy(pieces[i].gameObject);
            }
            pieces.Clear();
            enablePiece.Clear();

            for (int i = 0; i < infors.Count; i++)
            {
                var ifr = infors[i];
                if (ifr.sprite.name == "Line")
                {
                    GameObject lineObj = new GameObject("Line");
                    line = lineObj.AddComponent<SpriteRenderer>();
                    line.sortingOrder = 40;
                    line.sprite = ifr.sprite;
                    line.gameObject.transform.SetParent(transform, false);
                    line.transform.localPosition = ifr.localPosition;
                }
                else if (ifr.sprite.name.Contains("base"))
                {
                    GameObject gop = new GameObject();

                    var p = gop.AddComponent<SpriteRenderer>();
                    p.sprite = ifr.sprite;
                    p.transform.SetParent(transform, false);
                    p.transform.localPosition = ifr.localPosition;
                    gop.name = ifr.sprite.name;

                    gop.AddComponent<PolygonCollider2D>();

                    var pd = p.AddComponent<PieceData>();

                    pieces.Add(pd);
                }
            }
            DOVirtual.DelayedCall(0.2f, () =>
            {
                GetComponent<NeighborFinder>().Find();
            });
          
        }
        public void LoadPaintingByData(Data.Painting data)
        {
            //if (line != null) Destroy(line.gameObject);
            //for (int i = 0; i < pieces.Count; i++)
            //{
            //    Destroy(pieces[i].gameObject);
            //}
            //pieces.Clear();
            //enablePiece.Clear();

            //GameObject lineObj = new GameObject("Line");
            //line = lineObj.AddComponent<SpriteRenderer>();
            //line.sortingOrder = 5;
            //line.sprite = data.line;
            //line.gameObject.transform.SetParent(transform, false);
            //line.transform.localPosition = data.linePosition;

            for (int i = 0; i < pieces.Count; i++)
            {
                Data.Piece piece = data.pieceList[i];

                //GameObject gop = new GameObject();

                //var p = gop.AddComponent<SpriteRenderer>();
                //p.sprite = piece.raw;
                //p.transform.SetParent(transform, false);
                //p.transform.localPosition = piece.piecePosition;
                //gop.name = piece.id;

                //gop.AddComponent<PolygonCollider2D>();

                //var pd = p.AddComponent<PieceData>();
                var pd = pieces[i];
                if (data.activeOnStart.Contains(piece.id))
                {
                    enablePiece.Add(pd);
                }
                
                pd.id = piece.id;
                //pieces.Add(pd);

                pd.InitInfor(piece._colorID, ToolManager.I.ColorWithID(piece._colorID), piece.textPosition, piece._amount, true);

            }
            LoadNeighBors(data);
            GetAmount();
        }

        public void LoadNeighBors(Data.Painting data)
        {

            for (int i = 0; i < data.pieceList.Count; i++)
            {
                Data.Piece piece = data.pieceList[i];
                PieceData pieceData = pieces[i];
                pieceData.neighbors.Clear();
                for (int j = 0; j < piece.neighbors.Count; j++)
                {
                    pieceData.neighbors.Add(pieces.FirstOrDefault(pi => pi.id == piece.neighbors[j]));
                }

            }
        }
        public void GetAmount()
        {
            _amounts.Clear();
            if (pieces.Count == 0) return;
            for (int i = 0; i < pieces.Count; i++)
            {
                if (!pieces[i].isReady || pieces[i]._amount == 0)
                {
                    Debug.LogError("Bạn cần hoàn thành data của toàn bộ piece mới có thể sử dụng chức năng này");
                    return;
                }
                else
                {
                    AddAmount(pieces[i]._colorID, pieces[i]._amount);
                }

            }
        }
        public void CreateSandBox()
        {
            GetAmount();
            storgeManager.GenData(_amounts);
        }

        private Dictionary<int, int> _amounts = new Dictionary<int, int>();

        public void AddAmount(int colorID, int amount)
        {
            if (_amounts.ContainsKey(colorID))
            {
                _amounts[colorID] += amount;
            }
            else
            {
                _amounts[colorID] = amount;
            }
        }

        public PieceData handling { get; private set; }
        List<PieceData> neightbor = new List<PieceData>();
        public void AddToStart()
        {
            if (handling != null)
            {
                if (!enablePiece.Contains(handling))
                {
                    add.gameObject.SetActive(false);
                    remove.gameObject.SetActive(true);
                    enablePiece.Add(handling);
                    handling.StartTweenEnable();
                }
            }
        }
        public void RemoveToStart()
        {
            if (handling != null)
            {
                if (enablePiece.Contains(handling))
                {
                    add.gameObject.SetActive(true);
                    remove.gameObject.SetActive(false);
                    enablePiece.Remove(handling);
                    handling.StopTweenEnable();
                }
            }


        }
        private void Update()
        {
            if (Input.GetMouseButtonDown(0))
            {
                PieceData piece = ToolManager.I.CastPiece();
                if (piece == null) return;

                Debug.Log($"Piece Data Casting {piece.name}");
                Handler(piece);


            }
        }
        public void Handler(PieceData piece)
        {
            if (handling == null && piece.infor != null)
            {
                neightbor.Clear();
                handling = piece;
                handling.StartTyping(StopPieceHandling);
                StartPieceHandling();

            }
            else if (handling != null && handling != piece)
            {
                if (!neightbor.Contains(piece))
                {
                    neightbor.Add(piece);
                    piece.StartTweenNeighbor();
                }
                else
                {
                    neightbor.Remove(piece);
                    piece.StopTweenNeighbor();
                }

            }
        }
        private void StopPieceHandling()
        {
            add.gameObject.SetActive(false);
            remove.gameObject.SetActive(false);
            handling.neighbors = new List<PieceData>(neightbor);
            for (int i = 0; i < neightbor.Count; i++)
            {
                neightbor[i].StopTweenNeighbor();
            }
            handling.StopTweenEnable();
            handling = null;
        }

        private void StartPieceHandling()
        {
            neightbor = new List<PieceData>(handling.neighbors);
            for (int i = 0; i < neightbor.Count; i++)
            {
                neightbor[i].StartTweenNeighbor();
            }

            if (enablePiece.Contains(handling))
            {
                add.gameObject.SetActive(false);
                remove.gameObject.SetActive(true);
                handling.StartTweenEnable();
            }
            else
            {
                add.gameObject.SetActive(true);
                remove.gameObject.SetActive(false);
            }
        }


    }
}


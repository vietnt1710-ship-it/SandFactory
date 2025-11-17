using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ToolLevel
{
    public class NeighborFinder : MonoBehaviour
    {
        public float epsilon = 0.01f; // Độ sai số chấp nhận

        public void Find()
        {
            var pieces = GetComponentsInChildren<PieceData>();

            foreach (var sr in pieces)
            {
                List<PieceData> neighborPiece = new List<PieceData>();

                var b1 = sr.GetComponent<Collider2D>();

                foreach (var other in pieces)
                {
                    if (other == sr) continue;

                    var b2 = other.GetComponent<Collider2D>();

                    ColliderDistance2D d = Physics2D.Distance(b1, b2);

                    // d.isOverlapped == true  -> đang đè nhau
                    // d.distance == 0         -> chạm cạnh/điểm
                    // d.distance > 0          -> cách nhau một khoảng
                    if (d.isOverlapped || d.distance <= epsilon)
                    {
                        neighborPiece.Add(other);
                    }
                }

                sr.neighbors = neighborPiece;
            }
        }
    }
}


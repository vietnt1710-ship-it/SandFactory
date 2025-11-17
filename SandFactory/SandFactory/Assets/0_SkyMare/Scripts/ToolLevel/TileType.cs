using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using UnityEngine;

namespace ToolLevel
{
    [Serializable]
    public enum Type
    {
        normal, //1
        ice, //3
        hidden, //2
        key, //4
        _lock, //5
        pipe, //6 , 1 up, 2 down, 3 right , 4 left
        _default,
    }
    public class TileType : MonoBehaviour
    {
        public Type type;
        TileType clone;
        public void OnMouseDown()
        {
            clone = Instantiate(this, transform.position, transform.rotation);
            clone.GetComponent<BoxCollider>().enabled = false;
            clone.transform.GetChild(0).gameObject.SetActive(false);
        }

        public void OnMouseDrag()
        {
            Vector3 worldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            worldPos.z = clone.transform.position.z;
            clone.transform.position = worldPos;
        }
        public void OnMouseUp()
        {
            if (clone != null)
            {
                var slot = CastSlot();
                if (slot != null)
                {
                    slot.ChangeType(type);
                    //slot.OnInitialize(this, colorID, color);
                    //Debug.Log($"Complete dat cho {slot.name}");
                    //tiles.Add(slot);
                }
                Destroy(clone.gameObject);
            }
            Debug.Log($"Complete dat cho");

        }
        SlotTile CastSlot()
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
    }
}


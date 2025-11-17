using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace ToolLevel
{
    public class TileData : MonoBehaviour
    {
        public MeshRenderer mesh;
        public TMP_Text amountText;
        public int colorID { get; private set; }
        public Color color { get; private set; }
        public int amount { get; private set; }
        public int storgeValue { get; private set; }

        public List<SlotTile> tiles = new List<SlotTile>();

        public void UpdateAmount()
        {
            amount = storgeValue;
            for (int i = 0; i < tiles.Count; i++)
            {
                if (tiles[i].isPipe)
                {
                    for (int j = 0; j < tiles[i].pipeData.Count; j++)
                    {
                        if(tiles[i].pipeData[j].colorID == colorID)
                        {
                            amount -= tiles[i].pipeData[j].sandAmount;
                            Debug.Log($"- Valu {tiles[i].pipeData.Count} in {j} : {tiles[i].pipeData[j].sandAmount}");
                        }
                       
                    }
                }
                else
                {
                    amount -= tiles[i].data.sandAmount;
                }
            }
            amountText.text = amount.ToString();

            ToolManager.I.storgeManager.CheckCanExport();
        }

        public void SplitForChild(SlotTile slot)
        {
            slot.OnInitialize(this, colorID, color);
        }

        public void Init(int colorID, int amount, Color color)
        {
            this.colorID = colorID;
            this.storgeValue = amount;
            this.color = color;
            amountText.text = amount.ToString();
            mesh.material.color = color;

            UpdateAmount();
        }
        public void UpdateStorge(int amount)
        {
            this.storgeValue = amount;
            UpdateAmount();
        }

        TileData clone;
        public void OnMouseDown()
        {
            if(amount <= 0) return;
            clone = Instantiate(this, transform.position, transform.rotation);
            clone.amountText.text = "";
            clone.GetComponent<BoxCollider>().enabled = false;
        }

        public void OnMouseDrag()
        {
            Vector3 worldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            worldPos.z = clone.transform.position.z;
            clone.transform.position = worldPos;
        }
        public void OnMouseUp()
        {
            if(clone != null )
            {
                var slot = ToolManager.I.CastSlot();
                if(slot != null )
                {
                    slot.OnInitialize(this, colorID, color);
                    Debug.Log($"Complete dat cho {slot.name}");
                    if(!tiles.Contains(slot)) tiles.Add(slot);

                    UpdateAmount();
                }
                var slotPipe = ToolManager.I.CastSlotPipe();
                if (slotPipe != null)
                {
                    slotPipe.UpdateData( this, colorID, color);
                    UpdateAmount();
                }


                Destroy(clone.gameObject);
            }
            Debug.Log($"Complete dat cho");
          
        }
      

    }
}


using DG.Tweening;
using TMPro;
using UnityEngine;

namespace ToolLevel
{
    public class SlotTilePipe : Slot
    {
        public GameObject jar;
        public MeshRenderer mainMesh;
        public TMP_Text sandAmountTxt;
        public KeyboardNumberInput input;
        [HideInInspector] public SlotTile pipe;
        
        public void Open(SlotData data, SlotTile pipe)
        {
            this.pipe = pipe;
            this.data = data;
            sandAmountTxt.text = $"{data.sandAmount}";
            mainMesh.material.color = data.color;
            gameObject.SetActive(true);

        }
        public void UpdateData(TileData parent,int colorID, Color color)
        {
            var lastParent = data.parent;

            this.data.parent = parent;
            this.data.colorID = colorID;
            this.data.color = color;
            this.data.sandAmount = 1;

            sandAmountTxt.text = $"{data.sandAmount}";
            mainMesh.material.color = data.color;

            lastParent.UpdateAmount();
            parent.UpdateAmount();
            pipe.UpdateValue();
        }
        public void Close()
        {
            fading.Kill();
            mainMesh.material.DOColor(data.color, 0);
            gameObject.SetActive(false);
        }

        public void Update()
        {

            if (!isEditing) return;
            if (Input.GetKeyDown(KeyCode.Delete))
            {
                ClearJar();
            }
        }
        void ClearJar()
        {
            pipe.pipeData.Remove(data);
            data.parent.UpdateAmount();

            OnInputSubmit(input.displayText.text);
            fading.Kill();
            mainMesh.material.DOColor(data.color, 0);
            this.gameObject.SetActive(false);
           
        }
        public void OnInputSubmit(string text)
        {
            int amount = 0;
            if (int.TryParse(text, out amount))
                data.sandAmount = amount;
            sandAmountTxt.text = amount.ToString();
            data.parent.UpdateAmount();
            pipe.UpdateValue();
        }
        Tween fading;
        bool isEditing;
        SlotTile currentLock;
 
        public void OnMouseUp()
        {
            isEditing = true;
            EditAmount();

        }
        void EditAmount()
        {
            if (!input.isInputting)
            {
                input.StartTyping(() =>
                {
                    OnInputSubmit(input.displayText.text);
                    fading.Kill();
                    mainMesh.material.DOColor(data.color, 0);
                    isEditing = false;
                });
                fading = mainMesh.material.DOColor(data.color * 1.25f, 0.5f).SetLoops(-1, LoopType.Yoyo).SetEase(Ease.OutQuad);

            }
        }

        public void StartTwenColor()
        {
            fading = mainMesh.material.DOColor(data.color * 1.4f, 0.5f).SetLoops(-1, LoopType.Yoyo).SetEase(Ease.OutQuad);
        }

        public void StopTwenColor()
        {
            fading.Kill();
            mainMesh.material.DOColor(data.color, 0);
        }

       
    }

}


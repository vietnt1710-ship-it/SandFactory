using DG.Tweening;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace ToolLevel
{
    public class PieceData : MonoBehaviour
    {
        public string id { get; set; }
        public List<PieceData> neighbors = new List<PieceData>();
        public bool isReady { get;  set; } = false;
        SpriteRenderer render;
        public PaintingInfor infor;
        public int _colorID { get;  set; }
        public int _amount { get;  set; }

        public void InitInfor(int ColorID, Color color, Vector3 position,int amount = 0, bool local = false)
        {
            if(this.infor == null)
            {
                isReady = true;
                //this.infor = Instantiate(ToolManager.I.paintingInfor);
                this.infor.transform.SetParent(transform);
                if (local)
                {
                    this.infor.transform.localPosition = position;
                }
                else
                {
                    this.infor.transform.position = position;
                }
               
               
                this.infor.transform.localScale = Vector3.one;
            }
            else
            {
                if (local)
                {
                    this.infor.transform.localPosition = position;
                }
                else
                {
                    this.infor.transform.position = position;
                }
            }
            _amount = amount;
            this._colorID = ColorID;
            this.infor.color.color = color;
            this.infor.amount.text = $"{_amount}";

            //StartTyping();
        }

        void Start()
        {
            render = GetComponent<SpriteRenderer>();
        }

        public void OnInputSubmit(string text)
        {
            int amount = 0;
            if(int.TryParse(text, out amount))
            _amount = amount;
            infor.amount.text = _amount.ToString();
        }
        Tween fading;
        Tween cloringRed;
        Tween cloringGreen;
        public void StartTyping(Action endTyping = null)
        {
            Debug.Log("Đã nhấn vào Piece");
            if (infor != null && !infor.input.isInputting)
            {
                infor.input.StartTyping(() =>
                {
                    OnInputSubmit(infor.input.displayText.text);
                    StopNormalTweenColor();
                    endTyping?.Invoke();
                });
                StartNormalTweenColor();
            }
        }

        void StartNormalTweenColor()
        {
            fading = render.DOFade(0.5f, 0.5f).SetLoops(-1, LoopType.Yoyo).SetEase(Ease.OutBack);
        }
        void StopNormalTweenColor()
        {
            fading.Kill();
            render.DOFade(1, 0);
        }
        public void StartTweenNeighbor()
        {
            cloringRed = render.DOColor(Color.red, 0.5f).SetLoops(-1, LoopType.Yoyo).SetEase(Ease.OutQuad);
        }

        public void StopTweenNeighbor()
        {
            cloringRed.Kill();
            render.DOColor(Color.white, 0);
        }

        public void StartTweenEnable()
        {
            cloringGreen = render.DOColor(Color.green, 0.5f).SetLoops(-1, LoopType.Yoyo).SetEase(Ease.OutQuad);
        }

        public void StopTweenEnable()
        {
            cloringGreen.Kill();
            render.DOColor(Color.white, 0);
        }
    }
}


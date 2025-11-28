using Data;
using DG.Tweening;
using System.Collections.Generic;
using System.ComponentModel;
using TMPro;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;

namespace ToolLevel
{
    [System.Serializable]
    public class SlotData
    {
        public int sandAmount;

        public int colorID { get; set; }

        public Color color { get; set; }

        public TileData parent;

        public SlotData(int sandAmount, int colorID, Color color)
        {
            this.sandAmount = sandAmount;
            this.colorID = colorID;
            this.color = color;
        }
    }
    public class Slot : MonoBehaviour
    {
        public SlotData data = new SlotData(0, 0, Color.cyan);
    }
    public class SlotTile : Slot
    {
        public int row, col;
        public string value = "0";
        string tileID = "1";
        [HideInInspector] public string keylockID = "1";
        string pipeDirection = "1";
        public Type type = Type._default;


        public GameObject jar;
        public MeshRenderer mainMesh;
        public TMP_Text sandAmountTxt;
        public GameObject ice;
        public GameObject hidden;
        public GameObject gara;
        public KeyboardNumberInput input;
        public GameObject key;
        public GameObject _lock;

        [HideInInspector] public bool isPipe = false;
        // dữ liệu chứa bên trong pipe
        [HideInInspector] public List<SlotData> pipeData = new List<SlotData>();

        public void OnInitialize(TileData tileData, int colorID, Color color)
        {
            if (isPipe)
            {
                SlotData data = new SlotData(1, colorID, color);
                data.parent = tileData;
                pipeData.Add(data);
                if (isOpenPipe)
                {
                    int lastIndex = pipeData.Count - 1;
                    ToolManager.I.slotTilePipes[lastIndex].Open(pipeData[lastIndex], this);
                }
                UpdateValue();
            }
            else
            {
                if (data.parent == null)
                {
                    this.data.parent = tileData;
                    jar.gameObject.SetActive(true);
                    data.sandAmount = 1;
                    this.type = Type.normal;
                }
                else
                {
                    this.data.parent.tiles.Remove(this);
                    data.parent.UpdateAmount();

                    //update parent mới
                    this.data.parent = tileData;
                }

                UpdateValue();
                data.colorID = colorID;
                data.sandAmount = 1;
                mainMesh.material.color = color;
                data.color = color;
                input.ClearInput();
                sandAmountTxt.text = $"{data.sandAmount}";
            }


        }
        public void ChangeType(Type type)
        {
            if(this.type == Type.key)
            {
                ToolManager.I.ReMoveKey(this);
            }


            if (isOpenPipe) return;

            StopAllTween();

            this.type = type;
            switch (type)
            {
                case Type.normal: 
                    tileID = "1";
                    Invative(true, false, false, false, false);
                    break;
                case Type.hidden:
                    Invative(true, false, false, false, true);
                    tileID = "2"; 
                    break;
                case Type.ice: 
                    tileID = "3";
                    Invative(true, true, false, false, false);
                    break;
                case Type.key:
                    Invative(true, false, true, false, false);
                    tileID = "4"; break;
                case Type._lock:
                    Invative(true, false, false, true, false);
                    tileID = "5"; break;
                case Type.pipe:
                    Invative(false, false, false, false, false, true);
                    if (data.parent != null)
                    {
                        if (data.parent.tiles.Contains(this))
                        {
                            data.parent.tiles.Remove(this);
                            data.parent.UpdateAmount();
                        }
                    }
                    tileID = "6"; break;

            }
           
            if (type != Type.pipe)
            {
                for (int i = 0; i < pipeData.Count; i++)
                {
                    if (pipeData[i].parent.tiles.Contains(this))
                    {
                        this.pipeData[i].parent.tiles.Remove(this);
                    }
                    pipeData[i].parent.UpdateAmount();
                }
                pipeData.Clear();
            }
            UpdateValue();

            //if (data.parent != null) data.parent.UpdateAmount();
 
        }
        private void Invative(bool isMain, bool isFrezze, bool isKey, bool isLockk, bool isHidden, bool isPipe = false)
        {
            this.isPipe = isPipe;

            if (_lock.gameObject.activeInHierarchy)
            {
                ToolManager.I.ReMoveKeyLock(this);
            }
            mainMesh.gameObject.SetActive(isMain);
            ice.SetActive(isFrezze);
            key.SetActive(isKey);
            _lock.SetActive(isLockk);
            hidden.SetActive(isHidden);
            gara.SetActive(isPipe);

        }
        public void Update()
        {
            if (isOpenPipe)
            {

                if (Input.GetKeyDown(KeyCode.UpArrow))
                {
                    ChangePipeDirection(1);
                }

                if (Input.GetKeyDown(KeyCode.DownArrow))
                {
                    ChangePipeDirection(2);
                }

                if (Input.GetKeyDown(KeyCode.RightArrow))
                {
                    ChangePipeDirection(3);
                }

                if (Input.GetKeyDown(KeyCode.LeftArrow))
                {
                    ChangePipeDirection(4);
                }

            }
            if (!isEditing) return;
            if (Input.GetKeyDown(KeyCode.Delete))
            {
                ClearJar();
            }
        }

        void ChangePipeDirection(int dirIdx)
        {
            pipeDirection = dirIdx.ToString();
            UpdateValue();
            switch (dirIdx)
            {
                case 1: gara.transform.localEulerAngles = new Vector3(0, 0, 0); break; // up
                case 2: gara.transform.localEulerAngles = new Vector3(0, 0, 180); break; // down
                case 3: gara.transform.localEulerAngles = new Vector3(0, 0, -90); break; // right
                case 4: gara.transform.localEulerAngles = new Vector3(0, 0, 90); break; //left

            }
        }

        void ClearJar()
        {
            type = Type._default;
            value = "0";
            if (data.parent != null)
            {
                if (key.activeInHierarchy)
                {
                    ToolManager.I.ReMoveKey(this);
                }
                this.data.parent.tiles.Remove(this);

                OnInputSubmit(input.displayText.text);
                fading.Kill();
                mainMesh.material.DOColor(data.color, 0);


                jar.gameObject.SetActive(false);
                this.data.parent = null;

            }

        }
        public void OnInputSubmit(string text)
        {
            int amount = 0;
            if (int.TryParse(text, out amount))
                data.sandAmount = amount;
            sandAmountTxt.text = amount.ToString();
            this.data.sandAmount = amount;

            data.parent.UpdateAmount();
            UpdateValue();
        }

        public void UpdateValue()
        {
            if (type == Type.pipe)
            {
                value = $"{tileID}{pipeDirection}";
                for (int i = 0; i < pipeData.Count; i++)
                {
                    string head = i == 0 ? "_" : "+";
                    value += $"{head}{pipeData[i].colorID}_{pipeData[i].sandAmount}";
                }
            }
            else if (type == Type._default || data.parent == null)
            {
                value = "0";
            }
            else if (type != Type.pipe && type != Type._lock && type != Type.key)
            {
                value = $"{tileID}_{data.colorID}_{data.sandAmount}";
            }

            else if (type == Type.key)
            {
                int keylockID = ToolManager.I.KeyLockID(this);
                if (keylockID == -1)
                {
                    value = $"{1}_{data.colorID}_{data.sandAmount}";
                }
                else
                {
                    value = $"{tileID}{keylockID}_{data.colorID}_{data.sandAmount}";
                }

            }
            else if (type == Type._lock)
            {
                int keylockID = ToolManager.I.LockKeyID(this);
                if (keylockID == -1)
                {
                    value = $"{1}_{data.colorID}_{data.sandAmount}";
                }
                else
                {
                    value = $"{tileID}{keylockID}_{data.colorID}_{data.sandAmount}";
                }

            }
        }

        bool isEditing;
        SlotTile currentLock;
        bool isOpenPipe = false;
        public void OnMouseDown()
        {
            if (!isPipe) return;

            if (!isOpenPipe)
            {
                StartTwenScale();
                isOpenPipe = true;
                for (int i = 0; i < pipeData.Count; i++)
                {
                    ToolManager.I.slotTilePipes[i].Open(pipeData[i], this);
                }
            }
            else
            {
                StopTwenScale();
                isOpenPipe = false;
                for (int i = 0; i < pipeData.Count; i++)
                {
                    ToolManager.I.slotTilePipes[i].Close();
                }

            }
        }
        public void OnMouseUp()
        {
            if (cloneLock != null)
            {
                EditKey();
                return;
            }

            isEditing = true;
            EditAmount();
        }
        void EditKey()
        {
            if (currentLock != null)
            {
                currentLock.StopTwenColor();
                StopTwenColor();
            }

            var slot = ToolManager.I.CastSlot();

            if (slot != null && slot != this)
            {
                if (currentLock != slot && currentLock != null)
                {
                    currentLock.ChangeType(Type.normal);
                }

                slot.ChangeType(Type._lock);
                ToolManager.I.SubcriseKeyLock(this, slot);

            }
            Destroy(cloneLock.gameObject);
        }
        void EditAmount()
        {
            if (data.parent != null && !input.isInputting)
            {
                input.StartTyping(() =>
                {
                    OnInputSubmit(input.displayText.text);
                    fading.Kill();
                    StopTwenColor();
                });
                StartTwenColor();

            }
        }
        GameObject cloneLock;

        private void OnMouseDrag()
        {
            if (Input.GetKeyDown(KeyCode.K) && key.activeInHierarchy)
            {
                cloneLock = Instantiate(_lock);
                cloneLock.gameObject.SetActive(true);

                currentLock = ToolManager.I.GetLock(this);
                if (currentLock != null)
                {
                    currentLock.StartTwenColor();
                    StartTwenColor();
                }
            }

            if (cloneLock != null)
            {
                Vector3 worldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                worldPos.z = cloneLock.transform.position.z;
                cloneLock.transform.position = worldPos;
            }
        }
        Tween fading;
        Tween scaling;
        public void StartTwenColor()
        {
            fading = mainMesh.material.DOColor(data.color * 1.4f, 0.5f).SetLoops(-1, LoopType.Yoyo).SetEase(Ease.OutQuad);
        }
        public void StartTwenScale()
        {
            scaling = gara.transform.DOScale(1.3f, 0.5f).SetLoops(-1, LoopType.Yoyo).SetEase(Ease.OutBack);
        }
        public void StopTwenScale()
        {
            scaling.Kill();
            gara.transform.DOScale(1, 0);
        }
        public void StopTwenColor()
        {
            fading.Kill();
            mainMesh.material.DOColor(data.color, 0);
        }
        public void StopAllTween()
        {
            StopTwenColor(); StopTwenScale();
        }

        public void ReLoad(string idx)
        {
            type = Type._default;
            value = "0";
            jar.gameObject.SetActive(false);
            gara.SetActive(false);

            Debug.Log($"Id of tile{idx}");

            if (idx == "0") return;
            int typeid = GridParse.TypeOf(idx);
            // tách be/af 1 lần, dùng lại cho các case
            GridParse.OnSplitBeAf(idx, out string be, out string af);

            if (typeid == 6)
            {
                LoadPipe(be, af);
            }
            else
            {
                LoadNormalTile(typeid, be, af);
            }


        }
        public void Clear()
        {
            isPipe = false;
            pipeData.Clear();                     
            type = Type._default;
            tileID = "1";
            value = "0";
            jar.gameObject.SetActive(false);
            mainMesh.gameObject.SetActive(true);
            gara.SetActive(false);
        }
        void LoadPipe(string be, string af)
        {
            type = Type.pipe;
            tileID = "6";

            Invative(false, false, false, false, false, true);

            Debug.Log($"Pipe data: {be}, {af}");

            var listIm = GridParse.OnSplitPipe(af);
            pipeData.Clear();

            for (int i = 0; i < listIm.Count; i++)
            {
                Debug.Log($"Pipe data {i}: {listIm[i].colorID}, {listIm[i].sandAmount}");
                pipeData.Add(new SlotData(listIm[i].sandAmount, listIm[i].colorID, ToolManager.I.ColorWithID(listIm[i].colorID)));
            }

            ChangePipeDirection(int.Parse(be[1].ToString()));
        }
        void LoadNormalTile(int type, string be, string af)
        {
            Debug.Log($"Normal data: {be}, {af}");
            jar.gameObject.SetActive(true);

            switch (type)
            {
                case 1: this.type = Type.normal; tileID = "1" ; Invative(true, false, false, false, false); break;
                case 2: this.type = Type.hidden; tileID = "2"; Invative(true, false, false, false, true); break;
                case 3: this.type = Type.ice; tileID = "3"; Invative(true, true, false, false, false); break;
                case 4: this.type = Type.key; tileID = "4"; keylockID = be[1].ToString();ToolManager.I.prepareSlot.Add(this) ; Invative(true, false, true, false, false); break;
                case 5: this.type = Type._lock; tileID = "5"; keylockID = be[1].ToString(); ToolManager.I.prepareSlot.Add(this); Invative(true, false, false, true, false); break;
            }
            (int colorID, int sandAmount) im = GridParse.OnSplitID(af);

            data.colorID = im.colorID;
            data.sandAmount = im.sandAmount;
            sandAmountTxt.text = data.sandAmount.ToString();
            data.color = ToolManager.I.ColorWithID(im.colorID);
            mainMesh.material.color = data.color;

        }
    }
}


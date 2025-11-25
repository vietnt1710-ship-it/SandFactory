using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ToolLevel
{
    public class ColorToolLevel : MonoBehaviour
    {
        public Button prefabsButton;
        public ColorID colorIDs;
        public int selectID;
        public Color selectColor;
        public Image selectExample;
        public Sprite hiddenSprite;
        public TubeToolLevel tubes;

        [Header("Canvas Settings")]
        [SerializeField] private Canvas canvas;
        [SerializeField] private GraphicRaycaster raycaster;

        [Header("Layer Settings")]
        [SerializeField] private string targetLayerName = "UIPIPE";
        private int targetLayer;

        private EventSystem eventSystem;
        private float lastClickTime;
        private const float doubleClickThreshold = 0.3f;
        private bool isDrag = false;
        private Vector2 dragStartPosition;
        private const float dragThreshold = 10f; // Ngưỡng xác định drag

        public void Start()
        {
            ChangeSelect(1);
            InitButton();
            SetupRaycast();
        }

        public void InitButton()
        {
            for (int i = 0; i < colorIDs.colorWithIDs.Count; i++)
            {
                Button newBtn = Instantiate(prefabsButton);
                newBtn.transform.SetParent(this.transform);
                newBtn.transform.localScale = Vector3.one;
                newBtn.gameObject.SetActive(true);

                newBtn.GetComponent<Image>().color = colorIDs.colorWithIDs[i].color;

                int index = i;
                newBtn.onClick.AddListener(() => { ChangeSelect(index + 1); });
            }
        }

        public void ChangeSelect(int colorID)
        {
            selectID = colorID;
            selectColor = colorIDs.colorWithIDs[selectID - 1].color;
            selectExample.color = selectColor;
        }

        void SetupRaycast()
        {
            eventSystem = EventSystem.current;
            if (eventSystem == null)
            {
                GameObject eventSystemObj = new GameObject("EventSystem");
                eventSystem = eventSystemObj.AddComponent<EventSystem>();
                eventSystemObj.AddComponent<StandaloneInputModule>();
            }

            targetLayer = LayerMask.NameToLayer(targetLayerName);
        }

        bool isHidden = false;
        public Image hidden;

        bool isInsert = false;
        void Update()
        {
            if(Input.GetKeyDown(KeyCode.H))
            {
                isHidden = !isHidden;
                hidden.gameObject.SetActive(isHidden);
            }
            if (Input.GetKeyDown(KeyCode.I))
            {
                isInsert = !isInsert;
                Debug.Log("Insert is " +( isInsert ? "ON" : "OFF"));
            }
            if (Input.GetMouseButtonDown(0))
            {
                dragStartPosition = Input.mousePosition;
                isDrag = false;

                // Xử lý click ngay lập tức để tránh độ trễ
                HandleClick(Input.mousePosition, true);
            }
            else if (Input.GetMouseButton(0))
            {
                // Kiểm tra xem có phải là drag không
                if (Vector2.Distance(dragStartPosition, Input.mousePosition) > dragThreshold)
                {
                    isDrag = true;
                }

                if (isDrag)
                {
                    HandleClick(Input.mousePosition, false);
                }
            }
            else if (Input.GetMouseButtonUp(0))
            {
                isDrag = false;
            }
        }

        void HandleClick(Vector2 position, bool isClick)
        {
            PointerEventData pointerEventData = new PointerEventData(eventSystem);
            pointerEventData.position = position;

            List<RaycastResult> results = new List<RaycastResult>();
            raycaster.Raycast(pointerEventData, results);

            foreach (RaycastResult result in results)
            {
                GameObject hitObject = result.gameObject;
                Image image = hitObject.GetComponent<Image>();

                if (image != null && hitObject.layer == targetLayer)
                {
                    if (isClick)
                    {
                        // Kiểm tra double click chỉ khi là click, không phải drag
                        if (Time.time - lastClickTime < doubleClickThreshold)
                        {
                            ImageRemoveHandle(image);
                        }
                        else
                        {
                            ImageHandel(image);
                        }
                        lastClickTime = Time.time;
                    }
                    else
                    {
                        // Drag - chỉ đổi màu, không kiểm tra double click
                        ImageHandel(image);
                    }
                    break;
                }
            }
        }
        public void ImageHandel(Image image)
        {
            image.color = selectColor;
            if (isHidden)
            {
                image.sprite = hiddenSprite;
                image.name = (-selectID).ToString();
            }
            else
            {
                image.sprite = null;
                image.name = selectID.ToString();
            }
        }
        public void ImageRemoveHandle(Image image)
        {
            // Double click - reset màu
            image.color = new Color(55f / 255f, 55f / 255f, 55f / 255f);
            image.sprite = null;
            image.name = "None";
        }
    }
}
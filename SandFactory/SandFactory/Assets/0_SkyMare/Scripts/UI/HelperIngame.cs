using data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
public class HelperIngame : MonoBehaviour
{
   public ItemID itemID;

    public Image ic;
    public TMP_Text count;
    public Image pul;

    public Item item { get; private set; }

    public Button btn_Helper;
    public void Start()
    {
        item = GameManger.I.datas.items.GetItemByID(itemID);
        ic.sprite = item.itemSprite;
        ic.SetNativeSize();
        RefreshUI(0);
        item.OnItemChanged += RefreshUI;

        btn_Helper.onClick.AddListener(HelperAction);
    }

    public void HelperAction()
    {
        Debug.Log($"Helper {itemID} on click");
        if (item.Value > 0)
        {
            item.Value = -1;
        }
        else
        {
            for (int i = 0; i < PopUpManger.I.popups.Count; i++)
            {
                if(PopUpManger.I.popups[i] is PopUpBoster ps)
                {
                    ps.OpenBoster(item);
                }
            }
            //OpenPopUp;
        }
    }
    public void RefreshUI(int count)
    {
        this.count.text = item.Value.ToString();
        if(item.Value <= 0)
        {
            this.count.gameObject.SetActive(false);
            pul.gameObject.SetActive(true);
        }
        else
        {
            this.count.gameObject.SetActive(true);
            pul.gameObject.SetActive(false);
        }
    }
}

using TMPro;
using UnityEngine;

public class MenuLevelItem : MonoBehaviour
{
    public GameObject level_on;
    public GameObject level_off;
    public TMP_Text txt_level_on;
    public TMP_Text txt_level_off;

    public void Init(int level, bool On)
    {
        txt_level_on.text = $"{level}";
        txt_level_off.text = $"{level}";

        level_on.gameObject.SetActive(On);
        level_off.gameObject.SetActive(!On);
    }
}


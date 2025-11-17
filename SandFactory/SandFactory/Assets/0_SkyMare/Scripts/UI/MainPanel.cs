using DG.Tweening;
using UnityEngine;

public class MainPanel : MonoBehaviour
{
    public enum Dir
    {
        center,
        right, // left to right
        left // right to left
    }
    public RectTransform mainPanel;
    public float power = 150;
    public void Close()
    {
        mainPanel.gameObject.SetActive(false);
    }

    public void Open(Dir dir)
    {
        mainPanel.gameObject.SetActive(true);
        Debug.Log($"Main tween {dir}");
        switch(dir)
        {
            case Dir.right:
                mainPanel.DOAnchorPos(new Vector2(0, 0), 0.2f).From(new Vector2(-power, 0)).SetEase(Ease.OutBack , 0.85f);
                break;
            case Dir.left:
                mainPanel.DOAnchorPos(new Vector2(0, 0), 0.2f).From(new Vector2(power, 0)).SetEase(Ease.OutBack, 0.85f);
                break;
        }
    }
}

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

[DefaultExecutionOrder(-100)] // Đảm bảo khởi tạo trước các button
public class ButtonManager : MonoBehaviour
{
    public static ButtonManager I { get; private set; }

    // Từ điển lưu trữ button và trạng thái gốc
    private readonly Dictionary<Button, bool> _originalStates = new Dictionary<Button, bool>();

    // Danh sách các button đã đăng ký
    private readonly HashSet<Button> _registeredButtons = new HashSet<Button>();

    private Sequence _disableSequence;

    void Awake()
    {
        if (I == null)
        {
            I = this;
            DOTween.Init();
            DontDestroyOnLoad(gameObject); // Giữ lại khi load scene mới
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // Đăng ký button với hệ thống quản lý
    public void RegisterButton(Button button)
    {
        if (button == null || _registeredButtons.Contains(button)) return;

        _registeredButtons.Add(button);

        // Đăng ký sự kiện hủy đăng ký khi button bị hủy
        var tracker = button.gameObject.GetComponent<ButtonLifecycleTracker>();
        tracker.OnDestroyed += () => UnregisterButton(button);
    }

    // Hủy đăng ký button
    public void UnregisterButton(Button button)
    {
        if (button == null) return;

        _registeredButtons.Remove(button);

        // Loại bỏ khỏi trạng thái gốc nếu có
        if (_originalStates.ContainsKey(button))
        {
            _originalStates.Remove(button);
        }
    }

    // Tắt tất cả button đã đăng ký trong khoảng thời gian
    public void DisableButtonsForSeconds(float seconds)
    {
        _disableSequence?.Kill();

        // Tạo sequence mới
        _disableSequence = DOTween.Sequence();
        _disableSequence.AppendCallback(DisableAllButtons);
        _disableSequence.AppendInterval(seconds);
        _disableSequence.AppendCallback(EnableAllButtons);
    }

    // Bật lại tất cả button
    public void EnableAllButtons()
    {
        if (_originalStates.Count == 0) return;

        // Duyệt qua bản sao để tránh thay đổi collection trong khi lặp
        foreach (var kvp in new Dictionary<Button, bool>(_originalStates))
        {
            if (kvp.Key != null)
            {
                kvp.Key.enabled = kvp.Value;
            }
        }
        _originalStates.Clear();
    }

    // Tắt tất cả button
    public void DisableAllButtons()
    {
        foreach (var button in _registeredButtons)
        {
            Debug.Log($"Disable button {button.name}");
            if (button == null) continue;

            // Lưu trạng thái gốc nếu chưa lưu
            if (!_originalStates.ContainsKey(button))
            {
                _originalStates.Add(button, button.interactable);
            }

            button.enabled = false;
        }
    }
}

// Script theo dõi vòng đời của button

using data;
using DG.Tweening;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RefileHeartSystem : MonoBehaviour
{
    [Header("Main Referance")]
    public ItemID itemID = ItemID.heart;
    public Item item => GameManger.I.datas.items.GetItemByID(itemID);
    private int maxHearts => ETC_Heart.maxHearts;
    private const float regenTimeMinutes = 30f;

    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI heartText;
    [SerializeField] private TextMeshProUGUI timerText;

    [SerializeField] private Transform ticker;
    [SerializeField] private Transform full;

    public Item itemCoin => GameManger.I.datas.items.GetItemByID(ItemID.coin);
    public Button fillAds;
    public Button fillCoin;

    private int currentHearts;
    private DateTime nextRegenTime;
    private bool isRegenerating = false;

    private const string REGEN_TIME_KEY = "NextRegenTime";

    void Start()
    {
        LoadHeartData();
        CheckRegeneration();
        UpdateUI();
        fillAds.onClick.AddListener(FillBuyAds);
        fillCoin.onClick.AddListener(FillBuyCoin);

        OnHeartChanged();
        item.OnItemChanged += OnHeartChanged;
    }
    public void OnHeartChanged(int value = 0)
    {
        if(value < 0)
        {
            UseHeart();
        }
        if(currentHearts < maxHearts) // trường hợp heart chưa max
        {
            ticker.gameObject.SetActive(true);
            full.gameObject.SetActive(false);
        }
        else // trường hợp heart đã max
        {
            if(value != 0) // value mặc định khi start
            {
                Sequence s = DOTween.Sequence();
                s.Join(ticker.DOScale(0, 0.3f).From(1).SetEase(Ease.InBack).OnComplete(() =>
                {
                    ticker.gameObject.SetActive(false); ticker.DOScale(1, 0);
                }));
                s.Append(full.DOScale(1, 0.3f).From(0).SetEase(Ease.OutBack).OnStart(() =>
                {
                    full.gameObject.SetActive(true);
                }));
            }
            else // value mặc định khi được truyền vào bới event  item.OnItemChanged
            {
                ticker.gameObject.SetActive(false);
                full.gameObject.SetActive(true);

            }

        }
        
    }
    public void FillBuyAds()
    {
        AddOneHeart();
    }
    public void FillBuyCoin()
    {
        itemCoin.Value = -1000;
        AddOneHeart();
    }

    void Update()
    {
        if (isRegenerating && currentHearts < maxHearts)
        {
            CheckRegeneration();
        }
        UpdateTimerUI();
    }

    private void LoadHeartData()
    {
        currentHearts = item.Value;

        string savedTime = PlayerPrefs.GetString(REGEN_TIME_KEY, "");

        if (!string.IsNullOrEmpty(savedTime))
        {
            nextRegenTime = DateTime.Parse(savedTime);
            isRegenerating = true;
        }
        else if (currentHearts < maxHearts)
        {
            StartRegeneration();
        }
    }

    private void SaveHeartData()
    {
        item.Value = currentHearts - item.Value;


        if (isRegenerating)
        {
            PlayerPrefs.SetString(REGEN_TIME_KEY, nextRegenTime.ToString());
        }
        else
        {
            PlayerPrefs.DeleteKey(REGEN_TIME_KEY);
        }

        PlayerPrefs.Save();
    }

    private void CheckRegeneration()
    {
        if (currentHearts >= maxHearts)
        {
            isRegenerating = false;
            SaveHeartData();
            UpdateUI();
            return;
        }

        DateTime now = DateTime.Now;

        if (now >= nextRegenTime)
        {
            // Tính tổng thời gian đã qua từ lần hồi cuối
            TimeSpan timePassed = now - nextRegenTime;
            double totalMinutesPassed = timePassed.TotalMinutes;

            // Tính số heart đã hồi được
            int heartsToRegen = 1 + (int)(totalMinutesPassed / regenTimeMinutes);

            // Tính số phút dư thừa sau khi hồi
            double minutesRemaining = totalMinutesPassed % regenTimeMinutes;

            // Cộng hearts
            currentHearts = Mathf.Min(currentHearts + heartsToRegen, maxHearts);

            if (currentHearts < maxHearts)
            {
                // Set thời gian hồi tiếp theo = (30 phút - số phút dư)
                double nextRegenMinutes = regenTimeMinutes - minutesRemaining;
                nextRegenTime = now.AddMinutes(nextRegenMinutes);
            }
            else
            {
                isRegenerating = false;
            }

            SaveHeartData();
            UpdateUI();
       
        }
    }

    private void StartRegeneration()
    {
        if (currentHearts < maxHearts && !isRegenerating)
        {
            nextRegenTime = DateTime.Now.AddMinutes(regenTimeMinutes);
            isRegenerating = true;
            SaveHeartData();
        }
    }

    private void UpdateUI()
    {
        if (heartText != null)
        {
            heartText.text = currentHearts.ToString();
        }
    }

    private void UpdateTimerUI()
    {
        if (timerText != null)
        {
            if (currentHearts >= maxHearts && heartText.text != "Max")
            {
                timerText.text = "Full";
                heartText.text = "Max";

            }
            else
            {

                timerText.text = GetFormattedTimeRemaining();
            }
        }
    }

    public void UseHeart()
    {
        currentHearts--;

        if (!isRegenerating)
        {
            StartRegeneration();
        }

        UpdateUI();
    }

    public void AddOneHeart()
    {
        if (currentHearts < maxHearts)
        {
            currentHearts++;
            SaveHeartData();
            UpdateUI();

        }
    }

    public TimeSpan GetTimeUntilNextHeart()
    {
        if (!isRegenerating || currentHearts >= maxHearts)
        {
            return TimeSpan.Zero;
        }

        TimeSpan remaining = nextRegenTime - DateTime.Now;
        return remaining.TotalSeconds > 0 ? remaining : TimeSpan.Zero;
    }

    public string GetFormattedTimeRemaining()
    {
        TimeSpan time = GetTimeUntilNextHeart();
        return string.Format("{0:D2}:{1:D2}", (int)time.TotalMinutes, time.Seconds);
    }



}


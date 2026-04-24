using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

// ============================================================
//  StreakManager  –  daily login streak + learning streak
// ============================================================
public class StreakManager : MonoBehaviour
{
    public static StreakManager Instance { get; private set; }

    // ── Events ────────────────────────────────────────────
    public event Action<int>  OnStreakUpdated;    // current streak count
    public event Action       OnStreakBroken;
    public event Action<int>  OnMilestoneReached; // e.g. 7, 30, 100

    // ── Inspector refs ────────────────────────────────────
    [SerializeField] GameObject streakPopupPrefab;
    [SerializeField] Transform  popupParent;

    // ── State ─────────────────────────────────────────────
    public int  CurrentDailyStreak  { get; private set; }
    public int  LongestStreak       { get; private set; }
    public int  TotalWordsLearned   { get; private set; }

    static readonly int[] MilestoneDays = { 3, 7, 14, 30, 60, 100 };

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        LoadData();
        CheckDailyStreak();
    }

    // ── Daily Streak Logic ────────────────────────────────
    void CheckDailyStreak()
    {
        string lastPlayed = PlayerPrefs.GetString("LastPlayedDate", "");
        string today      = DateTime.Now.ToString("yyyy-MM-dd");
        string yesterday  = DateTime.Now.AddDays(-1).ToString("yyyy-MM-dd");

        if (lastPlayed == today)
        {
            // Already played today, streak unchanged
        }
        else if (lastPlayed == yesterday)
        {
            // Consecutive day!
            CurrentDailyStreak++;
            PlayerPrefs.SetInt("DailyStreak", CurrentDailyStreak);
            PlayerPrefs.SetString("LastPlayedDate", today);
            CheckMilestones();
            OnStreakUpdated?.Invoke(CurrentDailyStreak);
            ShowStreakPopup(CurrentDailyStreak);
        }
        else if (lastPlayed != "")
        {
            // Missed a day – reset
            CurrentDailyStreak = 1;
            PlayerPrefs.SetInt("DailyStreak", 1);
            PlayerPrefs.SetString("LastPlayedDate", today);
            OnStreakBroken?.Invoke();
        }
        else
        {
            // First ever play
            CurrentDailyStreak = 1;
            PlayerPrefs.SetInt("DailyStreak", 1);
            PlayerPrefs.SetString("LastPlayedDate", today);
        }

        LongestStreak = Mathf.Max(LongestStreak, CurrentDailyStreak);
        PlayerPrefs.SetInt("LongestStreak", LongestStreak);
        PlayerPrefs.Save();
    }

    void CheckMilestones()
    {
        foreach (int m in MilestoneDays)
            if (CurrentDailyStreak == m)
                OnMilestoneReached?.Invoke(m);
    }

    public void RecordWordLearned()
    {
        TotalWordsLearned++;
        PlayerPrefs.SetInt("TotalWordsLearned", TotalWordsLearned);
        AchievementManager.Instance?.CheckWordCountAchievements(TotalWordsLearned);
    }

    void ShowStreakPopup(int streak)
    {
        if (streakPopupPrefab == null) return;
        var go  = Instantiate(streakPopupPrefab, popupParent);
        var lbl = go.GetComponentInChildren<TextMeshProUGUI>();
        if (lbl != null)
            lbl.text = $"🔥 {streak} Day Streak!\nKeep it up!";
        Destroy(go, 3f);
    }

    void LoadData()
    {
        CurrentDailyStreak = PlayerPrefs.GetInt("DailyStreak", 0);
        LongestStreak      = PlayerPrefs.GetInt("LongestStreak", 0);
        TotalWordsLearned  = PlayerPrefs.GetInt("TotalWordsLearned", 0);
    }
}

// ============================================================
//  AchievementManager  –  tracks & unlocks achievements
// ============================================================
[Serializable]
public class Achievement
{
    public string id;
    public string title;
    public string descriptionEN;
    public string descriptionBN;    // Bangla description
    public string iconName;
    public bool   unlocked;
}

public class AchievementManager : MonoBehaviour
{
    public static AchievementManager Instance { get; private set; }

    public event Action<Achievement> OnAchievementUnlocked;

    [SerializeField] GameObject achievementBannerPrefab;
    [SerializeField] Transform  bannerParent;

    private List<Achievement> _all = new List<Achievement>();

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        InitAchievements();
    }

    void InitAchievements()
    {
        _all = new List<Achievement>
        {
            new Achievement
            {
                id = "first_word", title = "First Word!",
                descriptionEN = "Found your first word.",
                descriptionBN = "আপনার প্রথম শব্দ খুঁজে পেয়েছেন।",
                iconName = "trophy_bronze"
            },
            new Achievement
            {
                id = "words_10", title = "Getting Started",
                descriptionEN = "Learned 10 words.",
                descriptionBN = "১০টি শব্দ শিখেছেন।",
                iconName = "trophy_bronze"
            },
            new Achievement
            {
                id = "words_50", title = "Word Enthusiast",
                descriptionEN = "Learned 50 words.",
                descriptionBN = "৫০টি শব্দ শিখেছেন।",
                iconName = "trophy_silver"
            },
            new Achievement
            {
                id = "words_100", title = "Vocabulary Builder",
                descriptionEN = "Learned 100 words.",
                descriptionBN = "১০০টি শব্দ শিখেছেন।",
                iconName = "trophy_gold"
            },
            new Achievement
            {
                id = "words_500", title = "Vocabulary Master",
                descriptionEN = "Learned 500 words!",
                descriptionBN = "৫০০টি শব্দ শিখেছেন!",
                iconName = "trophy_diamond"
            },
            new Achievement
            {
                id = "streak_7", title = "Week Warrior",
                descriptionEN = "7-day learning streak!",
                descriptionBN = "৭ দিনের শেখার ধারা!",
                iconName = "flame_7"
            },
            new Achievement
            {
                id = "streak_30", title = "Monthly Master",
                descriptionEN = "30-day learning streak!",
                descriptionBN = "৩০ দিনের শেখার ধারা!",
                iconName = "flame_30"
            },
            new Achievement
            {
                id = "speed_5", title = "Speed Typer",
                descriptionEN = "Found 5 words in Speed Mode.",
                descriptionBN = "স্পিড মোডে ৫টি শব্দ খুঁজে পেয়েছেন।",
                iconName = "lightning"
            },
            new Achievement
            {
                id = "perfect_level", title = "Perfect Level",
                descriptionEN = "Completed a level without wrong attempts.",
                descriptionBN = "কোনো ভুল ছাড়াই একটি স্তর সম্পন্ন করেছেন।",
                iconName = "star_perfect"
            }
        };

        // Load unlocked state
        foreach (var a in _all)
            a.unlocked = PlayerPrefs.GetInt($"ach_{a.id}", 0) == 1;
    }

    public void Unlock(string id)
    {
        var ach = _all.Find(a => a.id == id);
        if (ach == null || ach.unlocked) return;
        ach.unlocked = true;
        PlayerPrefs.SetInt($"ach_{id}", 1);
        PlayerPrefs.Save();
        OnAchievementUnlocked?.Invoke(ach);
        ShowBanner(ach);
    }

    public void CheckWordCountAchievements(int totalWords)
    {
        if (totalWords >= 1)   Unlock("first_word");
        if (totalWords >= 10)  Unlock("words_10");
        if (totalWords >= 50)  Unlock("words_50");
        if (totalWords >= 100) Unlock("words_100");
        if (totalWords >= 500) Unlock("words_500");
    }

    public void CheckStreakAchievements(int streak)
    {
        if (streak >= 7)  Unlock("streak_7");
        if (streak >= 30) Unlock("streak_30");
    }

    void ShowBanner(Achievement ach)
    {
        if (achievementBannerPrefab == null) return;
        var go  = Instantiate(achievementBannerPrefab, bannerParent);
        var lbls = go.GetComponentsInChildren<TextMeshProUGUI>();
        if (lbls.Length >= 2)
        {
            lbls[0].text = ach.title;
            lbls[1].text = ach.descriptionBN;
        }
        Destroy(go, 4f);
    }

    public List<Achievement> GetAll() => _all;
}

// ============================================================
//  WordOfTheDay  –  shows the WOTD card on first open each day
// ============================================================
public class WordOfTheDayManager : MonoBehaviour
{
    [SerializeField] GameObject wotdPanel;
    [SerializeField] TextMeshProUGUI wordLabel;
    [SerializeField] TextMeshProUGUI banglaLabel;
    [SerializeField] TextMeshProUGUI phoneticLabel;
    [SerializeField] TextMeshProUGUI categoryLabel;
    [SerializeField] Button speakerBtn;
    [SerializeField] Button closeBtn;

    private string _wotdWord;
    private WordEntry _wotdEntry;

    void Start()
    {
        closeBtn?.onClick.AddListener(() => wotdPanel.SetActive(false));
        speakerBtn?.onClick.AddListener(PlayWOTD);
        StartCoroutine(ShowIfNewDay());
    }

    System.Collections.IEnumerator ShowIfNewDay()
    {
        while (!DictionaryManager.Instance.IsLoaded)
            yield return null;

        string today     = DateTime.Now.ToString("yyyy-MM-dd");
        string lastShown = PlayerPrefs.GetString("WOTDShownDate", "");

        if (lastShown == today) yield break;

        var (word, entry) = DictionaryManager.Instance.GetWordOfTheDay();
        _wotdWord  = word;
        _wotdEntry = entry;

        wordLabel.text      = word.ToUpper();
        banglaLabel.text    = entry.bn;
        phoneticLabel.text  = entry.phonetic;
        categoryLabel.text  = entry.category;

        wotdPanel.SetActive(true);
        AudioManager.Instance?.PlayPronunciation(word, entry.audio);

        PlayerPrefs.SetString("WOTDShownDate", today);
        PlayerPrefs.Save();
    }

    void PlayWOTD()
    {
        if (_wotdWord != null)
            AudioManager.Instance?.PlayPronunciation(_wotdWord, _wotdEntry?.audio);
    }
}

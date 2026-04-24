using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

// ============================================================
//  DailyChallengeMode
//  – One seeded puzzle per day, shared globally
//  – Leaderboard-friendly (same letters for all players)
// ============================================================
public class DailyChallengeMode : MonoBehaviour
{
    [SerializeField] LetterBoardController board;
    [SerializeField] GameController        gameController;
    [SerializeField] TextMeshProUGUI       dateLabel;
    [SerializeField] TextMeshProUGUI       countdownLabel;
    [SerializeField] GameObject            alreadyCompletedPanel;

    void Start()
    {
        string today = DateTime.Now.ToString("yyyy-MM-dd");
        if (dateLabel != null)
            dateLabel.text = DateTime.Now.ToString("MMMM dd, yyyy");

        bool done = PlayerPrefs.GetInt($"DailyDone_{today}", 0) == 1;
        if (done)
        {
            alreadyCompletedPanel?.SetActive(true);
            return;
        }

        StartCoroutine(LoadDailyPuzzle());
        StartCoroutine(CountdownToMidnight());
    }

    IEnumerator LoadDailyPuzzle()
    {
        while (!DictionaryManager.Instance.IsLoaded) yield return null;

        // Deterministic seed: yyyyMMdd
        int seed   = int.Parse(DateTime.Now.ToString("yyyyMMdd"));
        var rng    = new System.Random(seed);

        // Pick 6 letters and 3 target words from seed
        string alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        var    letters  = new List<char>();
        while (letters.Count < 6)
        {
            char c = alphabet[rng.Next(alphabet.Length)];
            if (!letters.Contains(c)) letters.Add(c);
        }

        board.BuildBoard(new string(letters.ToArray()));
        gameController.SetMode(GameMode.Daily);
    }

    IEnumerator CountdownToMidnight()
    {
        while (true)
        {
            var now       = DateTime.Now;
            var midnight  = now.Date.AddDays(1);
            var remaining = midnight - now;

            if (countdownLabel != null)
                countdownLabel.text =
                    $"Next puzzle in {remaining:hh\\:mm\\:ss}";

            yield return new WaitForSeconds(1f);
        }
    }

    public void MarkCompleted()
    {
        string today = DateTime.Now.ToString("yyyy-MM-dd");
        PlayerPrefs.SetInt($"DailyDone_{today}", 1);
        PlayerPrefs.Save();
    }
}

// ============================================================
//  VocabularyMode
//  – Themed category puzzles (animals, food, science…)
//  – Every word shows a definition, phonetic, and Bangla meaning
//  – Spaced-repetition review queue
// ============================================================
public class VocabularyMode : MonoBehaviour
{
    [SerializeField] LetterBoardController board;
    [SerializeField] TextMeshProUGUI       categoryTitle;
    [SerializeField] GameObject            reviewPopupPrefab;
    [SerializeField] Transform             popupParent;

    // Spaced-repetition data: word → next review date
    private Dictionary<string, DateTime> _reviewQueue =
        new Dictionary<string, DateTime>();

    // Available categories (shown as browsable cards in UI)
    private static readonly string[] Categories =
    {
        "animal", "food", "fruit", "nature", "action",
        "adjective", "science", "person", "place", "abstract"
    };

    public void StartCategory(string category)
    {
        if (categoryTitle != null)
            categoryTitle.text = char.ToUpper(category[0]) + category[1..];

        StartCoroutine(BuildCategoryBoard(category));
    }

    IEnumerator BuildCategoryBoard(string category)
    {
        while (!DictionaryManager.Instance.IsLoaded) yield return null;

        var words  = DictionaryManager.Instance.GetWordsByCategory(category);
        Shuffle(words);

        // Build a letter set covering first 3 words
        var letterSet = new HashSet<char>();
        int count = Mathf.Min(3, words.Count);
        for (int i = 0; i < count; i++)
            foreach (char c in words[i].ToUpper())
                letterSet.Add(c);

        // Pad to 7 letters
        string abc = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        var letters = new List<char>(letterSet);
        while (letters.Count < 7)
        {
            char c = abc[UnityEngine.Random.Range(0, abc.Length)];
            if (!letters.Contains(c)) letters.Add(c);
        }
        Shuffle(letters);

        board.BuildBoard(new string(letters.ToArray()));
    }

    public void RecordForReview(string word, bool wasHard)
    {
        // Simple spaced repetition: easy = +3 days, hard = +1 day
        int days = wasHard ? 1 : 3;
        _reviewQueue[word] = DateTime.Now.AddDays(days);
        SaveReviewQueue();
    }

    public List<string> GetDueForReview()
    {
        var due = new List<string>();
        foreach (var kv in _reviewQueue)
            if (kv.Value <= DateTime.Now)
                due.Add(kv.Key);
        return due;
    }

    void SaveReviewQueue()
    {
        foreach (var kv in _reviewQueue)
            PlayerPrefs.SetString($"review_{kv.Key}",
                                   kv.Value.ToString("o"));
        PlayerPrefs.Save();
    }

    // ── Category browser (returns list for UI) ─────────────
    public static string[] GetAllCategories() => Categories;

    static void Shuffle<T>(List<T> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = UnityEngine.Random.Range(0, i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }
}

// ============================================================
//  SpeedModeUI  –  HUD overlay for the 60-second sprint
// ============================================================
public class SpeedModeUI : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI timerDisplay;
    [SerializeField] TextMeshProUGUI comboDisplay;
    [SerializeField] Slider          timerBar;
    [SerializeField] GameObject      resultsPanel;
    [SerializeField] TextMeshProUGUI finalScoreLabel;
    [SerializeField] TextMeshProUGUI wordsFoundLabel;

    float _totalTime = 60f;
    float _remaining;
    bool  _running;
    int   _wordsInSession;
    int   _combo;

    void OnEnable()
    {
        _remaining = _totalTime;
        _running   = true;
        _wordsInSession = 0;
        _combo = 0;
        resultsPanel?.SetActive(false);
        if (timerBar != null) timerBar.maxValue = _totalTime;
    }

    void Update()
    {
        if (!_running) return;
        _remaining -= Time.deltaTime;
        _remaining  = Mathf.Max(0, _remaining);

        if (timerDisplay != null)
        {
            int s = Mathf.CeilToInt(_remaining);
            timerDisplay.text  = $"{s}";
            timerDisplay.color = _remaining <= 10f
                ? Color.red : Color.white;
        }
        if (timerBar != null)
            timerBar.value = _remaining;

        if (_remaining <= 0) EndGame();
    }

    public void OnWordFound(int score)
    {
        _wordsInSession++;
        _combo++;
        if (comboDisplay != null)
        {
            comboDisplay.text = _combo >= 3
                ? $"🔥 {_combo}x COMBO!" : "";
        }
        // Time bonus: +3s per word found
        _remaining = Mathf.Min(_remaining + 3f, _totalTime);
    }

    public void OnWrongWord() => _combo = 0;

    void EndGame()
    {
        _running = false;
        resultsPanel?.SetActive(true);
        if (finalScoreLabel != null)
            finalScoreLabel.text = $"Great run!";
        if (wordsFoundLabel != null)
            wordsFoundLabel.text = $"{_wordsInSession} words found";
    }
}

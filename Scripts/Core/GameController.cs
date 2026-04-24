using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

// ============================================================
//  GameController  –  central game loop orchestrator
//
//  Responsibilities:
//    • Choose letters for a level
//    • Validate words against dictionary
//    • Award points & update progress
//    • Trigger popup, audio, and streaks
//    • Emit events consumed by UI sub-systems
// ============================================================
public class GameController : MonoBehaviour
{
    // ── Inspector refs ────────────────────────────────────
    [Header("Core Systems")]
    [SerializeField] LetterBoardController board;
    [SerializeField] WordFoundPopup        wordPopup;

    [Header("UI")]
    [SerializeField] TextMeshProUGUI scoreLabel;
    [SerializeField] TextMeshProUGUI levelLabel;
    [SerializeField] TextMeshProUGUI streakLabel;
    [SerializeField] Slider          levelProgressBar;
    [SerializeField] GameObject      levelCompletePanel;
    [SerializeField] GameObject      hintButton;

    [Header("Level Data")]
    [SerializeField] LevelDatabase levelDatabase; // ScriptableObject (see below)

    // ── Events ────────────────────────────────────────────
    public event Action<int>    OnScoreChanged;
    public event Action<string> OnWordFoundEvent;   // word string
    public event Action         OnLevelComplete;

    // ── State ─────────────────────────────────────────────
    private int          _score;
    private int          _currentLevel;
    private int          _streakCount;
    private int          _wordsFoundThisLevel;
    private int          _targetWordsThisLevel;
    private HashSet<string> _foundWords    = new HashSet<string>();
    private List<string>    _levelWordPool = new List<string>();   // words to find
    private GameMode         _activeMode   = GameMode.Classic;

    // ── Speed Mode timer ──────────────────────────────────
    private float  _timeRemaining;
    private bool   _timerActive;
    [Header("Speed Mode")]
    [SerializeField] TextMeshProUGUI timerLabel;
    [SerializeField] float           speedModeSeconds = 60f;

    // ══════════════════════════════════════════════════════
    //  UNITY LIFECYCLE
    // ══════════════════════════════════════════════════════

    void Awake()
    {
        board.OnWordFormed    += HandleWordFormed;
        wordPopup.OnPopupClosed += OnPopupClosed;
    }

    void OnDestroy()
    {
        board.OnWordFormed    -= HandleWordFormed;
        wordPopup.OnPopupClosed -= OnPopupClosed;
    }

    void Start()
    {
        LoadProgress();
        StartCoroutine(WaitForDictAndLoad());
    }

    void Update()
    {
        if (_timerActive && _activeMode == GameMode.Speed)
        {
            _timeRemaining -= Time.deltaTime;
            UpdateTimerUI();
            if (_timeRemaining <= 0) EndSpeedMode();
        }
    }

    // ══════════════════════════════════════════════════════
    //  INITIALISATION
    // ══════════════════════════════════════════════════════

    IEnumerator WaitForDictAndLoad()
    {
        while (!DictionaryManager.Instance.IsLoaded)
            yield return null;

        LoadLevel(_currentLevel);
    }

    void LoadLevel(int levelIndex)
    {
        _foundWords.Clear();
        _wordsFoundThisLevel = 0;
        _streakCount         = 0;

        LevelData data = levelDatabase != null
            ? levelDatabase.GetLevel(levelIndex)
            : GenerateLevel(levelIndex);  // procedural fallback

        _levelWordPool         = new List<string>(data.targetWords);
        _targetWordsThisLevel  = _levelWordPool.Count;

        board.BuildBoard(data.letters);

        UpdateUI();
        levelCompletePanel.SetActive(false);

        if (_activeMode == GameMode.Speed)
        {
            _timeRemaining = speedModeSeconds;
            _timerActive   = true;
        }

        Debug.Log($"[Game] Level {levelIndex} loaded  |  " +
                  $"Letters: {data.letters}  |  Words: {_targetWordsThisLevel}");
    }

    // ── Procedural level generation (fallback when no ScriptableObject) ─
    LevelData GenerateLevel(int levelIndex)
    {
        int diff = Mathf.Clamp(1 + levelIndex / 5, 1, 3);
        var pool = DictionaryManager.Instance.GetWordsByDifficulty(diff);
        Shuffle(pool);

        // Pick 3-6 target words
        int count   = Mathf.Clamp(3 + levelIndex / 3, 3, 8);
        var targets = pool.GetRange(0, Mathf.Min(count, pool.Count));

        // Build letter set from all target words
        var letterSet = new HashSet<char>();
        foreach (var w in targets)
            foreach (var c in w.ToUpper())
                letterSet.Add(c);

        // Pad to at least 6 letters
        string alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        var letters     = new List<char>(letterSet);
        while (letters.Count < 6)
        {
            char r = alphabet[UnityEngine.Random.Range(0, alphabet.Length)];
            if (!letters.Contains(r)) letters.Add(r);
        }
        Shuffle(letters);

        return new LevelData
        {
            letters     = new string(letters.ToArray()),
            targetWords = targets
        };
    }

    // ══════════════════════════════════════════════════════
    //  WORD VALIDATION
    // ══════════════════════════════════════════════════════

    void HandleWordFormed(string word)
    {
        word = word.ToLower();

        // Already found?
        if (_foundWords.Contains(word))
        {
            board.AnimateWrongWord();
            AudioManager.Instance?.PlaySFX_Wrong();
            return;
        }

        // In this level's pool?
        bool inLevelPool = _levelWordPool.Contains(word);

        // In dictionary?
        if (!DictionaryManager.Instance.TryGetWord(word, out WordEntry entry))
        {
            board.AnimateWrongWord();
            AudioManager.Instance?.PlaySFX_Wrong();
            _streakCount = 0;
            UpdateStreakUI();
            return;
        }

        // ── CORRECT WORD ───────────────────────────────────
        _foundWords.Add(word);
        _streakCount++;
        int pts = CalculatePoints(word, entry, inLevelPool);
        AddScore(pts);

        board.AnimateCorrectWord();
        AudioManager.Instance?.PlaySFX_WordFound();

        if (_streakCount >= 3)
            AudioManager.Instance?.PlaySFX_Streak();

        wordPopup.ShowWord(word, entry, pts);

        OnWordFoundEvent?.Invoke(word);

        // Track bonus words even if not in pool
        if (inLevelPool)
        {
            _wordsFoundThisLevel++;
            CheckLevelComplete();
        }

        UpdateUI();
        SaveProgress();
    }

    int CalculatePoints(string word, WordEntry entry, bool inPool)
    {
        int  base_pts  = word.Length * 10;
        int  diff_mult = entry.difficulty;
        int  streak    = Mathf.Min(_streakCount, 5); // cap at 5x
        bool bonus     = inPool;

        int total = base_pts * diff_mult + (streak - 1) * 5 + (bonus ? 20 : 5);
        return total;
    }

    void CheckLevelComplete()
    {
        if (_wordsFoundThisLevel < _targetWordsThisLevel) return;

        _currentLevel++;
        SaveProgress();
        AudioManager.Instance?.PlaySFX_LevelComplete();
        StartCoroutine(ShowLevelComplete());
        OnLevelComplete?.Invoke();
    }

    IEnumerator ShowLevelComplete()
    {
        yield return new WaitForSeconds(1f);
        levelCompletePanel.SetActive(true);
    }

    // ══════════════════════════════════════════════════════
    //  GAME MODES
    // ══════════════════════════════════════════════════════

    public void SetMode(GameMode mode)
    {
        _activeMode = mode;
        _timerActive = mode == GameMode.Speed;
    }

    void EndSpeedMode()
    {
        _timerActive = false;
        StartCoroutine(ShowLevelComplete());
    }

    // ══════════════════════════════════════════════════════
    //  HINT SYSTEM
    // ══════════════════════════════════════════════════════

    int _hintsRemaining = 3;

    public void UseHint()
    {
        if (_hintsRemaining <= 0)
        {
            // Trigger rewarded ad
            AdManager.Instance?.ShowRewardedAd(OnHintRewarded);
            return;
        }

        RevealHint();
        _hintsRemaining--;
    }

    void OnHintRewarded() => _hintsRemaining += 2;

    void RevealHint()
    {
        // Find an un-found word from the pool and reveal its first letter
        foreach (var w in _levelWordPool)
        {
            if (!_foundWords.Contains(w))
            {
                // Flash the first tile matching w[0]
                // (board would expose a RevealLetter(char) method)
                Debug.Log($"[Hint] Try a word starting with '{w[0]}'");
                break;
            }
        }
    }

    // ══════════════════════════════════════════════════════
    //  PERSISTENCE
    // ══════════════════════════════════════════════════════

    void LoadProgress()
    {
        _currentLevel = PlayerPrefs.GetInt("CurrentLevel", 0);
        _score        = PlayerPrefs.GetInt("TotalScore", 0);
    }

    void SaveProgress()
    {
        PlayerPrefs.SetInt("CurrentLevel", _currentLevel);
        PlayerPrefs.SetInt("TotalScore", _score);
        PlayerPrefs.Save();
    }

    // ══════════════════════════════════════════════════════
    //  UI
    // ══════════════════════════════════════════════════════

    void UpdateUI()
    {
        if (scoreLabel != null)
            scoreLabel.text = $"{_score:N0}";

        if (levelLabel != null)
            levelLabel.text = $"Level {_currentLevel + 1}";

        if (levelProgressBar != null)
        {
            levelProgressBar.maxValue = _targetWordsThisLevel;
            levelProgressBar.value    = _wordsFoundThisLevel;
        }

        UpdateStreakUI();
    }

    void UpdateStreakUI()
    {
        if (streakLabel == null) return;
        streakLabel.gameObject.SetActive(_streakCount >= 2);
        streakLabel.text = _streakCount >= 2 ? $"🔥 {_streakCount}x Streak!" : "";
    }

    void UpdateTimerUI()
    {
        if (timerLabel == null) return;
        int secs   = Mathf.CeilToInt(_timeRemaining);
        timerLabel.text  = $"{secs / 60:00}:{secs % 60:00}";
        timerLabel.color = _timeRemaining < 10f ? Color.red : Color.white;
    }

    void AddScore(int pts)
    {
        _score += pts;
        OnScoreChanged?.Invoke(_score);

        // Floating score label (optional particle system / tween)
        FloatingScoreManager.Instance?.Spawn($"+{pts}", wordPopup.transform.position);
    }

    void OnPopupClosed() { /* resume input if needed */ }

    // ══════════════════════════════════════════════════════
    //  UTILITIES
    // ══════════════════════════════════════════════════════

    static void Shuffle<T>(List<T> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = UnityEngine.Random.Range(0, i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }

    // ── Next level button (wired from Inspector) ──────────
    public void OnNextLevelPressed()
    {
        AudioManager.Instance?.PlaySFX_Click();
        LoadLevel(_currentLevel);
    }
}

// ============================================================
//  Supporting types
// ============================================================
public enum GameMode { Classic, Daily, Vocabulary, Speed }

[Serializable]
public class LevelData
{
    public string       letters;      // e.g. "ABCDEF"
    public List<string> targetWords;  // words the player must find
}

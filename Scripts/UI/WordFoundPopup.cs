using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

// ============================================================
//  WordFoundPopup  –  animated card shown after a correct word
//
//  Hierarchy expected:
//    WordFoundPopup (this script + CanvasGroup)
//    ├── CardPanel (Image – white card)
//    │   ├── EnglishWord    (TMP)
//    │   ├── PhoneticLabel  (TMP)
//    │   ├── BanglaLabel    (TMP  – use Hind Siliguri / Noto Sans Bengali)
//    │   ├── CategoryBadge  (Image + TMP)
//    │   ├── SpeakerButton  (Button + Image)
//    │   └── ParticleRoot   (empty – spawn confetti here)
//    └── ScoreLabel         (TMP – "+15 pts!")
// ============================================================
public class WordFoundPopup : MonoBehaviour
{
    // ── Inspector refs ────────────────────────────────────
    [Header("Card Elements")]
    [SerializeField] CanvasGroup    canvasGroup;
    [SerializeField] RectTransform  cardPanel;
    [SerializeField] TextMeshProUGUI englishWordLabel;
    [SerializeField] TextMeshProUGUI phoneticLabel;
    [SerializeField] TextMeshProUGUI banglaLabel;
    [SerializeField] TextMeshProUGUI categoryLabel;
    [SerializeField] TextMeshProUGUI scoreLabel;
    [SerializeField] Image          categoryIcon;
    [SerializeField] Button         speakerButton;
    [SerializeField] Button         closeButton;

    [Header("Particle FX")]
    [SerializeField] ParticleSystem confettiParticles;

    [Header("Animation Settings")]
    [SerializeField] float showDuration    = 0.35f;
    [SerializeField] float holdDuration    = 2.8f;     // auto-close delay
    [SerializeField] float hideDuration    = 0.25f;
    [SerializeField] AnimationCurve bounceCurve =
        AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("Category Colours")]
    [SerializeField] Color colAnimal    = new Color(0.29f, 0.76f, 0.48f);
    [SerializeField] Color colAction    = new Color(0.29f, 0.58f, 0.92f);
    [SerializeField] Color colFood      = new Color(0.94f, 0.52f, 0.25f);
    [SerializeField] Color colAbstract  = new Color(0.72f, 0.45f, 0.91f);
    [SerializeField] Color colNature    = new Color(0.35f, 0.72f, 0.60f);
    [SerializeField] Color colDefault   = new Color(0.50f, 0.55f, 0.65f);

    // ── State ─────────────────────────────────────────────
    private string       _currentWord;
    private WordEntry    _currentEntry;
    private Coroutine    _autoCloseRoutine;

    // ── Events ────────────────────────────────────────────
    public event Action OnPopupClosed;

    // ── Unity lifecycle ────────────────────────────────────
    void Awake()
    {
        canvasGroup.alpha          = 0;
        canvasGroup.interactable   = false;
        canvasGroup.blocksRaycasts = false;
        gameObject.SetActive(false);

        speakerButton?.onClick.AddListener(OnSpeakerClicked);
        closeButton?.onClick.AddListener(HidePopup);
    }

    // ════════════════════════════════════════════════════════
    //  PUBLIC API
    // ════════════════════════════════════════════════════════

    /// <summary>Show the popup for a discovered word.</summary>
    public void ShowWord(string word, WordEntry entry, int scoreGained)
    {
        _currentWord  = word;
        _currentEntry = entry;

        // Populate text
        englishWordLabel.text = word.ToUpper();
        phoneticLabel.text    = entry.phonetic ?? "";
        banglaLabel.text      = entry.bn ?? "—";
        categoryLabel.text    = CapFirst(entry.category ?? "word");
        scoreLabel.text       = $"+{scoreGained}";

        // Category colour
        categoryIcon.color = GetCategoryColor(entry.category);

        // Auto-play audio
        AudioManager.Instance?.PlayPronunciation(word, entry.audio);

        // Animate in
        gameObject.SetActive(true);
        if (_autoCloseRoutine != null) StopCoroutine(_autoCloseRoutine);
        StartCoroutine(AnimateIn());
        confettiParticles?.Play();
        _autoCloseRoutine = StartCoroutine(AutoClose());
    }

    // ── Button callbacks ──────────────────────────────────

    void OnSpeakerClicked()
    {
        AudioManager.Instance?.PlayPronunciation(_currentWord, _currentEntry?.audio);
        AudioManager.Instance?.PlaySFX_Click();
        AnimateSpeakerBounce();
    }

    public void HidePopup()
    {
        if (_autoCloseRoutine != null)
        {
            StopCoroutine(_autoCloseRoutine);
            _autoCloseRoutine = null;
        }
        StartCoroutine(AnimateOut());
    }

    // ── Animations ────────────────────────────────────────

    IEnumerator AnimateIn()
    {
        canvasGroup.interactable   = true;
        canvasGroup.blocksRaycasts = true;

        float t = 0f;
        cardPanel.localScale = Vector3.one * 0.6f;

        while (t < showDuration)
        {
            t += Time.deltaTime;
            float p = Mathf.Clamp01(t / showDuration);
            float ease = bounceCurve.Evaluate(p);
            canvasGroup.alpha      = ease;
            cardPanel.localScale   = Vector3.Lerp(
                                         Vector3.one * 0.6f,
                                         Vector3.one * 1.05f, ease);
            yield return null;
        }

        // Slight overshoot settle
        float settle = 0f;
        while (settle < 0.1f)
        {
            settle += Time.deltaTime;
            float p = settle / 0.1f;
            cardPanel.localScale = Vector3.Lerp(
                                       Vector3.one * 1.05f, Vector3.one, p);
            yield return null;
        }

        cardPanel.localScale = Vector3.one;
        canvasGroup.alpha    = 1f;
    }

    IEnumerator AnimateOut()
    {
        canvasGroup.interactable   = false;
        canvasGroup.blocksRaycasts = false;

        float t = 0f;
        while (t < hideDuration)
        {
            t += Time.deltaTime;
            float p = Mathf.Clamp01(t / hideDuration);
            canvasGroup.alpha    = 1f - p;
            cardPanel.localScale = Vector3.Lerp(Vector3.one, Vector3.one * 0.8f, p);
            yield return null;
        }

        canvasGroup.alpha = 0f;
        gameObject.SetActive(false);
        OnPopupClosed?.Invoke();
    }

    IEnumerator AutoClose()
    {
        yield return new WaitForSeconds(holdDuration);
        HidePopup();
    }

    void AnimateSpeakerBounce()
    {
        StopCoroutine(nameof(SpeakerBounce));
        StartCoroutine(SpeakerBounce());
    }

    IEnumerator SpeakerBounce()
    {
        var rt = speakerButton.GetComponent<RectTransform>();
        float t = 0f;
        while (t < 0.3f)
        {
            t += Time.deltaTime;
            float s = 1f + Mathf.Sin(t * 30f) * 0.1f * Mathf.Exp(-t * 8f);
            rt.localScale = Vector3.one * s;
            yield return null;
        }
        rt.localScale = Vector3.one;
    }

    // ── Helpers ───────────────────────────────────────────

    Color GetCategoryColor(string cat)
    {
        return (cat?.ToLower()) switch
        {
            "animal"   => colAnimal,
            "action"   => colAction,
            "food"     => colFood,
            "fruit"    => colFood,
            "abstract" => colAbstract,
            "nature"   => colNature,
            _ => colDefault
        };
    }

    static string CapFirst(string s) =>
        s.Length == 0 ? s : char.ToUpper(s[0]) + s[1..];
}

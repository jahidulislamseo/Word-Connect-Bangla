using System;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

// ============================================================
//  AudioManager  –  pronunciation + SFX + TTS fallback
// ============================================================
public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    // ── Inspector ─────────────────────────────────────────
    [Header("SFX Clips (assign in Inspector)")]
    [SerializeField] AudioClip sfxWordFound;
    [SerializeField] AudioClip sfxWrongWord;
    [SerializeField] AudioClip sfxLevelComplete;
    [SerializeField] AudioClip sfxButtonClick;
    [SerializeField] AudioClip sfxStreakBonus;

    [Header("Pronunciation Source")]
    [SerializeField] AudioSource pronunciationSource;
    [SerializeField] AudioSource sfxSource;

    [Header("TTS Settings")]
    [Tooltip("Enable to fall back to Google TTS when no local audio found")]
    [SerializeField] bool useTTSFallback = true;

    // ── Cache ─────────────────────────────────────────────
    // Local clips loaded from Resources/Audio/Pronunciation/
    private System.Collections.Generic.Dictionary<string, AudioClip>
        _clipCache = new System.Collections.Generic.Dictionary<string,AudioClip>(
            StringComparer.OrdinalIgnoreCase);

    // ── Unity lifecycle ────────────────────────────────────
    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Ensure we have audio sources
        if (pronunciationSource == null)
            pronunciationSource = gameObject.AddComponent<AudioSource>();
        if (sfxSource == null)
        {
            sfxSource = gameObject.AddComponent<AudioSource>();
            sfxSource.playOnAwake = false;
        }
    }

    // ════════════════════════════════════════════════════════
    //  PUBLIC API
    // ════════════════════════════════════════════════════════

    /// <summary>
    /// Play pronunciation for a word.
    /// Tries:  1. local Resources clip  →  2. StreamingAssets mp3
    ///         →  3. Google TTS (online fallback)
    /// </summary>
    public void PlayPronunciation(string word, string audioName = null)
    {
        string key = (audioName ?? word).ToLower();

        // 1. Already cached?
        if (_clipCache.TryGetValue(key, out var cached))
        {
            PlayClip(pronunciationSource, cached);
            return;
        }

        // 2. Resources/Audio/Pronunciation/<key>
        var clip = Resources.Load<AudioClip>($"Audio/Pronunciation/{key}");
        if (clip != null)
        {
            _clipCache[key] = clip;
            PlayClip(pronunciationSource, clip);
            return;
        }

        // 3. StreamingAssets/<key>.mp3
        StartCoroutine(TryStreamingAssets(key, word));
    }

    IEnumerator TryStreamingAssets(string key, string word)
    {
        string path = Path.Combine(Application.streamingAssetsPath, $"Audio/{key}.mp3");
#if UNITY_ANDROID && !UNITY_EDITOR
        path = $"jar:file://{Application.dataPath}!/assets/Audio/{key}.mp3";
#endif
        using var req = UnityWebRequestMultimedia.GetAudioClip(
                            path, AudioType.MPEG);
        yield return req.SendWebRequest();

        if (req.result == UnityWebRequest.Result.Success)
        {
            var clip = DownloadHandlerAudioClip.GetContent(req);
            if (clip != null)
            {
                _clipCache[key] = clip;
                PlayClip(pronunciationSource, clip);
                yield break;
            }
        }

        // 4. TTS fallback
        if (useTTSFallback)
            StartCoroutine(PlayTTS(word));
    }

    // ── Google TTS fallback ────────────────────────────────
    // Uses the free, undocumented endpoint (no API key needed for short words).
    // For production, swap with Google Cloud TTS (requires API key).
    IEnumerator PlayTTS(string word)
    {
        string encoded = UnityEngine.Networking.UnityWebRequest.EscapeURL(word);
        string url     = $"https://translate.google.com/translate_tts" +
                         $"?ie=UTF-8&q={encoded}&tl=en&client=tw-ob";

        using var req = UnityWebRequestMultimedia.GetAudioClip(url, AudioType.MPEG);
        req.SetRequestHeader("User-Agent",
            "Mozilla/5.0 (Linux; Android 10) AppleWebKit/537.36");
        yield return req.SendWebRequest();

        if (req.result == UnityWebRequest.Result.Success)
        {
            var clip = DownloadHandlerAudioClip.GetContent(req);
            if (clip != null)
            {
                PlayClip(pronunciationSource, clip);
                // Cache for the session
                _clipCache[word.ToLower()] = clip;
            }
        }
        else
        {
            // Last resort: Android native TTS via Plugin
            AndroidTTSFallback(word);
        }
    }

    // ── Android native TTS (if plugin available) ──────────
    void AndroidTTSFallback(string word)
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        try
        {
            using var player = new AndroidJavaClass(
                "com.yourcompany.wordconnect.TTSPlayer");
            player.CallStatic("speak", word);
        }
        catch (Exception e)
        {
            Debug.LogWarning($"[Audio] TTS plugin not found: {e.Message}");
        }
#endif
    }

    // ── SFX helpers ───────────────────────────────────────

    public void PlaySFX_WordFound()    => PlayClip(sfxSource, sfxWordFound);
    public void PlaySFX_Wrong()        => PlayClip(sfxSource, sfxWrongWord);
    public void PlaySFX_LevelComplete()=> PlayClip(sfxSource, sfxLevelComplete);
    public void PlaySFX_Click()        => PlayClip(sfxSource, sfxButtonClick);
    public void PlaySFX_Streak()       => PlayClip(sfxSource, sfxStreakBonus);

    public void ReplayLastPronunciation()
    {
        if (pronunciationSource.clip != null)
            PlayClip(pronunciationSource, pronunciationSource.clip);
    }

    // ── Volume control ────────────────────────────────────

    public void SetMasterVolume(float v)
    {
        AudioListener.volume = Mathf.Clamp01(v);
        PlayerPrefs.SetFloat("MasterVolume", v);
    }

    public void SetSFXMuted(bool muted) =>
        sfxSource.mute = muted;

    // ── Internal helper ────────────────────────────────────
    void PlayClip(AudioSource src, AudioClip clip)
    {
        if (clip == null || src == null) return;
        src.clip = clip;
        src.Play();
    }

    void OnEnable()
    {
        float saved = PlayerPrefs.GetFloat("MasterVolume", 1f);
        AudioListener.volume = saved;
    }
}

using System.Collections;
using UnityEngine;
using UnityEngine.UI;

// ============================================================
//  PerformanceManager  –  adaptive quality for low-end devices
//
//  Detects RAM / GPU tier and adjusts:
//    • Target frame-rate
//    • Particle quality
//    • Shadow quality
//    • UI canvas scale
//    • Object pooling
// ============================================================
public class PerformanceManager : MonoBehaviour
{
    public static PerformanceManager Instance { get; private set; }

    // ── Inspector ─────────────────────────────────────────
    [Header("Particle Systems to Scale")]
    [SerializeField] ParticleSystem[] scalableParticles;

    [Header("Canvas Scaler to Adjust")]
    [SerializeField] CanvasScaler mainCanvasScaler;

    // ── Device tiers ──────────────────────────────────────
    public enum DeviceTier { Low, Mid, High }
    public DeviceTier CurrentTier { get; private set; }

    // ── Lifecycle ─────────────────────────────────────────
    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        DetectAndApply();
    }

    // ── Detection ─────────────────────────────────────────
    void DetectAndApply()
    {
        CurrentTier = ClassifyDevice();
        Debug.Log($"[Perf] Device tier: {CurrentTier}");
        ApplySettings(CurrentTier);
    }

    DeviceTier ClassifyDevice()
    {
        // SystemInfo gives us RAM in MB
        int ramMB = SystemInfo.systemMemorySize;

        // GPU tier from Unity's Graphics.activeTier (0=Low, 1=Mid, 2=High)
        int gpuTier = (int)UnityEngine.Rendering.Graphics.activeTier;

        // CPU core count
        int cores = SystemInfo.processorCount;

        // Scoring
        int score = 0;
        if (ramMB >= 3000) score += 2;
        else if (ramMB >= 1500) score += 1;

        score += gpuTier;               // 0, 1, or 2

        if (cores >= 6) score += 2;
        else if (cores >= 4) score += 1;

        return score >= 4 ? DeviceTier.High
             : score >= 2 ? DeviceTier.Mid
             :               DeviceTier.Low;
    }

    // ── Settings application ──────────────────────────────
    void ApplySettings(DeviceTier tier)
    {
        switch (tier)
        {
            case DeviceTier.Low:
                Application.targetFrameRate = 30;
                QualitySettings.SetQualityLevel(0, true);
                QualitySettings.shadows            = ShadowQuality.Disable;
                QualitySettings.antiAliasing       = 0;
                QualitySettings.vSyncCount         = 0;
                SetParticleQuality(0.3f);
                Screen.SetResolution(
                    (int)(Screen.width  * 0.75f),
                    (int)(Screen.height * 0.75f), true);
                break;

            case DeviceTier.Mid:
                Application.targetFrameRate = 45;
                QualitySettings.SetQualityLevel(2, true);
                QualitySettings.shadows            = ShadowQuality.HardOnly;
                QualitySettings.antiAliasing       = 2;
                SetParticleQuality(0.65f);
                break;

            case DeviceTier.High:
                Application.targetFrameRate = 60;
                QualitySettings.SetQualityLevel(5, true);
                QualitySettings.shadows            = ShadowQuality.All;
                QualitySettings.antiAliasing       = 4;
                SetParticleQuality(1.0f);
                break;
        }

        // Keep GPU from overheating on mobile
        Application.backgroundLoadingPriority =
            tier == DeviceTier.Low
                ? ThreadPriority.Low
                : ThreadPriority.Normal;
    }

    void SetParticleQuality(float fraction)
    {
        if (scalableParticles == null) return;
        foreach (var ps in scalableParticles)
        {
            if (ps == null) continue;
            var main = ps.main;
            main.maxParticles = Mathf.RoundToInt(main.maxParticles * fraction);

            if (fraction < 0.5f)
                ps.gameObject.SetActive(false);  // disable entirely on very low-end
        }
    }

    // ── Object Pooling (generic) ──────────────────────────
    // Simple pool: pre-instantiate N objects, reuse them.

    public static class Pool<T> where T : Component
    {
        static Queue<T> _pool = new Queue<T>();

        public static T Get(T prefab, Transform parent)
        {
            if (_pool.Count > 0)
            {
                var obj = _pool.Dequeue();
                obj.gameObject.SetActive(true);
                obj.transform.SetParent(parent, false);
                return obj;
            }
            return Object.Instantiate(prefab, parent);
        }

        public static void Return(T obj)
        {
            obj.gameObject.SetActive(false);
            _pool.Enqueue(obj);
        }
    }

    // ── FPS Monitor (debug overlay) ───────────────────────
    float _fpsTimer;
    float _fps;

    void Update()
    {
        _fpsTimer += Time.deltaTime;
        if (_fpsTimer >= 1f)
        {
            _fps      = 1f / Time.unscaledDeltaTime;
            _fpsTimer = 0;

            // Auto-downgrade if FPS drops badly
            if (_fps < 20 && CurrentTier != DeviceTier.Low)
            {
                Debug.LogWarning("[Perf] FPS dropped – reducing quality.");
                CurrentTier = DeviceTier.Low;
                ApplySettings(DeviceTier.Low);
            }
        }
    }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
    void OnGUI()
    {
        GUI.Label(new Rect(10, 10, 150, 20),
                  $"FPS: {_fps:F0} | {CurrentTier}");
    }
#endif
}

// ============================================================
//  FloatingScoreManager  –  "+50" labels that float and fade
// ============================================================
public class FloatingScoreManager : MonoBehaviour
{
    public static FloatingScoreManager Instance { get; private set; }

    [SerializeField] TMPro.TextMeshProUGUI floatingLabelPrefab;
    [SerializeField] Canvas               worldCanvas;

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
    }

    public void Spawn(string text, Vector3 worldPos)
    {
        if (floatingLabelPrefab == null || worldCanvas == null) return;
        var lbl = Instantiate(floatingLabelPrefab, worldCanvas.transform);
        lbl.text = text;

        // Convert world → canvas
        Vector2 screen = Camera.main.WorldToScreenPoint(worldPos);
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            worldCanvas.GetComponent<RectTransform>(),
            screen, worldCanvas.worldCamera,
            out Vector2 local);
        lbl.rectTransform.anchoredPosition = local;

        StartCoroutine(FloatUp(lbl));
    }

    IEnumerator FloatUp(TMPro.TextMeshProUGUI lbl)
    {
        float t = 0f, dur = 1.2f;
        Vector2 start = lbl.rectTransform.anchoredPosition;
        while (t < dur)
        {
            t += Time.deltaTime;
            float p = t / dur;
            lbl.rectTransform.anchoredPosition = start + Vector2.up * (80f * p);
            lbl.alpha = 1f - p * p;
            yield return null;
        }
        Destroy(lbl.gameObject);
    }
}

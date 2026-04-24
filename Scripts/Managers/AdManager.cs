using System;
using UnityEngine;

// ============================================================
//  AdManager  –  rewarded ads + premium state
//
//  Integrates with Google AdMob (Unity plugin required).
//  Drop in your real Ad Unit IDs below.
// ============================================================
public class AdManager : MonoBehaviour
{
    public static AdManager Instance { get; private set; }

#if UNITY_ANDROID
    const string REWARDED_AD_ID  = "ca-app-pub-XXXXXXXXXXXXXXXX/XXXXXXXXXX";
    const string BANNER_AD_ID    = "ca-app-pub-XXXXXXXXXXXXXXXX/XXXXXXXXXX";
    const string INTERSTITIAL_ID = "ca-app-pub-XXXXXXXXXXXXXXXX/XXXXXXXXXX";
#else
    // Test IDs for Editor
    const string REWARDED_AD_ID  = "ca-app-pub-3940256099942544/5224354917";
    const string BANNER_AD_ID    = "ca-app-pub-3940256099942544/6300978111";
    const string INTERSTITIAL_ID = "ca-app-pub-3940256099942544/1033173712";
#endif

    // ── State ─────────────────────────────────────────────
    public bool IsPremium { get; private set; }

    private Action _onRewardedEarned;
    private bool   _rewardedLoaded;

    // ── Lifecycle ─────────────────────────────────────────
    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        IsPremium = PlayerPrefs.GetInt("IsPremium", 0) == 1;

        if (!IsPremium)
            InitialiseAds();
    }

    void InitialiseAds()
    {
        // ── AdMob Init ──────────────────────────────────
        // Uncomment when AdMob Unity Plugin is imported:
        //
        // MobileAds.Initialize(status =>
        // {
        //     Debug.Log("[Ads] AdMob initialised.");
        //     LoadRewardedAd();
        //     if (!IsPremium) LoadBanner();
        // });

        Debug.Log("[Ads] AdManager ready (stub mode – import AdMob plugin).");
    }

    // ── Rewarded Ad ───────────────────────────────────────

    public void ShowRewardedAd(Action onRewarded)
    {
        _onRewardedEarned = onRewarded;

        if (IsPremium)
        {
            // Premium users get hints for free
            onRewarded?.Invoke();
            return;
        }

#if UNITY_EDITOR
        // Simulate reward in editor
        Debug.Log("[Ads] Simulated rewarded ad shown.");
        onRewarded?.Invoke();
        return;
#endif

        // Real AdMob call:
        // if (_rewardedAd != null && _rewardedAd.CanShowAd())
        //     _rewardedAd.Show(reward => { _onRewardedEarned?.Invoke(); LoadRewardedAd(); });
        // else
        //     ShowNoAdFallback();
    }

    void ShowNoAdFallback()
    {
        // Show a "No ad available" popup or offer alternative
        Debug.LogWarning("[Ads] No rewarded ad loaded.");
    }

    // ── Banner Ad ─────────────────────────────────────────

    public void ShowBanner()
    {
        if (IsPremium) return;
        // _bannerView?.Show();
    }

    public void HideBanner()
    {
        // _bannerView?.Hide();
    }

    // ── Interstitial ──────────────────────────────────────

    int _levelsSinceInterstitial = 0;

    public void OnLevelComplete()
    {
        if (IsPremium) return;
        _levelsSinceInterstitial++;
        if (_levelsSinceInterstitial >= 3)   // show every 3 levels
        {
            _levelsSinceInterstitial = 0;
            ShowInterstitial();
        }
    }

    void ShowInterstitial()
    {
#if UNITY_EDITOR
        Debug.Log("[Ads] Simulated interstitial.");
        return;
#endif
        // _interstitialAd?.Show();
    }

    // ── Premium unlock ────────────────────────────────────

    /// <summary>Call after successful IAP purchase.</summary>
    public void UnlockPremium()
    {
        IsPremium = true;
        PlayerPrefs.SetInt("IsPremium", 1);
        PlayerPrefs.Save();
        HideBanner();
        Debug.Log("[Ads] Premium unlocked – ads removed.");
    }
}

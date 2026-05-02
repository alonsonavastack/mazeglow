using UnityEngine;
using GoogleMobileAds.Api;
using System;

/// <summary>
/// AdManager — MazeGlow con Google Mobile Ads SDK v11
/// App ID:       ca-app-pub-1637113371666338~5391695579
/// Intersticial: ca-app-pub-1637113371666338/9300074435
/// Recompensado: ca-app-pub-1637113371666338/9872806575
/// </summary>
public class AdManager : MonoBehaviour
{
    public static AdManager Instance { get; private set; }

    // ─── IDs REALES ───────────────────────────────────────────────────────────────
    private const string ADMOB_APP_ID          = "ca-app-pub-1637113371666338~5391695579";
    private const string ADMOB_INTERSTITIAL_ID = "ca-app-pub-1637113371666338/9300074435";
    private const string ADMOB_REWARDED_ID     = "ca-app-pub-1637113371666338/9872806575";

    // ─── IDs de PRUEBA — usar en editor y builds de desarrollo ───────────────────
    private const string TEST_INTERSTITIAL_ID  = "ca-app-pub-3940256099942544/1033173712";
    private const string TEST_REWARDED_ID      = "ca-app-pub-3940256099942544/5224354917";

    // Usar IDs de prueba en el editor, reales en el dispositivo
    private string InterstitialID => Application.isEditor ? TEST_INTERSTITIAL_ID : ADMOB_INTERSTITIAL_ID;
    private string RewardedID     => Application.isEditor ? TEST_REWARDED_ID     : ADMOB_REWARDED_ID;

    private InterstitialAd interstitialAd;
    private RewardedAd     rewardedAd;

    public event Action OnInterstitialClosed;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        MobileAds.Initialize(initStatus =>
        {
            Debug.Log("[AdManager] AdMob inicializado.");
            LoadInterstitial();
            LoadRewarded();
        });
    }

    // ── Intersticial ──────────────────────────────────────────────────────────────
    private void LoadInterstitial()
    {
        var request = new AdRequest();
        InterstitialAd.Load(InterstitialID, request, (ad, error) =>
        {
            if (error != null) { Debug.LogWarning("[AdManager] Intersticial error: " + error); return; }
            interstitialAd = ad;
            Debug.Log("[AdManager] Intersticial cargado.");
        });
    }

    public void ShowInterstitial()
    {
        if (GameManager.Instance?.adsRemoved ?? false)
        {
            OnInterstitialClosed?.Invoke();
            return;
        }

        if (interstitialAd != null && interstitialAd.CanShowAd())
        {
            interstitialAd.OnAdFullScreenContentClosed += () =>
            {
                OnInterstitialClosed?.Invoke();
                LoadInterstitial(); // precargar el siguiente
            };
            interstitialAd.OnAdFullScreenContentFailed += _ =>
            {
                OnInterstitialClosed?.Invoke();
                LoadInterstitial();
            };
            interstitialAd.Show();
        }
        else
        {
            Debug.Log("[AdManager] Intersticial no disponible — continuando.");
            OnInterstitialClosed?.Invoke();
            LoadInterstitial();
        }
    }

    // ── Recompensado ──────────────────────────────────────────────────────────────
    private void LoadRewarded()
    {
        var request = new AdRequest();
        RewardedAd.Load(RewardedID, request, (ad, error) =>
        {
            if (error != null) { Debug.LogWarning("[AdManager] Recompensado error: " + error); return; }
            rewardedAd = ad;
            Debug.Log("[AdManager] Recompensado cargado.");
        });
    }

    public void ShowRewarded(int rewardCoins = 0, int rewardLives = 0)
    {
        if (GameManager.Instance?.adsRemoved ?? false)
        {
            GrantReward(rewardCoins, rewardLives);
            return;
        }

        if (rewardedAd != null && rewardedAd.CanShowAd())
        {
            rewardedAd.Show(reward =>
            {
                Debug.Log("[AdManager] Recompensa ganada: " + reward.Type + " x" + reward.Amount);
                GrantReward(rewardCoins, rewardLives);
                AchievementManager.Instance?.TrackEvent(AchievementEvent.AdWatched);
            });
            rewardedAd.OnAdFullScreenContentClosed += () =>
            {
                LoadRewarded(); // precargar el siguiente
            };
        }
        else
        {
            Debug.Log("[AdManager] Recompensado no disponible — dando recompensa directa.");
            GrantReward(rewardCoins, rewardLives);
            LoadRewarded();
        }
    }

    // ── Banner (opcional) ─────────────────────────────────────────────────────────
    public void ShowBanner()  { /* implementar si se necesita */ }
    public void HideBanner()  { /* implementar si se necesita */ }

    // ── Recompensa al jugador ─────────────────────────────────────────────────────
    private void GrantReward(int coins, int lives)
    {
        if (coins > 0) GameManager.Instance?.AddCoins(coins);
        if (lives > 0) GameManager.Instance?.AddLives(lives);
        AudioManager.Instance?.Play("coinEarned");
    }
}

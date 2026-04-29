using UnityEngine;

/// <summary>
/// AdManager — Gestiona todos los anuncios del juego.
/// Soporta AdMob (recompensados, intersticiales, banners) y Unity Ads.
/// 
/// ANTES DE USAR: Importa los siguientes paquetes en Unity:
///   1. Google Mobile Ads Unity Plugin:
///      https://github.com/googleads/googleads-mobile-unity/releases
///   2. Unity Ads (ya incluido en Package Manager de Unity)
/// </summary>
public class AdManager : MonoBehaviour
{
    public static AdManager Instance { get; private set; }

    // ─── REEMPLAZA ESTOS IDs CON LOS TUYOS DE ADMOB ─────────────────────────────
    private const string ADMOB_APP_ID           = "ca-app-pub-XXXXXXXXXXXXXXXX~XXXXXXXXXX";
    private const string ADMOB_REWARDED_ID      = "ca-app-pub-XXXXXXXXXXXXXXXX/XXXXXXXXXX";
    private const string ADMOB_INTERSTITIAL_ID  = "ca-app-pub-XXXXXXXXXXXXXXXX/XXXXXXXXXX";
    private const string ADMOB_BANNER_ID        = "ca-app-pub-XXXXXXXXXXXXXXXX/XXXXXXXXXX";
    // ─── IDs de prueba de AdMob (úsalos mientras desarrollas) ───────────────────
    // ADMOB_REWARDED_ID     = "ca-app-pub-3940256099942544/5224354917"
    // ADMOB_INTERSTITIAL_ID = "ca-app-pub-3940256099942544/1033173712"
    // ADMOB_BANNER_ID       = "ca-app-pub-3940256099942544/6300978111"

    // ─── REEMPLAZA CON TU GAME ID DE UNITY ADS ──────────────────────────────────
    private const string UNITY_GAME_ID          = "XXXXXXXX";
    private const string UNITY_REWARDED_ID      = "Rewarded_Android";
    private const string UNITY_INTERSTITIAL_ID  = "Interstitial_Android";

    // Callback de recompensa pendiente
    private System.Action<int, int> pendingRewardCallback;

    // Evento que se dispara cuando el intersticial se cierra
    public event System.Action OnInterstitialClosed;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        InitializeAds();
    }

    // ── Inicializar SDKs ─────────────────────────────────────────────────────────
    private void InitializeAds()
    {
        /*
         * PASO 1: Descomenta este bloque DESPUÉS de importar Google Mobile Ads SDK:
         *
         * MobileAds.Initialize(initStatus => {
         *     LoadInterstitial();
         *     LoadRewarded();
         *     LoadBanner();
         * });
         *
         * PASO 2: Descomenta este bloque DESPUÉS de importar Unity Ads:
         *
         * Advertisement.Initialize(UNITY_GAME_ID, false, this);
         */

        Debug.Log("[AdManager] Inicializado (SDKs pendientes de importar)");
    }

    // ── Anuncio Recompensado ─────────────────────────────────────────────────────
    /// <param name="rewardCoins">Monedas a dar si ve el anuncio</param>
    /// <param name="rewardLives">Vidas a dar si ve el anuncio</param>
    public void ShowRewarded(int rewardCoins = 0, int rewardLives = 0)
    {
        if (GameManager.Instance.adsRemoved)
        {
            // Si quitó los forzados, los recompensados siguen disponibles
            // Dar recompensa directamente (política de MazeGlow)
            GrantReward(rewardCoins, rewardLives);
            return;
        }

        pendingRewardCallback = (coins, lives) =>
        {
            GrantReward(coins, lives);
            AchievementManager.Instance?.TrackEvent(AchievementEvent.AdWatched);
        };

        /*
         * Descomenta cuando tengas el SDK de AdMob:
         *
         * if (rewardedAd != null && rewardedAd.CanShowAd())
         * {
         *     rewardedAd.Show(reward => pendingRewardCallback?.Invoke(rewardCoins, rewardLives));
         * }
         */

        Debug.Log($"[AdManager] Mostrar anuncio recompensado → +{rewardCoins} monedas, +{rewardLives} vidas");
        // TEMPORAL: Dar recompensa directamente mientras desarrollas
        GrantReward(rewardCoins, rewardLives);
    }

    // ── Anuncio Intersticial ─────────────────────────────────────────────────────
    public void ShowInterstitial()
    {
        if (GameManager.Instance.adsRemoved)
        {
            OnInterstitialClosed?.Invoke();
            return;
        }

        /*
         * Descomenta cuando tengas el SDK:
         *
         * if (interstitialAd != null && interstitialAd.CanShowAd())
         * {
         *     interstitialAd.OnAdFullScreenContentClosed += () => {
         *         OnInterstitialClosed?.Invoke();
         *         LoadInterstitial();
         *     };
         *     interstitialAd.Show();
         *     return;
         * }
         */

        Debug.Log("[AdManager] Mostrar anuncio intersticial");
        OnInterstitialClosed?.Invoke();
    }

    // ── Banner ───────────────────────────────────────────────────────────────────
    public void ShowBanner()
    {
        if (GameManager.Instance.adsRemoved) return;
        Debug.Log("[AdManager] Mostrar banner");
        // bannerView?.Show();
    }

    public void HideBanner()
    {
        Debug.Log("[AdManager] Ocultar banner");
        // bannerView?.Hide();
    }

    // ── Dar recompensa al jugador ─────────────────────────────────────────────────
    private void GrantReward(int coins, int lives)
    {
        if (coins > 0) GameManager.Instance?.AddCoins(coins);
        if (lives > 0) GameManager.Instance?.AddLives(lives);
        AudioManager.Instance?.Play("coinEarned");
    }
}

/*
 * ── GUÍA DE CONFIGURACIÓN DE ADMOB ──────────────────────────────────────────────
 *
 * 1. Ve a https://admob.google.com y crea una cuenta
 * 2. Crea una nueva App → Android → MazeGlow
 * 3. Crea 3 bloques de anuncios: Recompensado, Intersticial, Banner
 * 4. Copia los IDs y reemplaza las constantes en este archivo
 * 5. En Unity: Assets > Google Mobile Ads > Settings → pega el App ID
 * 6. En AndroidManifest.xml agrega:
 *    <meta-data android:name="com.google.android.gms.ads.APPLICATION_ID"
 *               android:value="ca-app-pub-XXXXXXXX~XXXXXXXXXX"/>
 *
 * ── GUÍA DE CONFIGURACIÓN DE UNITY ADS ─────────────────────────────────────────
 *
 * 1. Ve a https://dashboard.unity3d.com/monetization
 * 2. Crea un nuevo proyecto → Android
 * 3. Copia el Game ID y reemplaza UNITY_GAME_ID arriba
 * 4. En Unity: Window > Package Manager → busca "Advertisement Legacy" e instálalo
 */

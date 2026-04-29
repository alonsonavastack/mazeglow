using UnityEngine;

/// <summary>
/// PlayFabManager — Integración con PlayFab de Microsoft para respaldo en la nube.
/// 
/// ANTES DE USAR:
///   1. Crea una cuenta gratuita en https://playfab.com
///   2. Crea un nuevo título (tu juego) y copia el Title ID
///   3. Instala el SDK: Window > Package Manager > "PlayFab SDK" (busca por nombre)
///      O descarga desde: https://github.com/PlayFab/UnitySDK
///   4. Reemplaza PLAYFAB_TITLE_ID con tu ID real
/// </summary>
public class PlayFabManager : MonoBehaviour
{
    public static PlayFabManager Instance { get; private set; }

    // ─── REEMPLAZA CON TU TITLE ID DE PLAYFAB ────────────────────────────────────
    private const string PLAYFAB_TITLE_ID = "XXXXX";

    private bool isLoggedIn = false;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    // ── Login anónimo (sin cuenta de usuario) ────────────────────────────────────
    public void LoginAnonymous()
    {
        /*
         * Descomenta después de instalar PlayFab SDK:
         *
         * PlayFabSettings.staticSettings.TitleId = PLAYFAB_TITLE_ID;
         *
         * var request = new LoginWithAndroidDeviceIDRequest
         * {
         *     AndroidDeviceId = SystemInfo.deviceUniqueIdentifier,
         *     CreateAccount   = true
         * };
         *
         * PlayFabClientAPI.LoginWithAndroidDeviceID(request,
         *     result => { isLoggedIn = true; Debug.Log("[PlayFab] Login OK"); },
         *     error  => { Debug.LogError($"[PlayFab] Error: {error.ErrorMessage}"); }
         * );
         */

        Debug.Log("[PlayFabManager] Login anónimo (SDK pendiente de instalar)");
    }

    // ── Guardar progreso en la nube ──────────────────────────────────────────────
    public void SaveToCloud()
    {
        if (!isLoggedIn) { Debug.Log("[PlayFab] No logueado"); return; }

        var gm = GameManager.Instance;
        /*
         * var data = new System.Collections.Generic.Dictionary<string, string>
         * {
         *     { "currentLevel", gm.currentLevel.ToString() },
         *     { "coins",        gm.coins.ToString()        },
         *     { "lives",        gm.lives.ToString()        },
         *     { "adsRemoved",   gm.adsRemoved.ToString()   }
         * };
         *
         * PlayFabClientAPI.UpdateUserData(
         *     new UpdateUserDataRequest { Data = data },
         *     result => Debug.Log("[PlayFab] Guardado en nube OK"),
         *     error  => Debug.LogError($"[PlayFab] Error guardando: {error.ErrorMessage}")
         * );
         */

        Debug.Log("[PlayFabManager] Guardar en nube (SDK pendiente)");
    }

    // ── Cargar progreso desde la nube ────────────────────────────────────────────
    public void LoadFromCloud()
    {
        if (!isLoggedIn) return;

        /*
         * PlayFabClientAPI.GetUserData(
         *     new GetUserDataRequest(),
         *     result =>
         *     {
         *         var gm = GameManager.Instance;
         *         if (result.Data.TryGetValue("currentLevel", out var lvl))
         *             gm.currentLevel = int.Parse(lvl.Value);
         *         if (result.Data.TryGetValue("coins", out var coins))
         *             gm.coins = int.Parse(coins.Value);
         *         // ... etc
         *     },
         *     error => Debug.LogError($"[PlayFab] Error cargando: {error.ErrorMessage}")
         * );
         */

        Debug.Log("[PlayFabManager] Cargar desde nube (SDK pendiente)");
    }
}

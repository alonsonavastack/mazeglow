using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// SettingsManager — Controla la pantalla de Configuración.
/// Gestiona: idioma, vibraciones, sonido, modo oscuro, cuenta,
/// eliminar anuncios, restaurar compras, califícanos, escríbenos, privacidad.
/// </summary>
public class SettingsManager : MonoBehaviour
{
    [Header("Sección 1 — Preferencias")]
    public TextMeshProUGUI languageText;     // Muestra el idioma actual
    public Toggle vibrationsToggle;
    public Toggle soundToggle;
    public Toggle darkModeToggle;

    [Header("Sección 2 — Cuenta")]
    public Toggle accountToggle;
    public GameObject connectAccountModal;

    [Header("Sección 3 — Compras")]
    public Toggle  adsRemovedToggle;
    public GameObject removeAdsModal;

    [Header("Configuración")]
    public const string SUPPORT_EMAIL    = "alonso.nava086@gmail.com";
    public const string PRIVACY_URL      = "https://tunombre.github.io/mazeglow-privacy"; // ← Cambiar cuando tengas la URL
    public const string PLAY_STORE_URL   = "market://details?id=com.tunombre.mazeglow";

    private void Start()
    {
        LoadSettings();
        BindListeners();
    }

    // ── Cargar valores actuales en los controles ─────────────────────────────────
    private void LoadSettings()
    {
        var gm = GameManager.Instance;
        if (gm == null) return;

        vibrationsToggle.isOn = gm.vibrationsEnabled;
        soundToggle.isOn      = gm.soundEnabled;
        darkModeToggle.isOn   = gm.darkModeEnabled;
        adsRemovedToggle.isOn = gm.adsRemoved;
        UpdateLanguageText(gm.language);
    }

    // ── Asignar listeners a los controles ────────────────────────────────────────
    private void BindListeners()
    {
        vibrationsToggle.onValueChanged.AddListener(OnVibrationsToggle);
        soundToggle.onValueChanged.AddListener(OnSoundToggle);
        darkModeToggle.onValueChanged.AddListener(OnDarkModeToggle);
    }

    // ── Cambio de idioma ─────────────────────────────────────────────────────────
    public void OpenLanguageSelector()
    {
        // Abre sub-pantalla de idioma (implementar como panel adicional)
        Debug.Log("[Settings] Abrir selector de idioma");
    }

    public void SetLanguage(string lang)
    {
        GameManager.Instance.language = lang;
        UpdateLanguageText(lang);
        SaveManager.Instance?.SaveAll();
        LocalizationManager.Instance?.SetLanguage(lang);
    }

    private void UpdateLanguageText(string lang)
    {
        if (languageText == null) return;
        languageText.text = lang == "es" ? "español" : "english";
    }

    // ── Vibraciones ──────────────────────────────────────────────────────────────
    private void OnVibrationsToggle(bool value)
    {
        GameManager.Instance.vibrationsEnabled = value;
        SaveManager.Instance?.SaveAll();
    }

    // ── Sonidos ──────────────────────────────────────────────────────────────────
    private void OnSoundToggle(bool value)
    {
        GameManager.Instance.soundEnabled = value;
        AudioManager.Instance?.SetMute(!value);
        SaveManager.Instance?.SaveAll();
    }

    // ── Modo oscuro ──────────────────────────────────────────────────────────────
    private void OnDarkModeToggle(bool value)
    {
        GameManager.Instance.darkModeEnabled = value;
        ThemeManager.Instance?.ApplyTheme(value);
        SaveManager.Instance?.SaveAll();
    }

    // ── Cuenta — Conectar con Google / Play Games ────────────────────────────────
    public void OpenConnectAccountModal()
    {
        if (connectAccountModal != null)
            connectAccountModal.SetActive(true);
    }

    public void CloseConnectAccountModal()
    {
        if (connectAccountModal != null)
            connectAccountModal.SetActive(false);
    }

    public void LoginWithGoogle()
    {
        // TODO: Integrar Google Sign-In SDK
        Debug.Log("[Settings] Login con Google");
        CloseConnectAccountModal();
    }

    public void LoginWithPlayGames()
    {
        // TODO: Integrar Google Play Games SDK
        Debug.Log("[Settings] Login con Play Games");
        CloseConnectAccountModal();
    }

    // ── Eliminar anuncios ────────────────────────────────────────────────────────
    public void OpenRemoveAdsModal()
    {
        if (removeAdsModal != null)
            removeAdsModal.SetActive(true);
    }

    public void CloseRemoveAdsModal()
    {
        if (removeAdsModal != null)
            removeAdsModal.SetActive(false);
    }

    /// <summary>Compra el plan elegido. planIndex: 0=1mes, 1=3meses, 2=6meses</summary>
    public void PurchasePlan(int planIndex)
    {
        // TODO: Conectar con Google Play Billing
        string[] productIds = {
            "mazeglow_no_ads_1month",
            "mazeglow_no_ads_3months",
            "mazeglow_no_ads_6months"
        };
        Debug.Log($"[Settings] Comprar plan: {productIds[planIndex]}");
        // IAPManager.Instance?.Purchase(productIds[planIndex]);
        CloseRemoveAdsModal();
    }

    // ── Restaurar compras ────────────────────────────────────────────────────────
    public void RestorePurchases()
    {
        // TODO: Conectar con Google Play Billing para restaurar
        Debug.Log("[Settings] Restaurar compras");
        // IAPManager.Instance?.RestorePurchases();
    }

    // ── Califícanos ──────────────────────────────────────────────────────────────
    public void RateUs()
    {
        Application.OpenURL(PLAY_STORE_URL);
    }

    // ── Escríbenos ───────────────────────────────────────────────────────────────
    public void ContactSupport()
    {
        string subject = Uri.EscapeDataString("Soporte MazeGlow");
        string body    = Uri.EscapeDataString("Hola, tengo una pregunta sobre MazeGlow...");
        Application.OpenURL($"mailto:{SUPPORT_EMAIL}?subject={subject}&body={body}");
    }

    // ── Política de privacidad ───────────────────────────────────────────────────
    public void OpenPrivacyPolicy()
    {
        Application.OpenURL(PRIVACY_URL);
    }
}

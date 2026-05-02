using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class MainMenuController : MonoBehaviour
{
    [Header("Botones")]
    public Button playButton;
    public Button settingsButton;

    [Header("Logo (opcional)")]
    public UnityEngine.UI.Image logoImage;

    [Header("Bottom Tabs")]
    public Button tabHomeButton;
    public Button tabAdvancedButton;
    public Button tabCollectionButton;
    public Button tabSettingsButton;

    [Header("Paneles y Alertas")]
    public GameObject collectionPanel;
    public GameObject streakAlertIcon;

    private void Start()
    {
        // Buscar logo automáticamente si no está asignado
        if (logoImage == null) BuscarLogoAutomatico();
        if (logoImage != null)
        {
            ThemeManager.ProtectImage(logoImage);
            logoImage.color = Color.white;
            StartCoroutine(PulseLogo());
        }

        // Aplicar tema
        ThemeManager.Instance?.ApplyTheme(GameManager.Instance?.darkModeEnabled ?? true);

        // Conectar botones principales
        if (playButton != null)
            playButton.onClick.AddListener(OnPlayButtonPressed);
        if (settingsButton != null)
            settingsButton.onClick.AddListener(OnSettingsButtonPressed);

        // Conectar pestañas inferiores
        if (tabHomeButton != null)
            tabHomeButton.onClick.AddListener(ShowHome);
        if (tabAdvancedButton != null)
            tabAdvancedButton.onClick.AddListener(ShowAdvanced);
        if (tabCollectionButton != null)
            tabCollectionButton.onClick.AddListener(ShowCollection);
        if (tabSettingsButton != null)
            tabSettingsButton.onClick.AddListener(OnSettingsButtonPressed);

        // Mostrar alerta de racha si es un nuevo día y no se ha reclamado
        if (streakAlertIcon != null && StreakManager.Instance != null)
        {
            streakAlertIcon.SetActive(!StreakManager.Instance.StreakClaimedToday);
        }
    }

    private void BuscarLogoAutomatico()
    {
        var go = GameObject.Find("LogoImage") ?? GameObject.Find("Protected_LogoImage");
        if (go != null) { logoImage = go.GetComponent<Image>(); return; }

        var canvas = FindAnyObjectByType<Canvas>();
        if (canvas == null) return;
        foreach (var img in canvas.GetComponentsInChildren<Image>(true))
        {
            string n = img.gameObject.name.ToLower();
            if (n.Contains("logo") || n.Contains("brand"))
            { logoImage = img; return; }
        }
    }

    private IEnumerator PulseLogo()
    {
        Vector3 b = logoImage.transform.localScale;
        for (float t = 0f; ; t += Time.deltaTime * 1.2f)
        { logoImage.transform.localScale = b * (1f + 0.025f * Mathf.Sin(t)); yield return null; }
    }

    public void OnPlayButtonPressed()     => SceneLoader.Instance?.GoToGame();
    public void OnSettingsButtonPressed() => SceneLoader.Instance?.GoToSettings();

    // ── Lógica de Tabs Inferiores ──────────────────────────────────────────────
    public void ShowHome()
    {
        if (collectionPanel != null) collectionPanel.SetActive(false);
        // Aquí se pueden ocultar otros paneles en el futuro
    }

    public void ShowAdvanced()
    {
        Debug.Log("[MainMenu] Niveles avanzados: Próximamente");
        // Opcional: Mostrar un pequeño mensaje temporal (Toast) en la pantalla
    }

    public void ShowCollection()
    {
        if (collectionPanel != null) collectionPanel.SetActive(true);
    }
}

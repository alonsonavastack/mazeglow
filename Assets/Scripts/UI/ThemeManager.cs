using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class ThemeManager : MonoBehaviour
{
    public static ThemeManager Instance { get; private set; }

    [Header("Colores — Modo Oscuro")]
    public Color darkBackground = new Color(0.05f, 0.05f, 0.15f);
    public Color darkText       = new Color(1.00f, 1.00f, 1.00f);

    [Header("Colores — Modo Claro")]
    public Color lightBackground = new Color(0.92f, 0.95f, 1.00f);
    public Color lightText       = new Color(0.08f, 0.08f, 0.18f);

    private static readonly string[] PROTECTED_NAMES = {
        "logoimage", "logo", "logoimagen", "playericon",
        "iconimage", "icon", "sprite", "imagen", "thumbnail",
        "avatar", "badge"
    };

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        ApplyTheme(GameManager.Instance?.darkModeEnabled ?? true);
    }

    private void OnEnable()  => SceneManager.sceneLoaded += OnSceneLoaded;
    private void OnDisable() => SceneManager.sceneLoaded -= OnSceneLoaded;

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        ApplyTheme(GameManager.Instance?.darkModeEnabled ?? true);
    }

    public void ApplyTheme(bool darkMode)
    {
        Color bg  = darkMode ? darkBackground : lightBackground;
        Color txt = darkMode ? darkText       : lightText;

        // ── Fondo de cámara ───────────────────────────────────────────────────────
        if (Camera.main != null)
            Camera.main.backgroundColor = bg;

        // ── Paneles de fondo ──────────────────────────────────────────────────────
        var allImages = Object.FindObjectsByType<Image>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (var img in allImages)
        {
            if (img == null) continue;
            if (img.sprite != null) continue;

            string nameLower = img.gameObject.name.ToLower();
            if (IsProtected(nameLower)) continue;

            if (nameLower == "panelfondo"     || nameLower == "background"       ||
                nameLower == "panelsettings"  || nameLower == "panelresults"     ||
                nameLower == "panelmainmenu"  || nameLower == "canvasbackground" ||
                nameLower == "panelgameover"  || nameLower == "panelmenu")
            {
                img.color = bg;
            }
        }

        // ── Textos fuera de botones ───────────────────────────────────────────────
        var allTexts = Object.FindObjectsByType<TextMeshProUGUI>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (var tmp in allTexts)
        {
            if (tmp == null) continue;

            Transform t = tmp.transform.parent;
            bool dentroBoton = false;
            for (int i = 0; i < 3 && t != null; i++)
            {
                if (t.GetComponent<Button>() != null) { dentroBoton = true; break; }
                t = t.parent;
            }

            if (!dentroBoton) tmp.color = txt;
        }

        Debug.Log("[ThemeManager] Tema: " + (darkMode ? "OSCURO" : "CLARO"));
    }

    public void ToggleTheme()
    {
        if (GameManager.Instance == null) return;
        GameManager.Instance.darkModeEnabled = !GameManager.Instance.darkModeEnabled;
        ApplyTheme(GameManager.Instance.darkModeEnabled);
        SaveManager.Instance?.SaveAll();
    }

    public static void ProtectImage(Image img)
    {
        if (img == null) return;
        img.gameObject.name = "Protected_" + img.gameObject.name;
    }

    private static bool IsProtected(string nameLower)
    {
        foreach (var p in PROTECTED_NAMES)
            if (nameLower.Contains(p)) return true;
        return false;
    }
}

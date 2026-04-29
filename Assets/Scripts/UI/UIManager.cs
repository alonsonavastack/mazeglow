using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

/// <summary>
/// UIManager v7
/// Fix 1: DestroyOldHUD() destruye el AutoHUD viejo ANTES de crear uno nuevo
/// Fix 2: 2 corazones (MAX_LIVES = 2)
/// Fix 3: Botón reintentar con símbolo ↺ (U+21BA)
/// </summary>
public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("HUD (dejar vacío = se crea automático)")]
    public TextMeshProUGUI levelText;
    public Button          backButton;
    public Button          retryButton;
    public Transform       heartsContainer;

    [Header("Paneles opcionales")]
    public GameObject      gameOverPanel;
    public GameObject      achievementPopup;
    public TextMeshProUGUI achievementTitle;
    public TextMeshProUGUI achievementDesc;
    public Animator        achievementAnimator;
    public TextMeshProUGUI celebrationText;

    // CONSTANTE CENTRAL — cambia aquí para cambiar en todo el HUD
    private const int MAX_LIVES = 2;

    private static readonly Color HeartFull  = new Color(0.95f, 0.15f, 0.20f);
    private static readonly Color HeartEmpty = new Color(0.35f, 0.35f, 0.40f);

    private readonly string[] celebrationPhrases = {
        "Impresionante!", "Brillante!", "Perfecto!", "Increible!", "Fantastico!"
    };

    // GameObject del canvas auto-generado, para poder destruirlo correctamente
    private static GameObject _autoHudCanvas;

    private void Awake()
    {
        // ── Singleton por escena ───────────────────────────────────────────────────
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        // Se quitó DontDestroyOnLoad(gameObject) para evitar referencias perdidas

        // ── Destruir TODOS los AutoHUD anteriores que puedan existir ──────────────
        // Esto resuelve el problema de que en el editor el DontDestroyOnLoad
        // acumula objetos entre sesiones de Play
        DestroyAllOldAutoHUD();
    }

    private static void DestroyAllOldAutoHUD()
    {
        // Buscar TODOS los objetos llamados "AutoHUD" en toda la escena incluyendo inactivos
        var all = Resources.FindObjectsOfTypeAll<Canvas>();
        foreach (var c in all)
        {
            if (c == null) continue;
            if (c.gameObject.name == "AutoHUD")
            {
                Debug.Log("[UIManager] Destruyendo AutoHUD viejo.");
                Destroy(c.gameObject);
            }
        }
        _autoHudCanvas = null;
    }

    private void Start()
    {
        // Siempre reconstruir el HUD al iniciar
        // (ignora asignaciones del Inspector para garantizar MAX_LIVES=2)
        BuildAutoHUD();
        RefreshHUD();
    }

    // ══════════════════════════════════════════════════════════════════════════════
    //  BUILD HUD
    // ══════════════════════════════════════════════════════════════════════════════
    private void BuildAutoHUD()
    {
        // Destruir canvas anterior si existe
        if (_autoHudCanvas != null)
        {
            Destroy(_autoHudCanvas);
            _autoHudCanvas = null;
        }

        _autoHudCanvas = new GameObject("AutoHUD");
        // Se quitó DontDestroyOnLoad(_autoHudCanvas) para que se limpie al salir de la escena

        var canvas = _autoHudCanvas.AddComponent<Canvas>();
        canvas.renderMode   = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;

        var scaler = _autoHudCanvas.AddComponent<CanvasScaler>();
        scaler.uiScaleMode         = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080, 1920);
        scaler.matchWidthOrHeight  = 0.5f;
        _autoHudCanvas.AddComponent<GraphicRaycaster>();

        // ── BARRA SUPERIOR ─────────────────────────────────────────────────────────
        var topBar = new GameObject("TopBar");
        topBar.transform.SetParent(_autoHudCanvas.transform, false);
        var topRT = topBar.AddComponent<RectTransform>();
        topRT.anchorMin = new Vector2(0f, 1f);
        topRT.anchorMax = new Vector2(1f, 1f);
        topRT.pivot     = new Vector2(0.5f, 1f);
        topRT.offsetMin = new Vector2(0f, -100f);
        topRT.offsetMax = Vector2.zero;
        topBar.AddComponent<Image>().color = new Color(0f, 0f, 0f, 0.62f);

        // [ < ]  Nivel X  ♥♥  [ ↺ ]

        // Botón REGRESAR "<"
        backButton = MakeIconButton("BtnBack", topBar.transform,
            new Vector2(0f, 0.5f), new Vector2(0f, 0.5f),
            new Vector2(12f, 0f), new Vector2(80f, 76f), "<", 38f);
        backButton.onClick.AddListener(() => {
            AudioManager.Instance?.Play("buttonClick");
            SceneLoader.Instance?.GoToMainMenu();
        });

        // Texto NIVEL
        levelText = MakeTMP("TxtNivel", topBar.transform,
            "Nivel 1", 30f, FontStyles.Bold, Color.white,
            new Vector2(0.28f, 0.5f), new Vector2(0.28f, 0.5f),
            Vector2.zero, new Vector2(250f, 76f), TextAlignmentOptions.Center);

        // CORAZONES — MAX_LIVES = 2
        var hCont = new GameObject("HeartsCont");
        hCont.transform.SetParent(topBar.transform, false);
        var hRT = hCont.AddComponent<RectTransform>();
        hRT.anchorMin = hRT.anchorMax = new Vector2(0.62f, 0.5f);
        hRT.pivot            = new Vector2(0.5f, 0.5f);
        hRT.anchoredPosition = Vector2.zero;
        hRT.sizeDelta        = new Vector2(MAX_LIVES * 72f, 70f);

        var layout = hCont.AddComponent<HorizontalLayoutGroup>();
        layout.spacing                = 8f;
        layout.childAlignment         = TextAnchor.MiddleCenter;
        layout.childForceExpandWidth  = false;
        layout.childForceExpandHeight = false;

        heartsContainer = hCont.transform;

        // Crear exactamente MAX_LIVES corazones (2)
        for (int i = 0; i < MAX_LIVES; i++)
        {
            var hGO = new GameObject("Heart_" + i);
            hGO.transform.SetParent(hCont.transform, false);
            hGO.AddComponent<RectTransform>().sizeDelta = new Vector2(58f, 58f);
            var t = hGO.AddComponent<TextMeshProUGUI>();
            t.text      = "\u2665"; // ♥
            t.fontSize  = 48f;
            t.alignment = TextAlignmentOptions.Center;
            t.color     = HeartFull;
        }

        // Botón REINTENTAR con símbolo "R" (la fuente no soporta ↺)
        retryButton = MakeIconButton("BtnRetry", topBar.transform,
            new Vector2(1f, 0.5f), new Vector2(1f, 0.5f),
            new Vector2(-12f, 0f), new Vector2(80f, 76f), "R", 40f);
        retryButton.onClick.AddListener(() => {
            AudioManager.Instance?.Play("buttonClick");
            GameController.Instance?.RetryLevel();
        });

        // TEXTO CELEBRACIÓN
        var celGO = new GameObject("TxtCelebracion");
        celGO.transform.SetParent(_autoHudCanvas.transform, false);
        var celRT = celGO.AddComponent<RectTransform>();
        celRT.anchorMin = celRT.anchorMax = new Vector2(0.5f, 0.6f);
        celRT.anchoredPosition = Vector2.zero;
        celRT.sizeDelta = new Vector2(700f, 120f);
        var celTMP = celGO.AddComponent<TextMeshProUGUI>();
        celTMP.text      = "";
        celTMP.fontSize  = 62f;
        celTMP.fontStyle = FontStyles.Bold;
        celTMP.color     = new Color(1f, 0.88f, 0.10f);
        celTMP.alignment = TextAlignmentOptions.Center;
        celGO.SetActive(false);
        celebrationText = celTMP;

        Debug.Log("[UIManager] AutoHUD construido con MAX_LIVES=" + MAX_LIVES);
    }

    // ══════════════════════════════════════════════════════════════════════════════
    //  REFRESH
    // ══════════════════════════════════════════════════════════════════════════════
    public void RefreshHUD()
    {
        var gm = GameManager.Instance;
        if (gm == null) return;

        if (levelText != null)
            levelText.text = "Nivel " + gm.currentLevel;

        RefreshHearts(gm.lives);
    }

    private void RefreshHearts(int currentLives)
    {
        if (heartsContainer == null) return;
        var tmps = heartsContainer.GetComponentsInChildren<TextMeshProUGUI>();
        for (int i = 0; i < tmps.Length && i < MAX_LIVES; i++)
            tmps[i].color = i < currentLives ? HeartFull : HeartEmpty;
    }

    // ══════════════════════════════════════════════════════════════════════════════
    //  API PÚBLICA
    // ══════════════════════════════════════════════════════════════════════════════
    public void ShowGameHUD(int level)
    {
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        RefreshHUD();
    }

    public void ShowGameOverPanel()
    {
        if (gameOverPanel != null) gameOverPanel.SetActive(true);
    }

    public void ShowAchievementUnlocked(Achievement ach)
    {
        if (achievementPopup == null) return;
        if (achievementTitle != null) achievementTitle.text = ach.name;
        if (achievementDesc  != null) achievementDesc.text  = ach.description;
        StartCoroutine(AchievementRoutine());
    }

    private IEnumerator AchievementRoutine()
    {
        achievementPopup.SetActive(true);
        achievementAnimator?.SetTrigger("Show");
        yield return new WaitForSeconds(3f);
        achievementPopup.SetActive(false);
    }

    public void ShowCelebration()
    {
        if (celebrationText == null) return;
        celebrationText.text = celebrationPhrases[Random.Range(0, celebrationPhrases.Length)];
        StartCoroutine(HideCelebration());
    }

    private IEnumerator HideCelebration()
    {
        celebrationText.gameObject.SetActive(true);
        yield return new WaitForSeconds(2.5f);
        celebrationText.gameObject.SetActive(false);
    }

    // ══════════════════════════════════════════════════════════════════════════════
    //  HELPERS
    // ══════════════════════════════════════════════════════════════════════════════
    private static Button MakeIconButton(string name, Transform parent,
        Vector2 ancMin, Vector2 ancMax, Vector2 ancPos, Vector2 size,
        string symbol, float fontSize)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin = ancMin; rt.anchorMax = ancMax;
        rt.pivot            = new Vector2(ancMin.x > 0.5f ? 1f : 0f, 0.5f);
        rt.anchoredPosition = ancPos;
        rt.sizeDelta        = size;

        var img = go.AddComponent<Image>();
        img.color = new Color(0.18f, 0.18f, 0.32f, 0.85f);

        var iconGO = new GameObject("Icon");
        iconGO.transform.SetParent(go.transform, false);
        var iRT = iconGO.AddComponent<RectTransform>();
        iRT.anchorMin = Vector2.zero; iRT.anchorMax = Vector2.one;
        iRT.offsetMin = iRT.offsetMax = Vector2.zero;
        var tmp = iconGO.AddComponent<TextMeshProUGUI>();
        tmp.text = symbol; tmp.fontSize = fontSize;
        tmp.fontStyle = FontStyles.Bold; tmp.color = Color.white;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.raycastTarget = false;

        var btn = go.AddComponent<Button>();
        btn.targetGraphic = img;
        var cols = btn.colors;
        cols.normalColor      = new Color(0.18f, 0.18f, 0.32f, 0.85f);
        cols.highlightedColor = new Color(0.30f, 0.30f, 0.55f);
        cols.pressedColor     = new Color(0.10f, 0.10f, 0.20f);
        btn.colors = cols;
        return btn;
    }

    private static TextMeshProUGUI MakeTMP(string name, Transform parent,
        string text, float size, FontStyles style, Color color,
        Vector2 ancMin, Vector2 ancMax, Vector2 ancPos, Vector2 sizeDelta,
        TextAlignmentOptions align)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin = ancMin; rt.anchorMax = ancMax;
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = ancPos; rt.sizeDelta = sizeDelta;
        var tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text = text; tmp.fontSize = size;
        tmp.fontStyle = style; tmp.color = color;
        tmp.alignment = align;
        return tmp;
    }
}

using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class GameOverScreen : MonoBehaviour
{
    public static GameOverScreen Instance { get; private set; }

    private Canvas     canvas;
    private GameObject panel;
    private Image      flashImage;

    private TextMeshProUGUI txtTitulo;
    private TextMeshProUGUI txtMensaje;

    private Button btnContinuar;
    private Button btnReintentar;
    private Button btnMenu;

    private Camera mainCam;
    private bool   isShowing = false;

    private static readonly string[] Mensajes = {
        "Sin vidas!\nEl laberinto gano esta vez...",
        "Casi lo logras!\nIntentalo de nuevo",
        "El camino fue dificil!\nLo intentas otra vez?",
        "No te rindas!\nEstas muy cerca",
    };

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        mainCam = Camera.main;
        BuildCanvas();
        HideImmediate();
    }

    private void BuildCanvas()
    {
        var cGO = new GameObject("GameOverCanvas");
        cGO.transform.SetParent(transform, false);
        DontDestroyOnLoad(cGO);

        canvas = cGO.AddComponent<Canvas>();
        canvas.renderMode   = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 300;

        var scaler = cGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode         = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080, 1920);
        scaler.matchWidthOrHeight  = 0.5f;
        cGO.AddComponent<GraphicRaycaster>();

        // Flash rojo
        var flashGO = new GameObject("Flash");
        flashGO.transform.SetParent(cGO.transform, false);
        var fRT = flashGO.AddComponent<RectTransform>();
        fRT.anchorMin = Vector2.zero; fRT.anchorMax = Vector2.one;
        fRT.offsetMin = fRT.offsetMax = Vector2.zero;
        flashImage = flashGO.AddComponent<Image>();
        flashImage.color = new Color(0.8f, 0f, 0f, 0f);
        flashImage.raycastTarget = false;

        // Fondo
        var bg = new GameObject("BG");
        bg.transform.SetParent(cGO.transform, false);
        var bgRT = bg.AddComponent<RectTransform>();
        bgRT.anchorMin = Vector2.zero; bgRT.anchorMax = Vector2.one;
        bgRT.offsetMin = bgRT.offsetMax = Vector2.zero;
        bg.AddComponent<Image>().color = new Color(0f, 0f, 0f, 0.80f);

        // Panel
        panel = new GameObject("Panel");
        panel.transform.SetParent(cGO.transform, false);
        var pRT = panel.AddComponent<RectTransform>();
        pRT.anchorMin = new Vector2(0.5f, 0.5f);
        pRT.anchorMax = new Vector2(0.5f, 0.5f);
        pRT.pivot     = new Vector2(0.5f, 0.5f);
        pRT.sizeDelta = new Vector2(900f, 1000f);
        pRT.anchoredPosition = Vector2.zero;
        panel.AddComponent<Image>().color = new Color(0.12f, 0.03f, 0.03f, 0.97f);

        // Borde rojo
        var border = new GameObject("Border");
        border.transform.SetParent(panel.transform, false);
        var bRT = border.AddComponent<RectTransform>();
        bRT.anchorMin = Vector2.zero; bRT.anchorMax = Vector2.one;
        bRT.offsetMin = new Vector2(-3f, -3f); bRT.offsetMax = new Vector2(3f, 3f);
        border.AddComponent<Image>().color = new Color(0.8f, 0.1f, 0.1f, 0.7f);
        border.transform.SetSiblingIndex(0);

        // Icono corazones (unicode seguro)
        var iconGO = new GameObject("Icon");
        iconGO.transform.SetParent(panel.transform, false);
        var iRT = iconGO.AddComponent<RectTransform>();
        iRT.anchorMin = new Vector2(0.5f, 1f); iRT.anchorMax = new Vector2(0.5f, 1f);
        iRT.pivot = new Vector2(0.5f, 1f);
        iRT.anchoredPosition = new Vector2(0f, -60f);
        iRT.sizeDelta = new Vector2(160f, 160f);
        var iconTMP = iconGO.AddComponent<TextMeshProUGUI>();
        iconTMP.text = "\u2665\u2665";
        iconTMP.fontSize = 100f;
        iconTMP.color = new Color(0.5f, 0.05f, 0.05f);
        iconTMP.alignment = TextAlignmentOptions.Center;

        // Titulo
        txtTitulo = MakeTMP("TxtTitulo", panel.transform,
            "GAME OVER", 72f, FontStyles.Bold,
            new Color(0.9f, 0.1f, 0.1f),
            new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
            new Vector2(0f, -240f), new Vector2(860f, 110f),
            TextAlignmentOptions.Center);

        Sep(panel.transform, new Vector2(0f, -360f));

        txtMensaje = MakeTMP("TxtMensaje", panel.transform,
            "Sin vidas!", 36f, FontStyles.Normal,
            new Color(0.8f, 0.7f, 0.7f),
            new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
            new Vector2(0f, -400f), new Vector2(800f, 110f),
            TextAlignmentOptions.Center);

        Sep(panel.transform, new Vector2(0f, -520f));

        // Boton VER ANUNCIO — sin simbolos especiales
        btnContinuar = MakeButton("BtnContinuar", panel.transform,
            new Vector2(0f, -580f), new Vector2(760f, 110f),
            "VER ANUNCIO Y CONTINUAR", 34f,
            new Color(0.1f, 0.4f, 0.8f),
            new Color(0.15f, 0.6f, 1f));
        btnContinuar.onClick.AddListener(OnClickContinuar);

        // Boton REINTENTAR — sin simbolos especiales
        btnReintentar = MakeButton("BtnReintentar", panel.transform,
            new Vector2(0f, -710f), new Vector2(760f, 95f),
            "Reintentar", 36f,
            new Color(0.18f, 0.15f, 0.35f),
            new Color(0.3f, 0.25f, 0.6f));
        btnReintentar.onClick.AddListener(OnClickReintentar);

        // Boton MENU
        btnMenu = MakeButton("BtnMenu", panel.transform,
            new Vector2(0f, -825f), new Vector2(760f, 85f),
            "Menu principal", 30f,
            new Color(0.10f, 0.08f, 0.08f),
            new Color(0.25f, 0.15f, 0.15f));
        btnMenu.onClick.AddListener(OnClickMenu);
    }

    // ── API publica ──────────────────────────────────────────────────────────────
    public void Show()
    {
        if (isShowing) return;
        isShowing = true;
        canvas.gameObject.SetActive(true);
        if (txtMensaje != null)
            txtMensaje.text = Mensajes[Random.Range(0, Mensajes.Length)];
        if (panel != null) panel.transform.localScale = Vector3.zero;
        StartCoroutine(ShowRoutine());
    }

    public void HideImmediate()
    {
        StopAllCoroutines();
        isShowing = false;
        if (canvas     != null) canvas.gameObject.SetActive(false);
        if (flashImage != null) flashImage.color = new Color(0.8f, 0f, 0f, 0f);
    }

    // ── Animaciones ──────────────────────────────────────────────────────────────
    private IEnumerator ShowRoutine()
    {
        yield return StartCoroutine(RedFlash());
        yield return StartCoroutine(CameraShake(0.4f, 0.3f));
        yield return StartCoroutine(PanelDropIn());
    }

    private IEnumerator RedFlash()
    {
        float t = 0f;
        while (t < 0.15f)
        {
            t += Time.deltaTime;
            float a = Mathf.Sin(t / 0.15f * Mathf.PI) * 0.7f;
            if (flashImage != null) flashImage.color = new Color(0.8f, 0f, 0f, a);
            yield return null;
        }
        if (flashImage != null) flashImage.color = new Color(0.8f, 0f, 0f, 0f);
    }

    private IEnumerator CameraShake(float duration, float magnitude)
    {
        if (mainCam == null) yield break;
        Vector3 originalPos = mainCam.transform.localPosition;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float fade = 1f - (elapsed / duration);
            float x = Random.Range(-1f, 1f) * magnitude * fade;
            float y = Random.Range(-1f, 1f) * magnitude * fade;
            mainCam.transform.localPosition = originalPos + new Vector3(x, y, 0f);
            yield return null;
        }
        mainCam.transform.localPosition = originalPos;
    }

    private IEnumerator PanelDropIn()
    {
        if (panel == null) yield break;
        float t = 0f, dur = 0.45f;
        panel.transform.localScale = Vector3.zero;
        while (t < dur)
        {
            t += Time.deltaTime;
            panel.transform.localScale = Vector3.one * EaseOutBack(t / dur);
            yield return null;
        }
        panel.transform.localScale = Vector3.one;
    }

    // ── Botones ──────────────────────────────────────────────────────────────────
    private void OnClickContinuar()
    {
        HideImmediate();
        if (AdManager.Instance != null)
        {
            AdManager.Instance.OnRewardedClosed += HandleRewardedClosed;
            AdManager.Instance.ShowRewarded(rewardCoins: 0, rewardLives: 2);
        }
        else
        {
            GameController.Instance?.RetryLevel();
        }
    }

    private void HandleRewardedClosed()
    {
        if (AdManager.Instance != null)
            AdManager.Instance.OnRewardedClosed -= HandleRewardedClosed;
        GameController.Instance?.RetryLevel();
    }

    private void OnClickReintentar()
    {
        HideImmediate();
        GameController.Instance?.RetryLevel();
    }

    private void OnClickMenu()
    {
        HideImmediate();
        SceneLoader.Instance?.GoToMainMenu();
    }

    // ── Easing ───────────────────────────────────────────────────────────────────
    private static float EaseOutBack(float x)
    {
        float c1 = 1.70158f, c3 = c1 + 1f;
        return Mathf.Clamp01(1f + c3 * Mathf.Pow(x - 1f, 3f) + c1 * Mathf.Pow(x - 1f, 2f));
    }

    // ── Helpers UI ───────────────────────────────────────────────────────────────
    private static TextMeshProUGUI MakeTMP(string name, Transform parent,
        string text, float size, FontStyles style, Color color,
        Vector2 ancMin, Vector2 ancMax, Vector2 ancPos, Vector2 sizeDelta,
        TextAlignmentOptions align)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin = ancMin; rt.anchorMax = ancMax;
        rt.pivot = new Vector2(0.5f, 1f);
        rt.anchoredPosition = ancPos; rt.sizeDelta = sizeDelta;
        var tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text = text; tmp.fontSize = size;
        tmp.fontStyle = style; tmp.color = color;
        tmp.alignment = align;
        return tmp;
    }

    private static Button MakeButton(string name, Transform parent,
        Vector2 ancPos, Vector2 size, string label, float fontSize,
        Color colorNormal, Color colorHover)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 1f);
        rt.pivot = new Vector2(0.5f, 1f);
        rt.anchoredPosition = ancPos; rt.sizeDelta = size;
        var img = go.AddComponent<Image>();
        img.color = colorNormal;
        var lbl = new GameObject("Label");
        lbl.transform.SetParent(go.transform, false);
        var lRT = lbl.AddComponent<RectTransform>();
        lRT.anchorMin = Vector2.zero; lRT.anchorMax = Vector2.one;
        lRT.offsetMin = lRT.offsetMax = Vector2.zero;
        var tmp = lbl.AddComponent<TextMeshProUGUI>();
        tmp.text = label; tmp.fontSize = fontSize;
        tmp.fontStyle = FontStyles.Bold; tmp.color = Color.white;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.raycastTarget = false;
        var btn = go.AddComponent<Button>();
        btn.targetGraphic = img;
        var colors = btn.colors;
        colors.normalColor      = colorNormal;
        colors.highlightedColor = colorHover;
        colors.pressedColor     = colorNormal * 0.7f;
        btn.colors = colors;
        return btn;
    }

    private static void Sep(Transform parent, Vector2 ancPos)
    {
        var go = new GameObject("Sep");
        go.transform.SetParent(parent, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 1f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = ancPos; rt.sizeDelta = new Vector2(800f, 2f);
        go.AddComponent<Image>().color = new Color(0.8f, 0.1f, 0.1f, 0.4f);
    }
}

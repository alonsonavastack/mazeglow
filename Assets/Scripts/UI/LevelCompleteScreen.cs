using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

/// <summary>
/// LevelCompleteScreen — Pantalla PRO de nivel completado.
///
/// ESTRELLAS 3D REALES:
/// • Usa el mesh Estrella_MazeGlow.obj (Assets/Materials/)
/// • Una cámara secundaria (StarCamera) renderiza las 3 estrellas en 3D
/// • La cámara vuelca su imagen a un RenderTexture que el Canvas muestra como RawImage
/// • Las estrellas caen con física de rebote y giran continuamente
///
/// OTROS EFECTOS:
/// • Zoom-out de cámara principal al completar
/// • Confeti 3D real (cubos que explotan desde el jugador con gravedad)
/// • Panel con rebote al entrar
/// </summary>
public class LevelCompleteScreen : MonoBehaviour
{
    public static LevelCompleteScreen Instance { get; private set; }

    // ── Cámara principal ───────────────────────────────────────────────────────
    private Camera mainCam;
    private float  originalFOV;

    // ── Canvas de la pantalla ──────────────────────────────────────────────────
    private Canvas     canvas;
    private GameObject panel;

    // ── Textos del panel ───────────────────────────────────────────────────────
    private TextMeshProUGUI txtTitulo;
    private TextMeshProUGUI txtNivel;
    private TextMeshProUGUI txtMonedas;
    private TextMeshProUGUI txtMensaje;
    private Button          btnSiguiente;
    private Button          btnMenu;

    // ── Estrellas 3D ───────────────────────────────────────────────────────────
    private Star3D[]       stars3D     = new Star3D[3];
    private GameObject     starScene;
    private Camera         starCam;
    private RenderTexture  starRT;

    // ── Confeti 3D ─────────────────────────────────────────────────────────────
    private System.Collections.Generic.List<GameObject> confetti3D
        = new System.Collections.Generic.List<GameObject>();

    private static readonly Color[] ConfettiColors = {
        new Color(1f,0.15f,0.15f), new Color(1f,0.85f,0.05f),
        new Color(0.15f,0.9f,0.15f), new Color(0.2f,0.5f,1f),
        new Color(1f,0.35f,0.9f),   new Color(1f,0.55f,0.05f),
        new Color(0.8f,0.2f,1f),
    };

    private static readonly string[] Mensajes = {
        "¡IMPRESIONANTE!", "¡BRILLANTE!", "¡PERFECTO!", "¡INCREIBLE!",
        "¡FANTASTICO!",   "¡EXCELENTE!", "¡GENIAL!",   "¡ASOMBROSO!"
    };

    // ══════════════════════════════════════════════════════════════════════════
    //  AWAKE
    // ══════════════════════════════════════════════════════════════════════════
    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        mainCam = Camera.main;
        if (mainCam != null) originalFOV = mainCam.fieldOfView;

        Build3DStarScene();
        BuildCanvas();
        HideImmediate();
    }

    // ══════════════════════════════════════════════════════════════════════════
    //  ESCENA 3D DE ESTRELLAS
    // ══════════════════════════════════════════════════════════════════════════
    private void Build3DStarScene()
    {
        starScene = new GameObject("StarScene3D");
        starScene.transform.position = new Vector3(1000f, 0f, 0f);
        DontDestroyOnLoad(starScene);

        var lightGO = new GameObject("StarLight");
        lightGO.transform.SetParent(starScene.transform, false);
        lightGO.transform.localPosition = new Vector3(2f, 3f, -3f);
        var lt = lightGO.AddComponent<Light>();
        lt.type      = LightType.Directional;
        lt.color     = new Color(1f, 0.95f, 0.8f);
        lt.intensity = 1.6f;

        float[] posX = { -2.5f, 0f, 2.5f };
        for (int i = 0; i < 3; i++)
        {
            GameObject starGO = TryLoadStarMesh(i);
            starGO.transform.SetParent(starScene.transform, false);
            starGO.transform.localPosition = new Vector3(posX[i], 0f, 0f);
            starGO.transform.localScale    = Vector3.one * 0.9f;
            stars3D[i] = starGO.AddComponent<Star3D>();
        }

        var camGO = new GameObject("StarCamera");
        camGO.transform.SetParent(starScene.transform, false);
        camGO.transform.localPosition = new Vector3(0f, 0f, -6f);
        camGO.transform.localRotation = Quaternion.identity;

        starCam = camGO.AddComponent<Camera>();
        starCam.clearFlags      = CameraClearFlags.SolidColor;
        starCam.backgroundColor = new Color(0f, 0f, 0f, 0f);
        starCam.fieldOfView     = 35f;
        starCam.nearClipPlane   = 0.1f;
        starCam.farClipPlane    = 20f;
        starCam.depth           = 5;
        starCam.cullingMask     = ~0;

        starRT = new RenderTexture(900, 280, 16, RenderTextureFormat.ARGB32);
        starRT.antiAliasing   = 4;
        starCam.targetTexture = starRT;
        starCam.enabled       = false;
    }

    private GameObject TryLoadStarMesh(int index)
    {
        var prefab = Resources.Load<GameObject>("Estrella_MazeGlow");
        if (prefab != null)
        {
            var go = Instantiate(prefab);
            go.name = "Star3D_" + index;
            return go;
        }

        Debug.LogWarning("[LevelCompleteScreen] No se encontro Estrella_MazeGlow en Resources/. " +
            "Mueve Assets/Materials/Estrella_MazeGlow.obj a Assets/Resources/ para el mesh real.");

        var fallback = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        fallback.name = "Star3D_fallback_" + index;
        var col = fallback.GetComponent<Collider>();
        if (col != null) Destroy(col);
        return fallback;
    }

    // ══════════════════════════════════════════════════════════════════════════
    //  CANVAS UI
    // ══════════════════════════════════════════════════════════════════════════
    private void BuildCanvas()
    {
        var canvasGO = new GameObject("LevelCompleteCanvas");
        canvasGO.transform.SetParent(transform, false);
        DontDestroyOnLoad(canvasGO);

        canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode   = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 300;

        var scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode         = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080, 1920);
        scaler.matchWidthOrHeight  = 0.5f;
        canvasGO.AddComponent<GraphicRaycaster>();

        var bg = MakeStretch("BG", canvasGO.transform);
        bg.AddComponent<Image>().color = new Color(0f, 0f, 0f, 0.78f);

        panel = new GameObject("Panel");
        panel.transform.SetParent(canvasGO.transform, false);
        var panelRT = panel.AddComponent<RectTransform>();
        panelRT.anchorMin = panelRT.anchorMax = new Vector2(0.5f, 0.5f);
        panelRT.pivot     = new Vector2(0.5f, 0.5f);
        panelRT.sizeDelta = new Vector2(920f, 1150f);
        panelRT.anchoredPosition = Vector2.zero;
        panel.AddComponent<Image>().color = new Color(0.06f, 0.05f, 0.18f, 0.97f);

        var bdr = new GameObject("Border");
        bdr.transform.SetParent(panel.transform, false);
        var bdrRT = bdr.AddComponent<RectTransform>();
        bdrRT.anchorMin = Vector2.zero; bdrRT.anchorMax = Vector2.one;
        bdrRT.offsetMin = new Vector2(-3,-3); bdrRT.offsetMax = new Vector2(3,3);
        bdr.AddComponent<Image>().color = new Color(0.4f, 0.3f, 1f, 0.55f);
        bdr.transform.SetSiblingIndex(0);

        txtTitulo = MakeTMP("TxtTitulo", panel.transform,
            "¡NIVEL COMPLETADO!", 58f, FontStyles.Bold,
            new Color(1f, 0.92f, 0.1f),
            new Vector2(0.5f, 1f), new Vector2(0f, -60f),
            new Vector2(880f, 100f), TextAlignmentOptions.Center);

        Separator(panel.transform, new Vector2(0f, -175f));

        var rtGO = new GameObject("Stars3DView");
        rtGO.transform.SetParent(panel.transform, false);
        var rtRect = rtGO.AddComponent<RectTransform>();
        rtRect.anchorMin = new Vector2(0.5f, 1f);
        rtRect.anchorMax = new Vector2(0.5f, 1f);
        rtRect.pivot     = new Vector2(0.5f, 1f);
        rtRect.anchoredPosition = new Vector2(0f, -200f);
        rtRect.sizeDelta        = new Vector2(880f, 275f);
        var rawImg = rtGO.AddComponent<RawImage>();
        rawImg.texture = starRT;
        rawImg.color   = Color.white;

        txtNivel = MakeTMP("TxtNivel", panel.transform,
            "Nivel 1", 44f, FontStyles.Normal,
            new Color(0.7f, 0.7f, 1f),
            new Vector2(0.5f, 1f), new Vector2(0f, -500f),
            new Vector2(600f, 70f), TextAlignmentOptions.Center);

        txtMonedas = MakeTMP("TxtMonedas", panel.transform,
            "+ 20 monedas", 40f, FontStyles.Bold,
            new Color(1f, 0.85f, 0.1f),
            new Vector2(0.5f, 1f), new Vector2(0f, -580f),
            new Vector2(600f, 60f), TextAlignmentOptions.Center);

        txtMensaje = MakeTMP("TxtMensaje", panel.transform,
            "¡IMPRESIONANTE!", 52f, FontStyles.Bold,
            new Color(0.4f, 1f, 0.5f),
            new Vector2(0.5f, 1f), new Vector2(0f, -665f),
            new Vector2(880f, 80f), TextAlignmentOptions.Center);

        Separator(panel.transform, new Vector2(0f, -760f));

        // ── BOTÓN SIGUIENTE — sin carácter especial ────────────────────────────
        btnSiguiente = MakeButton("BtnSiguiente", panel.transform,
            new Vector2(0f, -850f), new Vector2(780f, 115f),
            "SIGUIENTE NIVEL  >>", 42f,
            new Color(0.1f, 0.6f, 0.1f), new Color(0.15f, 0.9f, 0.2f));
        btnSiguiente.onClick.AddListener(OnClickSiguiente);

        btnMenu = MakeButton("BtnMenu", panel.transform,
            new Vector2(0f, -985f), new Vector2(780f, 90f),
            "Volver al menu", 32f,
            new Color(0.12f, 0.10f, 0.28f), new Color(0.25f, 0.20f, 0.55f));
        btnMenu.onClick.AddListener(OnClickMenu);
    }

    // ══════════════════════════════════════════════════════════════════════════
    //  API PUBLICA
    // ══════════════════════════════════════════════════════════════════════════
    public void Show(int level, int starsEarned, int coinsEarned)
    {
        gameObject.SetActive(true);
        canvas.gameObject.SetActive(true);
        starCam.enabled = true;

        if (txtNivel   != null) txtNivel.text   = "Nivel " + level;
        if (txtMonedas != null) txtMonedas.text = "+ " + coinsEarned + " monedas";
        if (txtMensaje != null) txtMensaje.text = Mensajes[Random.Range(0, Mensajes.Length)];

        foreach (var s in stars3D) s?.SetEmpty();
        if (panel != null) panel.transform.localScale = Vector3.zero;

        StartCoroutine(ShowRoutine(starsEarned));
        StartCoroutine(SpawnConfetti3D());
        StartCoroutine(CameraZoomOut());
    }

    public void HideImmediate()
    {
        StopAllCoroutines();
        CleanConfetti();
        if (canvas  != null) canvas.gameObject.SetActive(false);
        if (starCam != null) starCam.enabled = false;
        if (mainCam != null) mainCam.fieldOfView = originalFOV;
    }

    // ══════════════════════════════════════════════════════════════════════════
    //  ANIMACIONES
    // ══════════════════════════════════════════════════════════════════════════
    private IEnumerator ShowRoutine(int starsEarned)
    {
        yield return StartCoroutine(ScaleIn(panel.transform, 0.5f));

        for (int i = 0; i < 3; i++)
            StartCoroutine(stars3D[i].AnimateEarn(i < starsEarned, delay: i * 0.28f));

        yield return new WaitForSeconds(3 * 0.28f + 0.5f);

        StartCoroutine(FadeInButton(btnSiguiente));
        yield return new WaitForSeconds(0.1f);
        StartCoroutine(FadeInButton(btnMenu));
    }

    private IEnumerator ScaleIn(Transform t, float dur)
    {
        float elapsed = 0f;
        while (elapsed < dur)
        {
            elapsed += Time.deltaTime;
            t.localScale = Vector3.one * EaseOutBack(elapsed / dur);
            yield return null;
        }
        t.localScale = Vector3.one;
    }

    private IEnumerator FadeInButton(Button btn)
    {
        if (btn == null) yield break;
        var img = btn.GetComponent<Image>();
        if (img == null) yield break;
        float t = 0f, dur = 0.3f;
        Color start = new Color(img.color.r, img.color.g, img.color.b, 0f);
        Color end   = img.color;
        while (t < dur)
        {
            t += Time.deltaTime;
            img.color = Color.Lerp(start, end, t / dur);
            yield return null;
        }
        img.color = end;
    }

    private IEnumerator SpawnConfetti3D()
    {
        var player = FindAnyObjectByType<PlayerController>();
        Vector3 origin = player != null
            ? player.transform.position + Vector3.up * 2f
            : new Vector3(5f, 3f, 5f);

        for (int wave = 0; wave < 4; wave++)
        {
            for (int i = 0; i < 15; i++) SpawnConfettiPiece(origin);
            yield return new WaitForSeconds(0.2f);
        }
    }

    private void SpawnConfettiPiece(Vector3 origin)
    {
        var obj = GameObject.CreatePrimitive(PrimitiveType.Cube);
        obj.name = "Confetti3D";
        obj.transform.position   = origin;
        obj.transform.localScale = new Vector3(
            Random.Range(0.08f, 0.18f),
            Random.Range(0.04f, 0.10f),
            Random.Range(0.08f, 0.18f));
        obj.transform.rotation = Random.rotation;

        var col = obj.GetComponent<Collider>();
        if (col != null) Destroy(col);

        var mat = new Material(Shader.Find("Standard"));
        Color c = ConfettiColors[Random.Range(0, ConfettiColors.Length)];
        mat.color = c;
        mat.EnableKeyword("_EMISSION");
        mat.SetColor("_EmissionColor", c * 0.5f);
        obj.GetComponent<Renderer>().material = mat;
        confetti3D.Add(obj);

        Vector3 vel    = new Vector3(Random.Range(-4f,4f), Random.Range(5f,12f), Random.Range(-4f,4f));
        Vector3 angVel = Random.insideUnitSphere * 400f;
        StartCoroutine(AnimateConfettiPiece(obj, vel, angVel));
    }

    private IEnumerator AnimateConfettiPiece(GameObject obj, Vector3 vel, Vector3 angVel)
    {
        float life = Random.Range(1.5f, 2.8f), t = 0f;
        while (t < life && obj != null)
        {
            t     += Time.deltaTime;
            vel.y -= 9.8f * Time.deltaTime;
            obj.transform.position += vel * Time.deltaTime;
            obj.transform.Rotate(angVel * Time.deltaTime);
            var rend = obj.GetComponent<Renderer>();
            if (rend != null)
            {
                Color c = rend.material.color;
                rend.material.color = new Color(c.r, c.g, c.b, Mathf.Clamp01(1f - t / life));
            }
            yield return null;
        }
        if (obj != null) { confetti3D.Remove(obj); Destroy(obj); }
    }

    private IEnumerator CameraZoomOut()
    {
        if (mainCam == null) yield break;
        float targetFOV = originalFOV + 15f, t = 0f, dur = 0.6f, startFOV = mainCam.fieldOfView;
        while (t < dur)
        {
            t += Time.deltaTime;
            mainCam.fieldOfView = Mathf.Lerp(startFOV, targetFOV, t / dur);
            yield return null;
        }
    }

    private void CleanConfetti()
    {
        foreach (var obj in confetti3D) if (obj != null) Destroy(obj);
        confetti3D.Clear();
    }

    private void OnClickSiguiente()
    {
        HideImmediate();
        if (mainCam != null) mainCam.fieldOfView = originalFOV;
        GameController.Instance?.StartLevel(GameManager.Instance?.currentLevel ?? 1);
    }

    private void OnClickMenu()
    {
        HideImmediate();
        if (mainCam != null) mainCam.fieldOfView = originalFOV;
        SceneLoader.Instance?.GoToMainMenu();
    }

    // ══════════════════════════════════════════════════════════════════════════
    //  HELPERS
    // ══════════════════════════════════════════════════════════════════════════
    private static float EaseOutBack(float x)
    {
        float c1 = 1.70158f, c3 = c1 + 1f;
        return Mathf.Clamp01(1f + c3 * Mathf.Pow(x-1f,3f) + c1 * Mathf.Pow(x-1f,2f));
    }

    private static GameObject MakeStretch(string name, Transform parent)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
        rt.offsetMin = rt.offsetMax = Vector2.zero;
        return go;
    }

    private static TextMeshProUGUI MakeTMP(string name, Transform parent,
        string text, float size, FontStyles style, Color color,
        Vector2 anchorMinMax, Vector2 ancPos, Vector2 sizeDelta,
        TextAlignmentOptions align)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = anchorMinMax;
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
        tmp.alignment = TextAlignmentOptions.Center; tmp.raycastTarget = false;
        var btn = go.AddComponent<Button>();
        btn.targetGraphic = img;
        var colors = btn.colors;
        colors.normalColor      = colorNormal;
        colors.highlightedColor = colorHover;
        colors.pressedColor     = colorNormal * 0.7f;
        btn.colors = colors;
        return btn;
    }

    private static void Separator(Transform parent, Vector2 ancPos)
    {
        var go = new GameObject("Sep");
        go.transform.SetParent(parent, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 1f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = ancPos; rt.sizeDelta = new Vector2(820f, 2f);
        go.AddComponent<Image>().color = new Color(0.4f, 0.3f, 1f, 0.4f);
    }

    private void OnDestroy()
    {
        if (starRT != null) { starRT.Release(); Destroy(starRT); }
    }
}

using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// LoginScreen — Pantalla flotante de login/registro con Firebase REST API.
/// </summary>
public class LoginScreen : MonoBehaviour
{
    private static LoginScreen _instance;
    public static LoginScreen Instance 
    { 
        get 
        {
            if (_instance == null)
            {
                var go = new GameObject("LoginScreenManager");
                _instance = go.AddComponent<LoginScreen>();
            }
            return _instance;
        } 
    }

    private Canvas canvas;
    private GameObject panel;
    private TMP_InputField inputEmail;
    private TMP_InputField inputPassword;
    private TextMeshProUGUI txtStatus;
    private TextMeshProUGUI txtTitle;
    private Button btnAction;
    private Button btnGoogle;
    private Button btnSwitch;
    private Button btnClose;
    private Button btnReset;

    private bool isLoginMode = true;

    private void Awake()
    {
        if (_instance != null && _instance != this) { Destroy(gameObject); return; }
        _instance = this;
        DontDestroyOnLoad(gameObject);
        BuildUI();
        Hide();
    }

    public void Show(bool loginMode = true)
    {
        isLoginMode = loginMode;
        UpdateMode();
        canvas.gameObject.SetActive(true);
        if (txtStatus != null) txtStatus.text = "";
    }

    public void Hide()
    {
        if (canvas != null) canvas.gameObject.SetActive(false);
    }

    private void OnClickAction()
    {
        string email = inputEmail?.text.Trim() ?? "";
        string pass  = inputPassword?.text ?? "";

        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(pass))
        { ShowStatus("Completa todos los campos.", true); return; }

        SetInteractable(false);
        ShowStatus("Procesando...", false);

        if (isLoginMode)
        {
            FirebaseManager.Instance?.LoginWithEmail(email, pass,
                onSuccess: () => { Hide(); SetInteractable(true); },
                onError:   err => { ShowStatus(err, true); SetInteractable(true); });
        }
        else
        {
            FirebaseManager.Instance?.RegisterWithEmail(email, pass,
                onSuccess: () => { Hide(); SetInteractable(true); },
                onError:   err => { ShowStatus(err, true); SetInteractable(true); });
        }
    }

    private void OnClickGoogle()
    {
        SetInteractable(false);
        ShowStatus("Abriendo Google...", false);

        FirebaseManager.Instance?.LoginWithGoogle(
            onSuccess: () => { Hide(); SetInteractable(true); },
            onError:   err => { ShowStatus(err, true); SetInteractable(true); });
    }

    private void OnClickSwitch()
    {
        isLoginMode = !isLoginMode;
        UpdateMode();
        if (txtStatus != null) txtStatus.text = "";
    }

    private void OnClickReset()
    {
        string email = inputEmail?.text.Trim() ?? "";
        if (string.IsNullOrEmpty(email))
        { ShowStatus("Escribe tu email primero.", true); return; }
        FirebaseManager.Instance?.SendPasswordReset(email,
            onSuccess: () => ShowStatus("Email enviado. Revisa tu bandeja.", false),
            onError:   err => ShowStatus(err, true));
    }

    private void UpdateMode()
    {
        if (txtTitle != null) txtTitle.text = isLoginMode ? "INICIAR SESIÓN" : "CREAR CUENTA";
        if (btnAction != null)
        {
            var lbl = btnAction.GetComponentInChildren<TextMeshProUGUI>();
            if (lbl != null) lbl.text = isLoginMode ? "ENTRAR" : "REGISTRARSE";
        }
        if (btnSwitch != null)
        {
            var lbl = btnSwitch.GetComponentInChildren<TextMeshProUGUI>();
            if (lbl != null) lbl.text = isLoginMode
                ? "No tienes cuenta? Registrate"
                : "Ya tienes cuenta? Inicia sesion";
        }
        if (btnReset != null) btnReset.gameObject.SetActive(isLoginMode);
    }

    private void ShowStatus(string msg, bool isError)
    {
        if (txtStatus == null) return;
        txtStatus.text  = msg;
        txtStatus.color = isError ? new Color(1f, 0.3f, 0.3f) : new Color(0.3f, 1f, 0.5f);
    }

    private void SetInteractable(bool val)
    {
        if (btnAction    != null) btnAction.interactable    = val;
        if (btnGoogle    != null) btnGoogle.interactable    = val;
        if (btnSwitch    != null) btnSwitch.interactable    = val;
        if (inputEmail   != null) inputEmail.interactable   = val;
        if (inputPassword!= null) inputPassword.interactable= val;
    }

    private void BuildUI()
    {
        var cGO = new GameObject("LoginCanvas");
        cGO.transform.SetParent(transform, false);
        DontDestroyOnLoad(cGO);
        canvas = cGO.AddComponent<Canvas>();
        canvas.renderMode   = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 500;
        var scaler = cGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode         = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080, 1920);
        scaler.matchWidthOrHeight  = 0.5f;
        cGO.AddComponent<GraphicRaycaster>();

        // Fondo
        var bgGO = new GameObject("BG");
        bgGO.transform.SetParent(cGO.transform, false);
        var bgRT = bgGO.AddComponent<RectTransform>();
        bgRT.anchorMin = Vector2.zero; bgRT.anchorMax = Vector2.one;
        bgRT.offsetMin = bgRT.offsetMax = Vector2.zero;
        bgGO.AddComponent<Image>().color = new Color(0,0,0,0.85f);

        // Panel
        panel = new GameObject("Panel");
        panel.transform.SetParent(cGO.transform, false);
        var pRT = panel.AddComponent<RectTransform>();
        pRT.anchorMin = pRT.anchorMax = new Vector2(0.5f, 0.5f);
        pRT.sizeDelta = new Vector2(900f, 1150f);
        pRT.anchoredPosition = Vector2.zero;
        panel.AddComponent<Image>().color = new Color(0.08f, 0.06f, 0.15f, 0.98f);

        txtTitle = MakeTMP("Title", panel.transform, "INICIAR SESION", 54f,
            FontStyles.Bold, new Color(0.4f, 0.8f, 1f),
            new Vector2(0.5f, 1f), new Vector2(0f, -80f), new Vector2(860f, 80f));

        MakeSep(panel.transform, new Vector2(0f, -175f));

        inputEmail    = MakeInput("Email",    panel.transform, "Correo electronico",
            new Vector2(0f, -230f), new Vector2(820f, 90f), false);
        inputPassword = MakeInput("Password", panel.transform, "Contrasena (min. 6 caracteres)",
            new Vector2(0f, -340f), new Vector2(820f, 90f), true);

        txtStatus = MakeTMP("Status", panel.transform, "", 30f, FontStyles.Normal,
            new Color(1f, 0.4f, 0.4f),
            new Vector2(0.5f, 1f), new Vector2(0f, -450f), new Vector2(820f, 80f));
        txtStatus.alignment = TextAlignmentOptions.Center;

        btnAction = MakeBtn("BtnAction", panel.transform, "ENTRAR",
            new Vector2(0f, -550f), new Vector2(820f, 110f),
            new Color(0.1f, 0.5f, 0.9f), 40f);
        btnAction.onClick.AddListener(OnClickAction);

        btnGoogle = MakeBtn("BtnGoogle", panel.transform, "CONTINUAR CON GOOGLE",
            new Vector2(0f, -680f), new Vector2(820f, 110f),
            new Color(0.85f, 0.3f, 0.25f), 40f);
        btnGoogle.onClick.AddListener(OnClickGoogle);

        btnSwitch = MakeBtn("BtnSwitch", panel.transform, "No tienes cuenta? Registrate",
            new Vector2(0f, -810f), new Vector2(820f, 70f),
            new Color(0.15f, 0.15f, 0.25f), 28f);
        btnSwitch.onClick.AddListener(OnClickSwitch);

        btnReset = MakeBtn("BtnReset", panel.transform, "Olvidaste tu contrasena?",
            new Vector2(0f, -895f), new Vector2(820f, 60f),
            new Color(0.1f, 0.1f, 0.15f), 24f);
        btnReset.onClick.AddListener(OnClickReset);

        btnClose = MakeBtn("BtnClose", panel.transform, "Cancelar",
            new Vector2(0f, -1000f), new Vector2(820f, 70f),
            new Color(0.2f, 0.08f, 0.08f), 28f);
        btnClose.onClick.AddListener(Hide);
    }

    private TextMeshProUGUI MakeTMP(string name, Transform parent, string text,
        float size, FontStyles style, Color color,
        Vector2 anchor, Vector2 pos, Vector2 sizeDelta)
    {
        var go = new GameObject(name); go.transform.SetParent(parent, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = anchor; rt.pivot = new Vector2(0.5f, 1f);
        rt.anchoredPosition = pos; rt.sizeDelta = sizeDelta;
        var tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text = text; tmp.fontSize = size; tmp.fontStyle = style;
        tmp.color = color; tmp.alignment = TextAlignmentOptions.Center;
        return tmp;
    }

    private TMP_InputField MakeInput(string name, Transform parent, string placeholder,
        Vector2 pos, Vector2 size, bool isPassword)
    {
        var go = new GameObject(name); go.transform.SetParent(parent, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 1f); rt.pivot = new Vector2(0.5f, 1f);
        rt.anchoredPosition = pos; rt.sizeDelta = size;
        go.AddComponent<Image>().color = new Color(0.15f, 0.12f, 0.25f);

        var area = new GameObject("TextArea"); area.transform.SetParent(go.transform, false);
        var aRT = area.AddComponent<RectTransform>();
        aRT.anchorMin = Vector2.zero; aRT.anchorMax = Vector2.one;
        aRT.offsetMin = new Vector2(15,5); aRT.offsetMax = new Vector2(-15,-5);
        area.AddComponent<RectMask2D>();

        var phGO = new GameObject("PH"); phGO.transform.SetParent(area.transform, false);
        var phRT = phGO.AddComponent<RectTransform>();
        phRT.anchorMin = Vector2.zero; phRT.anchorMax = Vector2.one;
        phRT.offsetMin = phRT.offsetMax = Vector2.zero;
        var phT = phGO.AddComponent<TextMeshProUGUI>();
        phT.text = placeholder; phT.fontSize = 28f;
        phT.color = new Color(0.5f,0.5f,0.6f);
        phT.alignment = TextAlignmentOptions.MidlineLeft;

        var tGO = new GameObject("Txt"); tGO.transform.SetParent(area.transform, false);
        var tRT = tGO.AddComponent<RectTransform>();
        tRT.anchorMin = Vector2.zero; tRT.anchorMax = Vector2.one;
        tRT.offsetMin = tRT.offsetMax = Vector2.zero;
        var tT = tGO.AddComponent<TextMeshProUGUI>();
        tT.fontSize = 30f; tT.color = Color.white;
        tT.alignment = TextAlignmentOptions.MidlineLeft;

        var field = go.AddComponent<TMP_InputField>();
        field.textViewport  = aRT;
        field.textComponent = tT;
        field.placeholder   = phT;
        if (isPassword)
        {
            field.contentType = TMP_InputField.ContentType.Password;
            field.inputType   = TMP_InputField.InputType.Password;
        }
        else
        {
            field.contentType = TMP_InputField.ContentType.EmailAddress;
        }
        return field;
    }

    private Button MakeBtn(string name, Transform parent, string label,
        Vector2 pos, Vector2 size, Color color, float fontSize)
    {
        var go = new GameObject(name); go.transform.SetParent(parent, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 1f); rt.pivot = new Vector2(0.5f, 1f);
        rt.anchoredPosition = pos; rt.sizeDelta = size;
        var img = go.AddComponent<Image>(); img.color = color;
        var lbl = new GameObject("Lbl"); lbl.transform.SetParent(go.transform, false);
        var lRT = lbl.AddComponent<RectTransform>();
        lRT.anchorMin = Vector2.zero; lRT.anchorMax = Vector2.one;
        lRT.offsetMin = lRT.offsetMax = Vector2.zero;
        var tmp = lbl.AddComponent<TextMeshProUGUI>();
        tmp.text = label; tmp.fontSize = fontSize;
        tmp.fontStyle = FontStyles.Bold; tmp.color = Color.white;
        tmp.alignment = TextAlignmentOptions.Center; tmp.raycastTarget = false;
        var btn = go.AddComponent<Button>(); btn.targetGraphic = img;
        var c = btn.colors;
        c.normalColor = color; c.highlightedColor = color * 1.3f; c.pressedColor = color * 0.7f;
        btn.colors = c;
        return btn;
    }

    private void MakeSep(Transform parent, Vector2 pos)
    {
        var go = new GameObject("Sep"); go.transform.SetParent(parent, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 1f); rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = pos; rt.sizeDelta = new Vector2(820f, 2f);
        go.AddComponent<Image>().color = new Color(0.3f, 0.3f, 0.5f, 0.5f);
    }
}

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

[RequireComponent(typeof(RectTransform))]
public class DPadController : MonoBehaviour
{
    [Header("Botones (se asignan automáticamente)")]
    public Button botonArriba;
    public Button botonAbajo;
    public Button botonIzquierda;
    public Button botonDerecha;

    [Header("Movimiento continuo")]
    public bool  holdToMove     = true;
    public float holdDelay      = 0.25f;
    public float holdRepeatRate = 0.12f;

    private Vector3 heldDirectionLocal = Vector3.zero; // dirección local (relativa a cámara)
    private float   holdTimer     = 0f;
    private float   repeatTimer   = 0f;
    private bool    isHolding     = false;

    private void Awake()
    {
        var rt = GetComponent<RectTransform>();
        rt.anchorMin        = new Vector2(0f, 0f);
        rt.anchorMax        = new Vector2(0f, 0f);
        rt.pivot            = new Vector2(0f, 0f);
        rt.anchoredPosition = new Vector2(40f, 40f);
        rt.sizeDelta        = new Vector2(240f, 240f);

        if (botonArriba    == null) botonArriba    = CrearBoton("BtnArriba",    new Vector2( 80f, 165f), "^");
        if (botonAbajo     == null) botonAbajo     = CrearBoton("BtnAbajo",     new Vector2( 80f,   5f), "v");
        if (botonIzquierda == null) botonIzquierda = CrearBoton("BtnIzquierda", new Vector2(  5f,  85f), "<");
        if (botonDerecha   == null) botonDerecha   = CrearBoton("BtnDerecha",   new Vector2(155f,  85f), ">");
    }

    private void Start()
    {
        // Guardamos la dirección local — se convierte a mundo en el momento del movimiento
        ConectarBoton(botonArriba,    Vector3.forward);
        ConectarBoton(botonAbajo,     Vector3.back);
        ConectarBoton(botonIzquierda, Vector3.left);
        ConectarBoton(botonDerecha,   Vector3.right);
    }

    private void Update()
    {
        if (!holdToMove || !isHolding) return;
        holdTimer += Time.deltaTime;
        if (holdTimer < holdDelay) return;
        repeatTimer += Time.deltaTime;
        if (repeatTimer >= holdRepeatRate)
        {
            repeatTimer = 0f;
            PlayerController.Instance?.MoveInDirection(CameraRelativeDir(heldDirectionLocal));
        }
    }

    // ── Convierte dirección local de cámara a dirección del mundo en el plano XZ ──
    private Vector3 CameraRelativeDir(Vector3 localDir)
    {
        var cam = Camera.main;
        if (cam == null) return localDir;

        // Forward de la cámara proyectado en el plano horizontal
        Vector3 camForward = cam.transform.forward;
        camForward.y = 0f;
        camForward.Normalize();

        Vector3 camRight = cam.transform.right;
        camRight.y = 0f;
        camRight.Normalize();

        // Combinar según la dirección local
        Vector3 worldDir = camForward * localDir.z + camRight * localDir.x;

        // Snap a los 4 ejes del laberinto (forward/back/left/right)
        return SnapToAxis(worldDir);
    }

    // Redondea la dirección al eje más cercano (evita diagonales)
    private Vector3 SnapToAxis(Vector3 dir)
    {
        if (Mathf.Abs(dir.x) >= Mathf.Abs(dir.z))
            return new Vector3(Mathf.Sign(dir.x), 0f, 0f);
        else
            return new Vector3(0f, 0f, Mathf.Sign(dir.z));
    }

    private Button CrearBoton(string nombre, Vector2 pos, string simbolo)
    {
        var go = new GameObject(nombre);
        go.transform.SetParent(transform, false);

        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin        = Vector2.zero;
        rt.anchorMax        = Vector2.zero;
        rt.pivot            = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = pos;
        rt.sizeDelta        = new Vector2(75f, 75f);

        var img   = go.AddComponent<Image>();
        img.color = new Color(0.2f, 0.2f, 0.6f, 0.7f);

        var textoGO = new GameObject("Label");
        textoGO.transform.SetParent(go.transform, false);
        var textoRT = textoGO.AddComponent<RectTransform>();
        textoRT.anchorMin = Vector2.zero;
        textoRT.anchorMax = Vector2.one;
        textoRT.offsetMin = Vector2.zero;
        textoRT.offsetMax = Vector2.zero;

        var tmp       = textoGO.AddComponent<TMPro.TextMeshProUGUI>();
        tmp.text      = simbolo;
        tmp.fontSize  = 42f;
        tmp.fontStyle = TMPro.FontStyles.Bold;
        tmp.color     = Color.white;
        tmp.alignment = TMPro.TextAlignmentOptions.Center;

        var btn             = go.AddComponent<Button>();
        var colors          = btn.colors;
        colors.normalColor  = Color.white;
        colors.pressedColor = new Color(0.6f, 0.6f, 1f, 1f);
        btn.colors          = colors;
        btn.targetGraphic   = img;

        return btn;
    }

    private void ConectarBoton(Button btn, Vector3 localDirection)
    {
        if (btn == null) return;

        btn.onClick.AddListener(() =>
            PlayerController.Instance?.MoveInDirection(CameraRelativeDir(localDirection)));

        var trigger = btn.gameObject.GetComponent<EventTrigger>()
                   ?? btn.gameObject.AddComponent<EventTrigger>();

        Agregar(trigger, EventTriggerType.PointerDown, (_) =>
        {
            heldDirectionLocal = localDirection;
            holdTimer = repeatTimer = 0f;
            isHolding = true;
        });
        Agregar(trigger, EventTriggerType.PointerUp,   (_) => { isHolding = false; });
        Agregar(trigger, EventTriggerType.PointerExit, (_) => { isHolding = false; });
    }

    private void Agregar(EventTrigger t, EventTriggerType tipo,
                         UnityEngine.Events.UnityAction<BaseEventData> cb)
    {
        var e = new EventTrigger.Entry { eventID = tipo };
        e.callback.AddListener(cb);
        t.triggers.Add(e);
    }
}

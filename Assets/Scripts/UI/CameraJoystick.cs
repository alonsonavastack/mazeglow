using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

/// <summary>
/// CameraJoystick — Joystick virtual en la derecha de la pantalla.
/// Arrastrarlo izquierda/derecha rota la cámara alrededor del jugador.
/// Arrastrarlo arriba/abajo ajusta el ángulo de inclinación (tilt).
/// </summary>
public class CameraJoystick : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    [Header("Visual del joystick")]
    public Image fondoJoystick;    // Círculo grande exterior
    public Image palancaJoystick;  // Círculo pequeño que se mueve

    [Header("Sensibilidad")]
    public float sensibilidadRotacion = 80f;   // grados por segundo al mover el joystick
    public float sensibilidadTilt     = 30f;
    public float suavidadRetorno      = 8f;    // qué tan rápido regresa al centro

    [Header("Límites de inclinación")]
    public float tiltMin = 30f;    // mínimo (casi horizontal)
    public float tiltMax = 85f;    // máximo (casi cenital)

    private Vector2  inputVector    = Vector2.zero;  // -1 a 1 en X e Y
    private Vector2  palancaOffset  = Vector2.zero;
    private bool     tocando        = false;
    private float    radioJoystick  = 60f;           // radio máximo de movimiento de la palanca

    private MazeCameraController camController;

    // Estado actual de la cámara
    private float anguloY   = 0f;    // rotación horizontal alrededor del jugador
    private float tiltActual = 58f;  // inclinación vertical

    private void Awake()
    {
        // Auto-construir si no hay referencias asignadas
        if (fondoJoystick == null) ConstruirJoystick();
    }

    private void Start()
    {
        camController = FindAnyObjectByType<MazeCameraController>();
        anguloY       = 0f;
        tiltActual    = 58f;
    }

    private void Update()
    {
        if (!tocando)
        {
            // Palanca regresa al centro suavemente
            palancaOffset = Vector2.Lerp(palancaOffset, Vector2.zero, Time.deltaTime * suavidadRetorno);
            inputVector   = Vector2.Lerp(inputVector,   Vector2.zero, Time.deltaTime * suavidadRetorno);
            ActualizarPosicionPalanca();
        }

        if (inputVector.magnitude < 0.05f) return;

        // Rotar ángulo horizontal con el eje X del joystick
        anguloY   += inputVector.x * sensibilidadRotacion * Time.deltaTime;

        // Ajustar tilt con el eje Y del joystick (invertido: arrastrar arriba = más cenital)
        tiltActual -= inputVector.y * sensibilidadTilt * Time.deltaTime;
        tiltActual  = Mathf.Clamp(tiltActual, tiltMin, tiltMax);

        // Aplicar a la cámara
        AplicarRotacionCamara();
    }

    // ── Eventos de toque ─────────────────────────────────────────────────────────
    public void OnPointerDown(PointerEventData eventData)
    {
        tocando = true;
        ActualizarInput(eventData);
    }

    public void OnDrag(PointerEventData eventData)
    {
        ActualizarInput(eventData);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        tocando       = false;
        inputVector   = Vector2.zero;
        palancaOffset = Vector2.zero;
        ActualizarPosicionPalanca();
    }

    private void ActualizarInput(PointerEventData eventData)
    {
        // Calcular offset desde el centro del fondo
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            fondoJoystick.rectTransform,
            eventData.position,
            eventData.pressEventCamera,
            out Vector2 localPoint);

        // Limitar al radio del joystick
        palancaOffset = Vector2.ClampMagnitude(localPoint, radioJoystick);
        inputVector   = palancaOffset / radioJoystick;

        ActualizarPosicionPalanca();
    }

    private void ActualizarPosicionPalanca()
    {
        if (palancaJoystick != null)
            palancaJoystick.rectTransform.anchoredPosition = palancaOffset;
    }

    // ── Aplicar rotación a la cámara ─────────────────────────────────────────────
    private void AplicarRotacionCamara()
    {
        if (camController == null) return;
        var player = PlayerController.Instance;
        if (player == null) return;

        // Usar los valores isoHeight e isoYaw del MazeCameraController
        float height = camController.isoHeight;
        float dist   = camController.isoDistance;
        float rad    = dist * Mathf.Cos(tiltActual * Mathf.Deg2Rad);

        float angleRad = anguloY * Mathf.Deg2Rad;
        Vector3 pivot  = player.transform.position;
        Vector3 camPos = new Vector3(
            pivot.x + Mathf.Sin(angleRad) * rad,
            height,
            pivot.z - Mathf.Cos(angleRad) * rad);

        camController.transform.position = camPos;
        camController.transform.rotation = Quaternion.Euler(tiltActual, anguloY, 0f);
    }

    // ── Construir joystick visual por código ──────────────────────────────────────
    private void ConstruirJoystick()
    {
        var rt = GetComponent<RectTransform>();
        if (rt == null) rt = gameObject.AddComponent<RectTransform>();

        // Posición: esquina inferior derecha
        rt.anchorMin        = new Vector2(1f, 0f);
        rt.anchorMax        = new Vector2(1f, 0f);
        rt.pivot            = new Vector2(1f, 0f);
        rt.anchoredPosition = new Vector2(-40f, 40f);
        rt.sizeDelta        = new Vector2(160f, 160f);

        // Fondo del joystick
        var fondoGO = new GameObject("FondoJoystick");
        fondoGO.transform.SetParent(transform, false);
        var fondoRT = fondoGO.AddComponent<RectTransform>();
        fondoRT.anchorMin = new Vector2(0.5f, 0.5f);
        fondoRT.anchorMax = new Vector2(0.5f, 0.5f);
        fondoRT.pivot     = new Vector2(0.5f, 0.5f);
        fondoRT.anchoredPosition = Vector2.zero;
        fondoRT.sizeDelta = new Vector2(140f, 140f);
        fondoJoystick     = fondoGO.AddComponent<Image>();
        fondoJoystick.color = new Color(0.2f, 0.2f, 0.6f, 0.4f);

        // Ícono de rotación (texto)
        var iconoGO = new GameObject("Icono");
        iconoGO.transform.SetParent(fondoGO.transform, false);
        var iconoRT = iconoGO.AddComponent<RectTransform>();
        iconoRT.anchorMin = Vector2.zero;
        iconoRT.anchorMax = Vector2.one;
        iconoRT.offsetMin = Vector2.zero;
        iconoRT.offsetMax = Vector2.zero;
        var tmp       = iconoGO.AddComponent<TMPro.TextMeshProUGUI>();
        tmp.text      = "360";
        tmp.fontSize  = 22f;
        tmp.color     = new Color(1f, 1f, 1f, 0.5f);
        tmp.alignment = TMPro.TextAlignmentOptions.Center;

        // Palanca (punto que se mueve)
        var palancaGO = new GameObject("Palanca");
        palancaGO.transform.SetParent(fondoGO.transform, false);
        var palancaRT = palancaGO.AddComponent<RectTransform>();
        palancaRT.anchorMin = new Vector2(0.5f, 0.5f);
        palancaRT.anchorMax = new Vector2(0.5f, 0.5f);
        palancaRT.pivot     = new Vector2(0.5f, 0.5f);
        palancaRT.anchoredPosition = Vector2.zero;
        palancaRT.sizeDelta = new Vector2(55f, 55f);
        palancaJoystick     = palancaGO.AddComponent<Image>();
        palancaJoystick.color = new Color(0.5f, 0.5f, 1f, 0.85f);

        radioJoystick = 50f;

        // Agregar EventTrigger al fondo para recibir los toques
        var et = fondoGO.AddComponent<EventTrigger>();
        // (los eventos se manejan en este componente — el fondoGO necesita Image para recibir raycast)
        fondoJoystick.raycastTarget = true;
    }
}

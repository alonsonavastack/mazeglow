using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// CelebrationFX v2 — Todo por código, sin prefabs externos.
/// • Muestra "¡COMPLETADO!" en pantalla
/// • Lanza confeti desde ambos extremos
/// • Pasa al siguiente nivel en 2 segundos
/// </summary>
public class CelebrationFX : MonoBehaviour
{
    // ── Partículas de confeti opcionales (si tienes prefab) ──────────────────────
    public ParticleSystem confettiPS;
    public ParticleSystem glowPS;

    // ── Estado interno ────────────────────────────────────────────────────────────
    private Canvas       celebCanvas;
    private TextMeshProUGUI completadoText;
    private List<GameObject> confettiPieces = new List<GameObject>();
    private bool isPlaying = false;

    // Colores del confeti
    private static readonly Color[] confettiColors = {
        new Color(1f,   0.2f, 0.2f),   // rojo
        new Color(1f,   0.85f, 0f),    // amarillo
        new Color(0.2f, 0.85f, 0.2f),  // verde
        new Color(0.2f, 0.5f, 1f),     // azul
        new Color(1f,   0.4f, 0.9f),   // rosa
        new Color(1f,   0.6f, 0.1f),   // naranja
        new Color(0.8f, 0.3f, 1f),     // morado
    };

    private void Awake()
    {
        BuildCanvas();
    }

    // ── Construir canvas de celebración ──────────────────────────────────────────
    private void BuildCanvas()
    {
        var go = new GameObject("CelebrationCanvas");
        go.transform.SetParent(transform, false);
        DontDestroyOnLoad(go);

        celebCanvas = go.AddComponent<Canvas>();
        celebCanvas.renderMode  = RenderMode.ScreenSpaceOverlay;
        celebCanvas.sortingOrder = 200;

        var scaler = go.AddComponent<CanvasScaler>();
        scaler.uiScaleMode         = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080, 1920);

        go.AddComponent<GraphicRaycaster>();

        // ── Texto "¡COMPLETADO!" ──────────────────────────────────────────────────
        var textGO = new GameObject("TextCompletado");
        textGO.transform.SetParent(go.transform, false);

        var rt = textGO.AddComponent<RectTransform>();
        rt.anchorMin        = new Vector2(0.5f, 0.55f);
        rt.anchorMax        = new Vector2(0.5f, 0.55f);
        rt.pivot            = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = Vector2.zero;
        rt.sizeDelta        = new Vector2(900f, 200f);

        completadoText = textGO.AddComponent<TextMeshProUGUI>();
        completadoText.text      = "¡COMPLETADO!";
        completadoText.fontSize  = 80f;
        completadoText.fontStyle = FontStyles.Bold;
        completadoText.color     = new Color(1f, 0.92f, 0.1f);
        completadoText.alignment = TextAlignmentOptions.Center;

        // Sombra para legibilidad
        var shadow = textGO.AddComponent<Shadow>();
        shadow.effectColor    = new Color(0f, 0f, 0f, 0.8f);
        shadow.effectDistance = new Vector2(4f, -4f);

        go.SetActive(false);
        celebCanvas.gameObject.SetActive(false);
    }

    // ── Play — llamar desde GameController ────────────────────────────────────────
    public void Play()
    {
        if (isPlaying) return;
        isPlaying = true;

        // Partículas opcionales
        confettiPS?.Play();
        glowPS?.Play();

        StartCoroutine(CelebrationRoutine());
    }

    public void Stop()
    {
        confettiPS?.Stop();
        glowPS?.Stop();
        StopAllCoroutines();
        LimpiarConfeti();
        if (celebCanvas != null) celebCanvas.gameObject.SetActive(false);
        isPlaying = false;
    }

    // ── Rutina principal ──────────────────────────────────────────────────────────
    private IEnumerator CelebrationRoutine()
    {
        // Mostrar canvas
        celebCanvas.gameObject.SetActive(true);

        // Animación de entrada del texto (escala)
        yield return StartCoroutine(AnimarTexto());

        // Lanzar confeti desde ambos extremos durante 1.5 seg
        StartCoroutine(LanzarConfeti());

        // Esperar 2 segundos y pasar al siguiente nivel
        yield return new WaitForSeconds(2f);

        // Ocultar
        celebCanvas.gameObject.SetActive(false);
        LimpiarConfeti();
        isPlaying = false;

        // Siguiente nivel
        GameController.Instance?.StartLevel(GameManager.Instance?.currentLevel ?? 1);
    }

    // ── Animación de escala del texto ─────────────────────────────────────────────
    private IEnumerator AnimarTexto()
    {
        float dur = 0.4f;
        float t   = 0f;
        var rt    = completadoText.rectTransform;

        while (t < dur)
        {
            t += Time.deltaTime;
            float p     = t / dur;
            float scale = Mathf.LerpUnclamped(0f, 1f,
                          EaseOutBack(p));
            rt.localScale = Vector3.one * scale;
            yield return null;
        }
        rt.localScale = Vector3.one;
    }

    private float EaseOutBack(float x)
    {
        float c1 = 1.70158f;
        float c3 = c1 + 1f;
        return 1f + c3 * Mathf.Pow(x - 1f, 3f) + c1 * Mathf.Pow(x - 1f, 2f);
    }

    // ── Confeti por código ────────────────────────────────────────────────────────
    private IEnumerator LanzarConfeti()
    {
        float duracion  = 1.8f;
        float intervalo = 0.06f;
        float elapsed   = 0f;

        while (elapsed < duracion)
        {
            // Extremo izquierdo
            SpawnPieza(new Vector2(0.05f, 0.85f), Vector2.right);
            SpawnPieza(new Vector2(0.05f, 0.90f), new Vector2(0.8f, 1f));
            SpawnPieza(new Vector2(0.05f, 0.80f), new Vector2(0.6f, 0.9f));

            // Extremo derecho
            SpawnPieza(new Vector2(0.95f, 0.85f), Vector2.left);
            SpawnPieza(new Vector2(0.95f, 0.90f), new Vector2(-0.8f, 1f));
            SpawnPieza(new Vector2(0.95f, 0.80f), new Vector2(-0.6f, 0.9f));

            elapsed += intervalo;
            yield return new WaitForSeconds(intervalo);
        }
    }

    private void SpawnPieza(Vector2 anchorPos, Vector2 direction)
    {
        var go = new GameObject("Confeti");
        go.transform.SetParent(celebCanvas.transform, false);

        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin        = anchorPos;
        rt.anchorMax        = anchorPos;
        rt.pivot            = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = Vector2.zero;
        rt.sizeDelta        = new Vector2(
            Random.Range(18f, 32f),
            Random.Range(10f, 20f));
        rt.localRotation = Quaternion.Euler(0f, 0f, Random.Range(0f, 360f));

        var img = go.AddComponent<Image>();
        img.color = confettiColors[Random.Range(0, confettiColors.Length)];

        confettiPieces.Add(go);
        StartCoroutine(AnimarPieza(rt, direction));
    }

    private IEnumerator AnimarPieza(RectTransform rt, Vector2 dir)
    {
        if (rt == null) yield break;

        float speed  = Random.Range(400f, 800f);
        float gravity = Random.Range(600f, 1000f);
        float rotSpd  = Random.Range(-300f, 300f);
        float life    = Random.Range(1.0f, 2.0f);
        float t       = 0f;
        Vector2 vel   = dir.normalized * speed;
        vel.x        += Random.Range(-150f, 150f);
        vel.y        += Random.Range(100f, 400f);

        Color startColor = rt.GetComponent<Image>().color;

        while (t < life && rt != null)
        {
            t            += Time.deltaTime;
            vel.y        -= gravity * Time.deltaTime;
            rt.anchoredPosition += vel * Time.deltaTime;
            rt.Rotate(0f, 0f, rotSpd * Time.deltaTime);

            // Fade out al final
            float alpha = Mathf.Clamp01(1f - (t / life) * 0.6f);
            var img = rt.GetComponent<Image>();
            if (img != null) img.color = new Color(startColor.r, startColor.g, startColor.b, alpha);

            yield return null;
        }

        if (rt != null) Destroy(rt.gameObject);
    }

    private void LimpiarConfeti()
    {
        foreach (var p in confettiPieces)
            if (p != null) Destroy(p);
        confettiPieces.Clear();
    }
}

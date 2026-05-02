using UnityEngine;
using System.Collections;

/// <summary>
/// Star3D — Estrella 3D individual que se anima al ganar un nivel.
/// • Usa el mesh Estrella_MazeGlow.obj que está en Assets/Materials/
/// • Tiene animación de entrada (caída con rebote), brillo pulsante y rotación
/// • Se renderiza en WorldSpace frente a la cámara de UI 3D
/// </summary>
public class Star3D : MonoBehaviour
{
    // ── Materiales ─────────────────────────────────────────────────────────────
    private Material matEarned;   // dorado brillante
    private Material matEmpty;    // gris oscuro

    // ── Estado ─────────────────────────────────────────────────────────────────
    private bool earned = false;
    private bool animating = false;
    private MeshRenderer meshRend;

    // ── Colores ────────────────────────────────────────────────────────────────
    private static readonly Color ColorGold    = new Color(1.0f, 0.82f, 0.05f);
    private static readonly Color ColorGoldEmi = new Color(1.0f, 0.60f, 0.00f);
    private static readonly Color ColorGray    = new Color(0.22f, 0.22f, 0.30f);

    private void Awake()
    {
        meshRend = GetComponent<MeshRenderer>();
        if (meshRend == null) meshRend = GetComponentInChildren<MeshRenderer>();

        BuildMaterials();
        SetEmpty();
    }

    private void BuildMaterials()
    {
        // Material dorado con emisión
        matEarned = new Material(Shader.Find("Standard"));
        matEarned.color = ColorGold;
        matEarned.EnableKeyword("_EMISSION");
        matEarned.SetColor("_EmissionColor", ColorGoldEmi * 1.5f);
        matEarned.SetFloat("_Glossiness", 0.9f);
        matEarned.SetFloat("_Metallic", 0.4f);

        // Material gris oscuro sin brillo
        matEmpty = new Material(Shader.Find("Standard"));
        matEmpty.color = ColorGray;
        matEmpty.SetFloat("_Glossiness", 0.1f);
        matEmpty.SetFloat("_Metallic", 0f);
    }

    // ── API pública ─────────────────────────────────────────────────────────────

    public void SetEmpty()
    {
        earned = false;
        if (meshRend != null) meshRend.material = matEmpty;
    }

    /// <summary>
    /// Anima la estrella: cae desde arriba, aterriza con rebote y se vuelve dorada.
    /// </summary>
    public IEnumerator AnimateEarn(bool isEarned, float delay = 0f)
    {
        yield return new WaitForSeconds(delay);

        // --- Caída desde arriba ---
        Vector3 endPos = transform.localPosition;
        Vector3 startPos = endPos + Vector3.up * 3f;
        transform.localPosition = startPos;
        transform.localScale = Vector3.one * 1.6f;

        float t = 0f, dur = 0.45f;
        while (t < dur)
        {
            t += Time.deltaTime;
            float p = t / dur;
            transform.localPosition = Vector3.Lerp(startPos, endPos, EaseOutBounce(p));
            transform.localScale    = Vector3.Lerp(Vector3.one * 1.6f, Vector3.one, p);
            yield return null;
        }

        transform.localPosition = endPos;
        transform.localScale    = Vector3.one;

        // --- Cambiar a dorado si se ganó ---
        earned = isEarned;
        if (isEarned && meshRend != null)
        {
            meshRend.material = matEarned;
            AudioManager.Instance?.Play("starEarned");
            StartCoroutine(PulseGlow());
        }
    }

    // ── Rotación suave continua ─────────────────────────────────────────────────
    private void Update()
    {
        if (earned)
            transform.Rotate(Vector3.up, 60f * Time.deltaTime, Space.Self);
        else
            transform.Rotate(Vector3.up, 15f * Time.deltaTime, Space.Self);
    }

    // ── Pulso de brillo al ganar ────────────────────────────────────────────────
    private IEnumerator PulseGlow()
    {
        float t = 0f, dur = 0.5f;
        while (t < dur)
        {
            t += Time.deltaTime;
            float intensity = Mathf.Sin(t / dur * Mathf.PI);
            matEarned.SetColor("_EmissionColor", ColorGoldEmi * (1.5f + intensity * 2f));
            yield return null;
        }
        matEarned.SetColor("_EmissionColor", ColorGoldEmi * 1.5f);
    }

    // ── Easing ──────────────────────────────────────────────────────────────────
    private static float EaseOutBounce(float x)
    {
        float n1 = 7.5625f, d1 = 2.75f;
        if (x < 1f / d1)   return n1 * x * x;
        if (x < 2f / d1)   return n1 * (x -= 1.5f / d1) * x + 0.75f;
        if (x < 2.5f / d1) return n1 * (x -= 2.25f / d1) * x + 0.9375f;
        return n1 * (x -= 2.625f / d1) * x + 0.984375f;
    }

    private void OnDestroy()
    {
        if (matEarned != null) Destroy(matEarned);
        if (matEmpty  != null) Destroy(matEmpty);
    }
}

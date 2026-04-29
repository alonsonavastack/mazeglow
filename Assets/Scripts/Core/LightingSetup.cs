using UnityEngine;

/// <summary>
/// LightingSetup — Configura el ambiente 3D de MazeGlow automáticamente al iniciar.
/// Agrega este script al GameObject "_Managers" en la escena Game.
///
/// Efectos que crea:
///   • Niebla atmosférica suave (azul noche)
///   • Skybox sólido oscuro
///   • Luz direccional con sombras suaves
///   • Sombras en el Player
/// </summary>
public class LightingSetup : MonoBehaviour
{
    [Header("Niebla")]
    public bool  enableFog    = true;
    public Color fogColor     = new Color(0.05f, 0.04f, 0.12f);
    [Range(0.01f, 0.15f)]
    public float fogDensity   = 0.04f;

    [Header("Skybox / Fondo")]
    public Color skyColor     = new Color(0.04f, 0.03f, 0.10f);

    [Header("Luz direccional")]
    public Color lightColor   = new Color(0.65f, 0.60f, 0.90f);
    [Range(0.3f, 2f)]
    public float lightIntensity = 0.85f;
    public bool  castShadows  = true;

    private void Awake()
    {
        ApplySettings();
    }

    private void ApplySettings()
    {
        // ── Niebla ────────────────────────────────────────────────────────────────
        RenderSettings.fog          = enableFog;
        RenderSettings.fogMode      = FogMode.ExponentialSquared;
        RenderSettings.fogColor     = fogColor;
        RenderSettings.fogDensity   = fogDensity;

        // ── Skybox sólido ─────────────────────────────────────────────────────────
        RenderSettings.skybox       = null;
        Camera.main.backgroundColor = skyColor;
        Camera.main.clearFlags      = CameraClearFlags.SolidColor;

        // ── Luz ambiental ─────────────────────────────────────────────────────────
        RenderSettings.ambientMode  = UnityEngine.Rendering.AmbientMode.Flat;
        RenderSettings.ambientLight = new Color(0.08f, 0.07f, 0.14f);

        // ── Luz direccional ───────────────────────────────────────────────────────
        var dirLight = FindAnyObjectByType<Light>();
        if (dirLight != null && dirLight.type == LightType.Directional)
        {
            dirLight.color     = lightColor;
            dirLight.intensity = lightIntensity;
            if (castShadows)
            {
                dirLight.shadows          = LightShadows.Soft;
                dirLight.shadowStrength   = 0.6f;
                dirLight.shadowBias       = 0.02f;
                dirLight.shadowNormalBias = 0.02f;
            }
        }
        else
        {
            // Si no existe, crear una
            var go = new GameObject("DirectionalLight");
            var lt = go.AddComponent<Light>();
            lt.type      = LightType.Directional;
            lt.color     = lightColor;
            lt.intensity = lightIntensity;
            go.transform.rotation = Quaternion.Euler(45f, -30f, 0f);
            if (castShadows)
            {
                lt.shadows        = LightShadows.Soft;
                lt.shadowStrength = 0.6f;
            }
        }

        // ── Calidad de sombras en tiempo de ejecución ─────────────────────────────
        QualitySettings.shadows          = ShadowQuality.All;
        QualitySettings.shadowResolution = ShadowResolution.Medium;
        QualitySettings.shadowDistance   = 30f;
    }
}

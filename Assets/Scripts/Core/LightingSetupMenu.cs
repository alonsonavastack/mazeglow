using UnityEngine;

/// <summary>
/// LightingSetupMenu — Configura iluminación profesional para el menú principal 3D.
/// Agrega niebla colorida, skybox vibrante y luces para un look de juego real.
/// </summary>
public class LightingSetupMenu : MonoBehaviour
{
    [Header("Niebla Atmosférica")]
    public bool  enableFog    = true;
    public Color fogColor     = new Color(0.10f, 0.15f, 0.30f); // Azul vibrante
    [Range(0.01f, 0.10f)]
    public float fogDensity   = 0.02f;

    [Header("Fondo del Mundo")]
    public Color skyColor     = new Color(0.08f, 0.12f, 0.25f); // Azul profundo

    [Header("Iluminación Principal")]
    public Color lightColor   = new Color(0.80f, 0.85f, 1.00f); // Luz azul clara
    [Range(0.5f, 2f)]
    public float lightIntensity = 1.2f;
    public bool  castShadows  = false; // Sin sombras en menú para performance

    private void Awake()
    {
        ApplyMenuLighting();
    }

    private void ApplyMenuLighting()
    {
        // Niebla suave y colorida
        RenderSettings.fog          = enableFog;
        RenderSettings.fogMode      = FogMode.ExponentialSquared;
        RenderSettings.fogColor     = fogColor;
        RenderSettings.fogDensity   = fogDensity;

        // Skybox sólido vibrante
        RenderSettings.skybox       = null;
        if (Camera.main != null)
        {
            Camera.main.backgroundColor = skyColor;
            Camera.main.clearFlags      = CameraClearFlags.SolidColor;
        }

        // Luz ambiental para resaltar colores
        RenderSettings.ambientMode  = UnityEngine.Rendering.AmbientMode.Flat;
        RenderSettings.ambientLight = new Color(0.15f, 0.20f, 0.35f);

        // Luz direccional suave
        Light directionalLight = FindAnyObjectByType<Light>();
        if (directionalLight == null)
        {
            GameObject lightObj = new GameObject("Directional Light");
            directionalLight = lightObj.AddComponent<Light>();
            directionalLight.type = LightType.Directional;
            directionalLight.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
        }

        directionalLight.color       = lightColor;
        directionalLight.intensity   = lightIntensity;
        directionalLight.shadows     = castShadows ? LightShadows.Soft : LightShadows.None;
    }
}
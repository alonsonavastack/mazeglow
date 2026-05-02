using UnityEngine;

/// <summary>
/// ScreenDebugInfo — Muestra en pantalla las dimensiones reales del dispositivo.
/// Ponlo en cualquier objeto de la escena. Solo visible en el Editor y Development Build.
/// Quítalo antes de publicar en Google Play.
/// </summary>
public class ScreenDebugInfo : MonoBehaviour
{
    private GUIStyle style;

    private void Start()
    {
        style = new GUIStyle();
        style.fontSize  = 28;
        style.fontStyle = FontStyle.Bold;
        style.normal.textColor = Color.yellow;
    }

    private void OnGUI()
    {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        int w = Screen.width;
        int h = Screen.height;

        string orientacion = w > h ? "HORIZONTAL (Landscape)" : "VERTICAL (Portrait)";
        float ratio = (float)w / h;

        string info =
            $"Pantalla: {w} x {h} px\n" +
            $"Orientacion: {orientacion}\n" +
            $"Ratio: {ratio:F2}\n" +
            $"DPI: {Screen.dpi}\n" +
            $"Resolucion actual: {Screen.currentResolution.width}x{Screen.currentResolution.height}";

        // Fondo negro semitransparente
        GUI.color = new Color(0, 0, 0, 0.6f);
        GUI.DrawTexture(new Rect(0, 100, 520, 160), Texture2D.whiteTexture);

        // Texto
        GUI.color = Color.white;
        GUI.Label(new Rect(10, 108, 510, 155), info, style);
#endif
    }
}

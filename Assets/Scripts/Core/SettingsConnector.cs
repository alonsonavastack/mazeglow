using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// SettingsConnector — Todos los controles son Button.
/// El estado ON/OFF se lee y escribe directo en GameManager.Instance.
/// </summary>
public class SettingsConnector : MonoBehaviour
{
    [Header("Botones de navegación")]
    public Button botonRegresar;
    public Button botonIdioma;
    public Button botonEliminarAnuncios;
    public Button botonRestaurar;
    public Button botonCalificanos;
    public Button botonEscribenos;
    public Button botonPrivacidad;
    public Button botonCuenta;

    [Header("Botones tipo toggle (Button con texto ON/OFF)")]
    public Button botonVibraciones;
    public Button botonSonido;
    public Button botonModoOscuro;

    [Header("Textos opcionales para mostrar estado")]
    public TextMeshProUGUI textoIdioma;
    public TextMeshProUGUI textoVibraciones;
    public TextMeshProUGUI textoSonido;
    public TextMeshProUGUI textoModoOscuro;

    private void Start()
    {
        ActualizarTextos();
        ConectarBotones();
    }

    // ── Refleja el estado actual de GameManager en los textos ─────────────────
    private void ActualizarTextos()
    {
        var gm = GameManager.Instance;
        if (gm == null) return;

        if (textoIdioma       != null) textoIdioma.text       = gm.language == "es" ? "Idioma: Español" : "Idioma: English";
        if (textoVibraciones  != null) textoVibraciones.text  = "Vibraciones: " + (gm.vibrationsEnabled ? "ON" : "OFF");
        if (textoSonido       != null) textoSonido.text       = "Sonido: "      + (gm.soundEnabled      ? "ON" : "OFF");
        if (textoModoOscuro   != null) textoModoOscuro.text   = "Modo oscuro: " + (gm.darkModeEnabled   ? "ON" : "OFF");
    }

    // ── Conecta todos los botones ─────────────────────────────────────────────
    private void ConectarBotones()
    {
        // Regresar al menú
        botonRegresar?.onClick.AddListener(() =>
            SceneLoader.Instance?.GoToMenu()
        );

        // Idioma — alterna ES ↔ EN
        botonIdioma?.onClick.AddListener(() =>
        {
            var gm = GameManager.Instance;
            if (gm == null) return;
            gm.language = gm.language == "es" ? "en" : "es";
            LocalizationManager.Instance?.SetLanguage(gm.language);
            SaveManager.Instance?.SaveAll();
            ActualizarTextos();
        });

        // Vibraciones — alterna ON/OFF
        botonVibraciones?.onClick.AddListener(() =>
        {
            var gm = GameManager.Instance;
            if (gm == null) return;
            gm.vibrationsEnabled = !gm.vibrationsEnabled;
            SaveManager.Instance?.SaveAll();
            ActualizarTextos();
        });

        // Sonido — alterna ON/OFF
        botonSonido?.onClick.AddListener(() =>
        {
            var gm = GameManager.Instance;
            if (gm == null) return;
            gm.soundEnabled = !gm.soundEnabled;
            AudioManager.Instance?.SetMute(!gm.soundEnabled);
            SaveManager.Instance?.SaveAll();
            ActualizarTextos();
        });

        // Modo oscuro — alterna ON/OFF
        botonModoOscuro?.onClick.AddListener(() =>
        {
            var gm = GameManager.Instance;
            if (gm == null) return;
            gm.darkModeEnabled = !gm.darkModeEnabled;
            ThemeManager.Instance?.ApplyTheme(gm.darkModeEnabled);
            SaveManager.Instance?.SaveAll();
            ActualizarTextos();
        });

        // Eliminar anuncios
        botonEliminarAnuncios?.onClick.AddListener(() =>
            Debug.Log("[Settings] Abrir modal eliminar anuncios — pendiente IAP")
        );

        // Restaurar compras
        botonRestaurar?.onClick.AddListener(() =>
            Debug.Log("[Settings] Restaurar compras — pendiente IAP")
        );

        // Califícanos
        botonCalificanos?.onClick.AddListener(() =>
            Application.OpenURL("market://details?id=com.tunombre.mazeglow")
        );

        // Escríbenos
        botonEscribenos?.onClick.AddListener(() =>
        {
            string sub  = System.Uri.EscapeDataString("Soporte MazeGlow");
            string body = System.Uri.EscapeDataString("Hola, tengo una pregunta...");
            Application.OpenURL($"mailto:alonso.nava086@gmail.com?subject={sub}&body={body}");
        });

        // Política de privacidad
        botonPrivacidad?.onClick.AddListener(() =>
            Application.OpenURL("https://tunombre.github.io/mazeglow-privacy")
        );

        // Cuenta (Login de Google)
        botonCuenta?.onClick.AddListener(() =>
        {
            if (LoginScreen.Instance != null)
                LoginScreen.Instance.Show(loginMode: true);
        });
    }
}

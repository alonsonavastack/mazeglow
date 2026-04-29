using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// ResultsConnector — Conecta los botones de la escena Results.
/// Agregar a un GameObject vacío llamado "ResultsConnector" en la escena Results.
/// </summary>
public class ResultsConnector : MonoBehaviour
{
    [Header("Botones — Escena Results")]
    public Button botonSiguiente;

    [Header("Textos opcionales")]
    public TextMeshProUGUI textNivelCompleto;
    public TextMeshProUGUI textMonedasGanadas;

    private void Start()
    {
        // Actualizar textos con datos del nivel completado
        if (textNivelCompleto != null)
            textNivelCompleto.text = "¡NIVEL COMPLETADO!";

        if (textMonedasGanadas != null && GameManager.Instance != null)
            textMonedasGanadas.text = $"+{10 + GameManager.Instance.currentLevel} monedas";

        // Conectar botón Siguiente
        if (botonSiguiente != null)
        {
            botonSiguiente.onClick.RemoveAllListeners();
            botonSiguiente.onClick.AddListener(() =>
            {
                SceneLoader.Instance?.GoToGame();
            });
        }
    }
}

using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// ButtonConnector — Conecta todos los botones de la escena Game
/// con sus managers automáticamente en Start().
/// Agregar este script al GameObject GameController en la escena Game.
/// </summary>
public class ButtonConnector : MonoBehaviour
{
    [Header("Botones — Escena Game (PanelGameOver)")]
    public Button botonReintentar;
    public Button botonVerAnuncio;

    private void Start()
    {
        ConectarBotonesGame();
    }

    private void ConectarBotonesGame()
    {
        if (botonReintentar != null)
        {
            botonReintentar.onClick.RemoveAllListeners();
            botonReintentar.onClick.AddListener(() =>
            {
                // Ocultar panel y reiniciar nivel
                var panel = botonReintentar.transform.parent?.gameObject;
                if (panel != null) panel.SetActive(false);
                GameController.Instance?.RetryLevel();
            });
        }

        if (botonVerAnuncio != null)
        {
            botonVerAnuncio.onClick.RemoveAllListeners();
            botonVerAnuncio.onClick.AddListener(() =>
            {
                AdManager.Instance?.ShowRewarded();
                // Dar una vida extra y reintentar al ver el anuncio
                GameManager.Instance?.AddLives(1);
                var panel = botonVerAnuncio.transform.parent?.gameObject;
                if (panel != null) panel.SetActive(false);
                GameController.Instance?.RetryLevel();
            });
        }
    }
}

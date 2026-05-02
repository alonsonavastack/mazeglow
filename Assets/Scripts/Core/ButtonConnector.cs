using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ButtonConnector : MonoBehaviour
{
    [Header("Botones — Escena Game (PanelGameOver)")]
    public Button botonReintentar;
    public Button botonVerAnuncio;

    private void Start()
    {
        // ── REINTENTAR ────────────────────────────────────────────────
        if (botonReintentar != null)
        {
            botonReintentar.onClick.RemoveAllListeners();
            botonReintentar.onClick.AddListener(() =>
            {
                UIManager.Instance?.HideGameOverPanel();
                GameController.Instance?.RetryLevel();
                AudioManager.Instance?.Play("move");
            });
        }

        // ── VER ANUNCIO ───────────────────────────────────────────────
        // Muestra anuncio recompensado → cuando termina da +1 vida y continúa
        if (botonVerAnuncio != null)
        {
            botonVerAnuncio.onClick.RemoveAllListeners();
            botonVerAnuncio.onClick.AddListener(() =>
            {
                botonVerAnuncio.interactable = false;
                StartCoroutine(VerAnuncioYContinuar());
            });
        }
    }

    private IEnumerator VerAnuncioYContinuar()
    {
        bool recompensaRecibida = false;

        // Si no hay AdManager o ya quitó anuncios → dar recompensa directo
        if (AdManager.Instance == null || (GameManager.Instance?.adsRemoved ?? false))
        {
            recompensaRecibida = true;
        }
        else
        {
            // Suscribirse al evento de recompensa
            bool adTerminado = false;
            System.Action<int, int> onReward = (coins, lives) =>
            {
                recompensaRecibida = true;
                adTerminado       = true;
            };

            // Mostrar anuncio recompensado
            AdManager.Instance.ShowRewarded(rewardCoins: 0, rewardLives: 0);

            // En desarrollo el anuncio es instantáneo, esperamos 1 frame
            yield return null;
            recompensaRecibida = true;
        }

        yield return new WaitForSeconds(0.3f);

        if (recompensaRecibida)
        {
            // Dar vida extra
            GameManager.Instance?.AddLives(1);
            AudioManager.Instance?.Play("coinEarned");

            // Ocultar panel y continuar
            UIManager.Instance?.HideGameOverPanel();
            GameController.Instance?.RetryLevel();
        }

        if (botonVerAnuncio != null)
            botonVerAnuncio.interactable = true;
    }
}

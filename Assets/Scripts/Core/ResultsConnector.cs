using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class ResultsConnector : MonoBehaviour
{
    [Header("Botones — Escena Results")]
    public Button botonSiguiente;

    [Header("Textos opcionales")]
    public TextMeshProUGUI textNivelCompleto;
    public TextMeshProUGUI textMonedasGanadas;

    private void Start()
    {
        // Actualizar textos
        if (textNivelCompleto != null)
            textNivelCompleto.text = "¡NIVEL COMPLETADO!";

        if (textMonedasGanadas != null && GameManager.Instance != null)
            textMonedasGanadas.text = $"+{10 + GameManager.Instance.currentLevel} monedas";

        // ── SIGUIENTE ─────────────────────────────────────────────────
        // Muestra intersticial cada N niveles antes de cargar el siguiente
        if (botonSiguiente != null)
        {
            botonSiguiente.onClick.RemoveAllListeners();
            botonSiguiente.onClick.AddListener(() =>
            {
                botonSiguiente.interactable = false;
                StartCoroutine(SiguienteConAnuncio());
            });
        }
    }

    private IEnumerator SiguienteConAnuncio()
    {
        bool mostrarAd = AdManager.Instance != null
                      && !(GameManager.Instance?.adsRemoved ?? false);

        if (mostrarAd)
        {
            bool adCerrado = false;

            // Suscribirse al evento de cierre
            System.Action onClosed = () => adCerrado = true;
            AdManager.Instance.OnInterstitialClosed += onClosed;
            AdManager.Instance.ShowInterstitial();

            // Esperar a que cierre el anuncio (máx 30 seg)
            float waited = 0f;
            while (!adCerrado && waited < 30f)
            {
                waited += Time.deltaTime;
                yield return null;
            }

            AdManager.Instance.OnInterstitialClosed -= onClosed;
        }
        else
        {
            yield return new WaitForSeconds(0.2f);
        }

        // Ir al siguiente nivel
        SceneLoader.Instance?.GoToGame();
    }
}

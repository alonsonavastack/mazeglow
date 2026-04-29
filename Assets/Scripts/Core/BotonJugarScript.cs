using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Ponlo directamente en BotonJugar. No depende de ningún otro script.
/// </summary>
[RequireComponent(typeof(Button))]
public class BotonJugarScript : MonoBehaviour
{
    private void Start()
    {
        Button btn = GetComponent<Button>();
        btn.onClick.RemoveAllListeners();
        btn.onClick.AddListener(Jugar);
        Debug.Log("[BotonJugar] Listo para jugar");
    }

    public void Jugar()
    {
        Debug.Log("[BotonJugar] Click recibido! Cargando escena 1...");
        SceneManager.LoadScene(1);
    }
}

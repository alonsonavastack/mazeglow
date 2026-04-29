using UnityEngine;

/// <summary>
/// BootstrapGame — Se ejecuta PRIMERO en la escena Game.
/// Si no hay GameManager (porque se abrió la escena directamente sin pasar por MainMenu),
/// crea cada manager como GameObject raíz independiente para que DontDestroyOnLoad funcione.
/// </summary>
public class BootstrapGame : MonoBehaviour
{
    private void Awake()
    {
        // Crear managers faltantes — cada uno como root GameObject independiente
        // para que DontDestroyOnLoad no lance warnings

        if (GameManager.Instance == null)
        {
            Debug.LogWarning("[Bootstrap] Creando GameManager temporal.");
            new GameObject("_GameManager_Auto").AddComponent<GameManager>();
        }

        if (SaveManager.Instance == null)
        {
            new GameObject("_SaveManager_Auto").AddComponent<SaveManager>();
        }

        if (AchievementManager.Instance == null)
        {
            new GameObject("_AchievementManager_Auto").AddComponent<AchievementManager>();
        }

        if (AudioManager.Instance == null)
        {
            Debug.LogWarning("[Bootstrap] Creando AudioManager temporal.");
            new GameObject("_AudioManager_Auto").AddComponent<AudioManager>();
        }

        Debug.Log("[Bootstrap] Managers verificados. Nivel: " +
                  (GameManager.Instance != null ? GameManager.Instance.currentLevel.ToString() : "?"));
    }
}

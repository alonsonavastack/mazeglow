using UnityEngine;

/// <summary>
/// BootstrapGame v2 — Crea todos los managers necesarios si no existen.
/// Incluye LevelCompleteScreen y GameOverScreen PRO.
/// </summary>
public class BootstrapGame : MonoBehaviour
{
    private void Awake()
    {
        // GameManager
        if (GameManager.Instance == null)
        {
            Debug.LogWarning("[Bootstrap] Creando managers automáticamente.");
            var go = new GameObject("_Managers_Auto");
            go.AddComponent<GameManager>();
            go.AddComponent<SaveManager>();
            go.AddComponent<AudioManager>();
            go.AddComponent<AchievementManager>();
        }

        // LevelCompleteScreen
        if (LevelCompleteScreen.Instance == null)
        {
            var go = new GameObject("LevelCompleteScreen");
            go.AddComponent<LevelCompleteScreen>();
            DontDestroyOnLoad(go);
            Debug.Log("[Bootstrap] LevelCompleteScreen creado.");
        }

        // GameOverScreen
        if (GameOverScreen.Instance == null)
        {
            var go = new GameObject("GameOverScreen");
            go.AddComponent<GameOverScreen>();
            DontDestroyOnLoad(go);
            Debug.Log("[Bootstrap] GameOverScreen creado.");
        }

        // UIManager
        if (UIManager.Instance == null)
        {
            var go = new GameObject("UIManager");
            go.AddComponent<UIManager>();
            DontDestroyOnLoad(go);
        }
    }
}

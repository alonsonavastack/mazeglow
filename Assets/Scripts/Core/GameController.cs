using UnityEngine;
using System.Collections;

public class GameController : MonoBehaviour
{
    public static GameController Instance { get; private set; }

    [Header("Referencias")]
    public MazeGenerator    mazeGenerator;
    public PlayerController player;
    public CelebrationFX    celebrationFX;

    [Header("Anuncios")]
    public int adEveryNLevels = 3;

    private int  levelsSinceAd  = 0;
    private bool levelCompleted = false;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        if (mazeGenerator == null) mazeGenerator = FindAnyObjectByType<MazeGenerator>();
        if (player        == null) player        = FindAnyObjectByType<PlayerController>();
        if (celebrationFX == null) celebrationFX = FindAnyObjectByType<CelebrationFX>();
    }

    private void Start()
    {
        if (GameManager.Instance == null) { Debug.LogError("[GameController] No hay GameManager."); return; }

        // Auto-crear MazeGenerator si falta en la escena
        if (mazeGenerator == null)
        {
            Debug.LogWarning("[GameController] MazeGenerator no encontrado. Creando uno automáticamente.");
            var mazeGO = new GameObject("MazeGenerator_Auto");
            mazeGenerator = mazeGO.AddComponent<MazeGenerator>();
        }

        StartLevel(GameManager.Instance.currentLevel);
    }

    // ── Iniciar nivel ─────────────────────────────────────────────────────────────
    public void StartLevel(int level)
    {
        levelCompleted = false;

        // FIX: Resetear 5 vidas al inicio de cada nivel nuevo (no al reintentar)
        GameManager.Instance?.ResetLivesForNewLevel();

        mazeGenerator.GenerateMaze(level);
        UIManager.Instance?.ShowGameHUD(level);
        AudioManager.Instance?.Play("levelStart");
        Debug.Log("[GameController] Nivel " + level + " iniciado.");
    }

    // ── Nivel completado ──────────────────────────────────────────────────────────
    public void OnLevelComplete(float time = 0f, int collisions = 0)
    {
        if (levelCompleted) return;
        levelCompleted = true;

        Debug.Log("[GameController] Nivel completado.");
        AudioManager.Instance?.Play("levelComplete");
        celebrationFX?.Play();

        if (GameManager.Instance != null)
        {
            GameManager.Instance.AddCoins(10 + GameManager.Instance.currentLevel);
            GameManager.Instance.CompleteLevel();
            // FIX: NO tocar las vidas aquí — StartLevel lo hace al iniciar el siguiente
        }

        AchievementManager.Instance?.TrackEvent(AchievementEvent.LevelCompleted);
        AchievementManager.Instance?.TrackEvent(AchievementEvent.StarEarned, 3);
        levelsSinceAd++;
        StartCoroutine(NextLevelRoutine());
    }

    // ── Game Over ─────────────────────────────────────────────────────────────────
    public void OnGameOver()
    {
        AudioManager.Instance?.Play("gameOver");
        UIManager.Instance?.ShowGameOverPanel();
    }

    // ── Reintentar ───────────────────────────────────────────────────────────────
    public void RetryLevel()
    {
        // Reiniciar nivel y resetear vidas a 2
        GameManager.Instance?.ResetLivesForNewLevel();
        
        int level = GameManager.Instance?.currentLevel ?? 1;
        levelCompleted = false;
        mazeGenerator.GenerateMaze(level);
        UIManager.Instance?.ShowGameHUD(level);
        Debug.Log("[GameController] Reintentando nivel " + level);
    }

    // ── Siguiente nivel ───────────────────────────────────────────────────────────
    private IEnumerator NextLevelRoutine()
    {
        yield return new WaitForSeconds(2f);

        // Anuncio intersticial simple sin esperar callback
        if (AdManager.Instance != null
         && GameManager.Instance != null
         && !GameManager.Instance.adsRemoved
         && levelsSinceAd >= adEveryNLevels)
        {
            levelsSinceAd = 0;
            AdManager.Instance.ShowInterstitial();
            yield return new WaitForSeconds(0.5f);
        }

        if (GameManager.Instance != null)
            StartLevel(GameManager.Instance.currentLevel);
    }
}

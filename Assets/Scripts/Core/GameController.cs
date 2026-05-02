using UnityEngine;
using System.Collections;

/// <summary>
/// GameController v5 — Conecta LevelCompleteScreen y GameOverScreen PRO.
/// Calcula estrellas según choques: 0=3★, 1=2★, 2+=1★
/// </summary>
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
    private float levelStartTime = 0f;

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
        if (GameManager.Instance == null)
        {
            Debug.LogError("[GameController] No hay GameManager.");
            return;
        }
        if (mazeGenerator == null)
        {
            var go = new GameObject("MazeGenerator_Auto");
            mazeGenerator = go.AddComponent<MazeGenerator>();
        }
        StartLevel(GameManager.Instance.currentLevel);
    }

    // ── Iniciar nivel ─────────────────────────────────────────────────────────────
    public void StartLevel(int level)
    {
        levelCompleted = false;
        levelStartTime = Time.time;

        // Ocultar pantallas si están visibles
        LevelCompleteScreen.Instance?.HideImmediate();
        GameOverScreen.Instance?.HideImmediate();

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

        float elapsed = Time.time - levelStartTime;

        // Calcular estrellas: 0 choques = 3★, 1 choque = 2★, 2+ choques = 1★
        int stars = collisions == 0 ? 3 : collisions == 1 ? 2 : 1;

        // Monedas según estrellas
        int coinsEarned = 10 + GameManager.Instance.currentLevel + (stars * 5);

        Debug.Log($"[GameController] Nivel completado. Choques={collisions} Estrellas={stars} Monedas={coinsEarned}");

        AudioManager.Instance?.Play("levelComplete");

        if (GameManager.Instance != null)
        {
            GameManager.Instance.AddCoins(coinsEarned);
            GameManager.Instance.CompleteLevel();
        }

        AchievementManager.Instance?.TrackEvent(AchievementEvent.LevelCompleted);
        AchievementManager.Instance?.TrackEvent(AchievementEvent.StarEarned, stars);

        levelsSinceAd++;

        // Mostrar pantalla PRO de nivel completado
        StartCoroutine(ShowLevelComplete(stars, coinsEarned));
    }

    private IEnumerator ShowLevelComplete(int stars, int coins)
    {
        // Pequeña pausa dramática
        yield return new WaitForSeconds(0.4f);

        if (LevelCompleteScreen.Instance != null)
        {
            LevelCompleteScreen.Instance.Show(
                GameManager.Instance?.currentLevel - 1 ?? 1,
                stars,
                coins);
        }
        else
        {
            // Fallback si no existe la pantalla
            yield return new WaitForSeconds(2f);
            StartNextLevel();
        }
    }

    // ── Game Over ─────────────────────────────────────────────────────────────────
    public void OnGameOver()
    {
        AudioManager.Instance?.Play("gameOver");

        if (GameOverScreen.Instance != null)
            GameOverScreen.Instance.Show();
        else
            UIManager.Instance?.ShowGameOverPanel(); // fallback
    }

    // ── Reintentar ────────────────────────────────────────────────────────────────
    public void RetryLevel()
    {
        GameManager.Instance?.ResetLivesForNewLevel();
        int level = GameManager.Instance?.currentLevel ?? 1;
        levelCompleted = false;
        levelStartTime = Time.time;

        LevelCompleteScreen.Instance?.HideImmediate();
        GameOverScreen.Instance?.HideImmediate();

        mazeGenerator.GenerateMaze(level);
        UIManager.Instance?.ShowGameHUD(level);
        Debug.Log("[GameController] Reintentando nivel " + level);
    }

    // ── Siguiente nivel (llamado desde LevelCompleteScreen) ───────────────────────
    public void StartNextLevel()
    {
        StartCoroutine(NextLevelRoutine());
    }

    private IEnumerator NextLevelRoutine()
    {
        if (AdManager.Instance != null
         && GameManager.Instance != null
         && !GameManager.Instance.adsRemoved
         && levelsSinceAd >= adEveryNLevels)
        {
            levelsSinceAd = 0;
            AdManager.Instance.ShowInterstitial();
            yield return new WaitForSeconds(0.5f);
        }
        yield return null;
        if (GameManager.Instance != null)
            StartLevel(GameManager.Instance.currentLevel);
    }
}

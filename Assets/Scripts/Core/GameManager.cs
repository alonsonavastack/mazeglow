using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Estado del jugador")]
    public int currentLevel = 1;
    public int coins        = 0;
    public int lives        = 2;   // 2 vidas por nivel
    public int hints        = 3;
    
    [Header("Rachas (Streaks)")]
    public int currentStreak = 0;
    public int longestStreak = 0;
    public string lastLoginDate = "";
    public string lastStreakDate = "";

    [Header("Configuración")]
    public bool soundEnabled      = true;
    public bool vibrationsEnabled = true;
    public bool darkModeEnabled   = true;
    public string language        = "es";
    public bool adsRemoved        = false;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        SaveManager.Instance?.LoadAll();
    }

    public void AddCoins(int amount)
    {
        coins += amount;
        SaveManager.Instance?.SaveAll();
        UIManager.Instance?.RefreshHUD();
    }

    public bool SpendCoins(int amount)
    {
        if (coins < amount) return false;
        coins -= amount;
        SaveManager.Instance?.SaveAll();
        UIManager.Instance?.RefreshHUD();
        return true;
    }

    public void AddLives(int amount)
    {
        lives = Mathf.Min(lives + amount, 2); // máximo 2
        SaveManager.Instance?.SaveAll();
        UIManager.Instance?.RefreshHUD();
    }

    public bool SpendLife()
    {
        if (lives <= 0) return false;
        lives--;
        SaveManager.Instance?.SaveAll();
        UIManager.Instance?.RefreshHUD();
        return true;
    }

    public void AddHints(int amount)
    {
        hints += amount;
        SaveManager.Instance?.SaveAll();
        UIManager.Instance?.RefreshHUD();
    }

    public bool UseHint()
    {
        if (hints <= 0) return false;
        hints--;
        AchievementManager.Instance?.TrackEvent(AchievementEvent.HintUsed);
        SaveManager.Instance?.SaveAll();
        UIManager.Instance?.RefreshHUD();
        return true;
    }

    public void CompleteLevel()
    {
        currentLevel++;
        AchievementManager.Instance?.TrackEvent(AchievementEvent.LevelCompleted);
        StreakManager.Instance?.ExtendStreak();
        SaveManager.Instance?.SaveAll();
    }

    // Resetear vidas al iniciar nivel nuevo
    public void ResetLivesForNewLevel()
    {
        lives = 2;   // 2 vidas por nivel
        UIManager.Instance?.RefreshHUD();
    }
}

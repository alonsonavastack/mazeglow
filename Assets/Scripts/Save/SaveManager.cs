using UnityEngine;
using System.IO;
using System;

/// <summary>
/// SaveManager — Guarda y carga TODO el progreso del jugador.
/// Usa JSON para datos complejos y PlayerPrefs para datos simples y configuración.
/// Funciona 100% sin internet.
/// </summary>
public class SaveManager : MonoBehaviour
{
    public static SaveManager Instance { get; private set; }

    // Ruta del archivo de guardado
    private string SavePath => Path.Combine(Application.persistentDataPath, "mazeglow_save.json");

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    // ── Guardar todo ────────────────────────────────────────────────────────────
    public void SaveAll()
    {
        var gm = GameManager.Instance;
        if (gm == null) return;

        // Datos de progreso en JSON
        var data = new SaveData
        {
            currentLevel       = gm.currentLevel,
            coins              = gm.coins,
            lives              = gm.lives <= 0 ? 2 : gm.lives,
            hints              = gm.hints,
            adsRemoved         = gm.adsRemoved,
            lastLoginDate      = gm.lastLoginDate,
            lastStreakDate     = gm.lastStreakDate,
            currentStreak      = gm.currentStreak,
            longestStreak      = gm.longestStreak,
            achievements       = AchievementManager.Instance?.GetSaveData()
        };

        File.WriteAllText(SavePath, JsonUtility.ToJson(data, true));

        // Configuración en PlayerPrefs (más rápido para booleanos simples)
        PlayerPrefs.SetInt("SoundEnabled",      gm.soundEnabled      ? 1 : 0);
        PlayerPrefs.SetInt("VibrationsEnabled", gm.vibrationsEnabled ? 1 : 0);
        PlayerPrefs.SetInt("DarkMode",          gm.darkModeEnabled   ? 1 : 0);
        PlayerPrefs.SetString("Language",       gm.language);
        PlayerPrefs.Save();
    }

    // ── Cargar todo ─────────────────────────────────────────────────────────────
    public void LoadAll()
    {
        var gm = GameManager.Instance;
        if (gm == null) return;

        // Cargar configuración desde PlayerPrefs
        gm.soundEnabled      = PlayerPrefs.GetInt("SoundEnabled", 1) == 1;
        gm.vibrationsEnabled = PlayerPrefs.GetInt("VibrationsEnabled", 1) == 1;
        gm.darkModeEnabled   = PlayerPrefs.GetInt("DarkMode", 1) == 1;
        gm.language          = PlayerPrefs.GetString("Language", "es");

        // Cargar progreso desde JSON
        if (!File.Exists(SavePath)) return;

        try
        {
            var data = JsonUtility.FromJson<SaveData>(File.ReadAllText(SavePath));
            gm.currentLevel = data.currentLevel;
            gm.coins        = data.coins;
            gm.lives = data.lives <= 0 ? 2 : data.lives;
            gm.hints        = data.hints;
            gm.adsRemoved   = data.adsRemoved;
            gm.lastLoginDate  = data.lastLoginDate;
            gm.lastStreakDate = data.lastStreakDate;
            gm.currentStreak  = data.currentStreak;
            gm.longestStreak  = data.longestStreak;
            AchievementManager.Instance?.LoadSaveData(data.achievements);
        }
        catch (Exception e)
        {
            Debug.LogError($"[SaveManager] Error al cargar: {e.Message}");
        }
    }

    // ── Borrar todo (para pruebas) ──────────────────────────────────────────────
    public void DeleteAll()
    {
        if (File.Exists(SavePath)) File.Delete(SavePath);
        PlayerPrefs.DeleteAll();
        Debug.Log("[SaveManager] Datos borrados.");
    }
}

// ── Estructura de datos serializables ──────────────────────────────────────────
[Serializable]
public class SaveData
{
    public int    currentLevel;
    public int    coins;
    public int    lives;
    public int    hints;
    public bool   adsRemoved;
    public string lastLoginDate;
    public string lastStreakDate;
    public int    currentStreak;
    public int    longestStreak;
    public AchievementSaveData achievements;
}

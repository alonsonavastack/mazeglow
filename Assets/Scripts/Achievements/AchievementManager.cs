using UnityEngine;
using System.Collections.Generic;
using System;

/// <summary>
/// Tipos de eventos que pueden disparar el avance de un logro.
/// Agrega aquí nuevos eventos si necesitas más logros en el futuro.
/// </summary>
public enum AchievementEvent
{
    LevelCompleted,
    LevelCompletedNoBounce,
    HintUsed,
    AdWatched,
    DailyLogin,
    StarEarned
}

/// <summary>
/// AchievementManager — Sistema completo de logros.
/// Maneja logros diarios, semanales y permanentes.
/// </summary>
public class AchievementManager : MonoBehaviour
{
    public static AchievementManager Instance { get; private set; }

    // Lista de todos los logros definidos en el juego
    private List<Achievement> allAchievements = new List<Achievement>();

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        InitializeAchievements();
    }

    // ── Definición de todos los logros del juego ─────────────────────────────────
    private void InitializeAchievements()
    {
        allAchievements.Clear();

        // ── LOGROS DIARIOS ────────────────────────────────────────────────────────
        allAchievements.Add(new Achievement
        {
            id           = "daily_play_5",
            name         = "¡En racha!",
            description  = "Juega 5 niveles hoy",
            category     = AchievementCategory.Daily,
            targetEvent  = AchievementEvent.LevelCompleted,
            targetCount  = 5,
            rewardCoins  = 50,
            rewardLives  = 0
        });

        allAchievements.Add(new Achievement
        {
            id           = "daily_no_bounce_3",
            name         = "Precisión perfecta",
            description  = "Completa 3 niveles sin chocar",
            category     = AchievementCategory.Daily,
            targetEvent  = AchievementEvent.LevelCompletedNoBounce,
            targetCount  = 3,
            rewardCoins  = 30,
            rewardLives  = 1
        });

        allAchievements.Add(new Achievement
        {
            id           = "daily_use_hints_2",
            name         = "Explorador",
            description  = "Usa 2 pistas hoy",
            category     = AchievementCategory.Daily,
            targetEvent  = AchievementEvent.HintUsed,
            targetCount  = 2,
            rewardCoins  = 20,
            rewardLives  = 0
        });

        // ── LOGROS SEMANALES ──────────────────────────────────────────────────────
        allAchievements.Add(new Achievement
        {
            id           = "weekly_play_30",
            name         = "Maratonista",
            description  = "Juega 30 niveles esta semana",
            category     = AchievementCategory.Weekly,
            targetEvent  = AchievementEvent.LevelCompleted,
            targetCount  = 30,
            rewardCoins  = 200,
            rewardLives  = 0
        });

        allAchievements.Add(new Achievement
        {
            id           = "weekly_100_stars",
            name         = "Coleccionista de estrellas",
            description  = "Consigue 100 estrellas esta semana",
            category     = AchievementCategory.Weekly,
            targetEvent  = AchievementEvent.StarEarned,
            targetCount  = 100,
            rewardCoins  = 300,
            rewardLives  = 2
        });

        allAchievements.Add(new Achievement
        {
            id           = "weekly_login_7",
            name         = "Semana completa",
            description  = "Entra al juego todos los días de la semana",
            category     = AchievementCategory.Weekly,
            targetEvent  = AchievementEvent.DailyLogin,
            targetCount  = 7,
            rewardCoins  = 500,
            rewardLives  = 3
        });

        // ── LOGROS PERMANENTES ────────────────────────────────────────────────────
        allAchievements.Add(new Achievement
        {
            id           = "perm_100_levels",
            name         = "Centenario",
            description  = "Completa 100 niveles en total",
            category     = AchievementCategory.Permanent,
            targetEvent  = AchievementEvent.LevelCompleted,
            targetCount  = 100,
            rewardCoins  = 1000,
            rewardLives  = 5
        });

        allAchievements.Add(new Achievement
        {
            id           = "perm_50_ads",
            name         = "Gran apoyo",
            description  = "Ve 50 anuncios",
            category     = AchievementCategory.Permanent,
            targetEvent  = AchievementEvent.AdWatched,
            targetCount  = 50,
            rewardCoins  = 500,
            rewardLives  = 0
        });
    }

    // ── Registrar un evento de juego ─────────────────────────────────────────────
    public void TrackEvent(AchievementEvent evt, int amount = 1)
    {
        bool anyUnlocked = false;

        foreach (var ach in allAchievements)
        {
            if (ach.isCompleted) continue;
            if (ach.targetEvent != evt) continue;
            if (!IsInValidPeriod(ach)) continue;

            ach.currentCount += amount;

            if (ach.currentCount >= ach.targetCount)
            {
                UnlockAchievement(ach);
                anyUnlocked = true;
            }
        }

        if (anyUnlocked)
            SaveManager.Instance?.SaveAll();
    }

    // ── Desbloquear logro y entregar recompensa ──────────────────────────────────
    private void UnlockAchievement(Achievement ach)
    {
        ach.isCompleted    = true;
        ach.completionDate = DateTime.Now.ToString("yyyy-MM-dd");

        // Entregar recompensa automáticamente
        if (ach.rewardCoins > 0) GameManager.Instance?.AddCoins(ach.rewardCoins);
        if (ach.rewardLives > 0) GameManager.Instance?.AddLives(ach.rewardLives);

        // Mostrar animación de desbloqueo en UI
        UIManager.Instance?.ShowAchievementUnlocked(ach);

        Debug.Log($"[AchievementManager] Logro desbloqueado: {ach.name}");
    }

    // ── Validar si el logro está en su período activo ────────────────────────────
    private bool IsInValidPeriod(Achievement ach)
    {
        switch (ach.category)
        {
            case AchievementCategory.Daily:
                return ach.lastResetDate == DateTime.Now.ToString("yyyy-MM-dd");
            case AchievementCategory.Weekly:
                int week = System.Globalization.CultureInfo.CurrentCulture
                    .Calendar.GetWeekOfYear(DateTime.Now,
                        System.Globalization.CalendarWeekRule.FirstFourDayWeek,
                        DayOfWeek.Monday);
                return ach.lastResetWeek == week;
            case AchievementCategory.Permanent:
                return true;
            default:
                return false;
        }
    }

    // ── Reiniciar logros vencidos ─────────────────────────────────────────────────
    public void CheckAndResetPeriods()
    {
        string today = DateTime.Now.ToString("yyyy-MM-dd");
        int currentWeek = System.Globalization.CultureInfo.CurrentCulture
            .Calendar.GetWeekOfYear(DateTime.Now,
                System.Globalization.CalendarWeekRule.FirstFourDayWeek,
                DayOfWeek.Monday);

        foreach (var ach in allAchievements)
        {
            if (ach.category == AchievementCategory.Daily && ach.lastResetDate != today)
            {
                ach.currentCount  = 0;
                ach.isCompleted   = false;
                ach.lastResetDate = today;
            }
            else if (ach.category == AchievementCategory.Weekly && ach.lastResetWeek != currentWeek)
            {
                ach.currentCount   = 0;
                ach.isCompleted    = false;
                ach.lastResetWeek  = currentWeek;
            }
        }
    }

    // ── Serialización para SaveManager ──────────────────────────────────────────
    public AchievementSaveData GetSaveData()
    {
        var data = new AchievementSaveData();
        data.achievements = new List<AchievementEntry>();

        foreach (var ach in allAchievements)
        {
            data.achievements.Add(new AchievementEntry
            {
                id             = ach.id,
                currentCount   = ach.currentCount,
                isCompleted    = ach.isCompleted,
                lastResetDate  = ach.lastResetDate,
                lastResetWeek  = ach.lastResetWeek,
                completionDate = ach.completionDate
            });
        }
        return data;
    }

    public void LoadSaveData(AchievementSaveData data)
    {
        if (data?.achievements == null) return;

        foreach (var entry in data.achievements)
        {
            var ach = allAchievements.Find(a => a.id == entry.id);
            if (ach == null) continue;
            ach.currentCount   = entry.currentCount;
            ach.isCompleted    = entry.isCompleted;
            ach.lastResetDate  = entry.lastResetDate;
            ach.lastResetWeek  = entry.lastResetWeek;
            ach.completionDate = entry.completionDate;
        }

        CheckAndResetPeriods();
    }

    public List<Achievement> GetAllAchievements() => allAchievements;
}

// ── Clases de datos ─────────────────────────────────────────────────────────────
public enum AchievementCategory { Daily, Weekly, Permanent }

[Serializable]
public class Achievement
{
    public string             id;
    public string             name;
    public string             description;
    public AchievementCategory category;
    public AchievementEvent   targetEvent;
    public int                targetCount;
    public int                rewardCoins;
    public int                rewardLives;
    // Estado en tiempo de ejecución
    public int    currentCount;
    public bool   isCompleted;
    public string lastResetDate;
    public int    lastResetWeek;
    public string completionDate;
}

[Serializable]
public class AchievementSaveData
{
    public List<AchievementEntry> achievements;
}

[Serializable]
public class AchievementEntry
{
    public string id;
    public int    currentCount;
    public bool   isCompleted;
    public string lastResetDate;
    public int    lastResetWeek;
    public string completionDate;
}

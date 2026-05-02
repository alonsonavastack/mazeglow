using UnityEngine;
using System;

public class StreakManager : MonoBehaviour
{
    public static StreakManager Instance { get; private set; }

    // Indica si el usuario ya ganó un nivel hoy y aumentó su racha
    public bool StreakClaimedToday { get; private set; } = false;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        // Se ejecuta después de que GameManager ha cargado los datos
        CheckDailyLogin();
    }

    public void CheckDailyLogin()
    {
        var gm = GameManager.Instance;
        if (gm == null) return;

        string todayString = DateTime.Now.ToString("yyyy-MM-dd");
        
        // Si no hay fecha registrada, es primera vez
        if (string.IsNullOrEmpty(gm.lastLoginDate))
        {
            gm.lastLoginDate = todayString;
            gm.currentStreak = 0;
            StreakClaimedToday = false;
            SaveManager.Instance?.SaveAll();
            return;
        }

        DateTime today = DateTime.Now.Date;
        DateTime lastLogin = DateTime.Now.Date;
        DateTime lastStreak = DateTime.Now.Date;

        bool parsedLogin = DateTime.TryParse(gm.lastLoginDate, out lastLogin);
        bool parsedStreak = DateTime.TryParse(gm.lastStreakDate, out lastStreak);

        if (!parsedLogin) lastLogin = today;
        if (!parsedStreak) lastStreak = today.AddDays(-2); // Forzar que no sea válido si falla

        // Calcular días de diferencia desde la última vez que incrementó la racha
        int daysSinceLastStreak = (today - lastStreak).Days;

        // Ya reclamó hoy
        if (daysSinceLastStreak == 0)
        {
            StreakClaimedToday = true;
        }
        else if (daysSinceLastStreak == 1)
        {
            // Entró al día siguiente, racha pendiente de extender
            StreakClaimedToday = false;
        }
        else
        {
            // Pasó más de 1 día sin reclamar racha (o es la primera vez). Racha perdida.
            StreakClaimedToday = false;
            gm.currentStreak = 0;
        }

        // Actualizar último login
        if (gm.lastLoginDate != todayString)
        {
            gm.lastLoginDate = todayString;
            SaveManager.Instance?.SaveAll();
        }
    }

    public void ExtendStreak()
    {
        if (StreakClaimedToday) return; // Ya reclamada hoy

        var gm = GameManager.Instance;
        if (gm == null) return;

        StreakClaimedToday = true;
        gm.currentStreak++;
        gm.lastStreakDate = DateTime.Now.ToString("yyyy-MM-dd");

        if (gm.currentStreak > gm.longestStreak)
        {
            gm.longestStreak = gm.currentStreak;
        }

        SaveManager.Instance?.SaveAll();
        Debug.Log($"[StreakManager] Racha extendida! Racha actual: {gm.currentStreak} días.");
    }
}

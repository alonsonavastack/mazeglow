using UnityEngine;
using System;

/// <summary>
/// NotificationManager — Gestiona notificaciones locales de Android.
/// No requiere internet. Usa Unity Mobile Notifications Package.
///
/// Instalar: Window > Package Manager > "Mobile Notifications" (com.unity.mobile.notifications)
/// </summary>
public class NotificationManager : MonoBehaviour
{
    public static NotificationManager Instance { get; private set; }

    private const string CHANNEL_ID = "mazeglow_channel";

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        InitChannel();
        ScheduleDailyReminder();
    }

    // ── Crear canal de notificaciones (requerido en Android 8+) ──────────────────
    private void InitChannel()
    {
        /*
         * Descomenta después de instalar Mobile Notifications:
         *
         * var channel = new AndroidNotificationChannel
         * {
         *     Id          = CHANNEL_ID,
         *     Name        = "MazeGlow",
         *     Description = "Notificaciones de MazeGlow",
         *     Importance  = Importance.Default
         * };
         * AndroidNotificationCenter.RegisterNotificationChannel(channel);
         */

        Debug.Log("[NotificationManager] Canal inicializado");
    }

    // ── Recordatorio diario (para mantener la racha) ──────────────────────────────
    public void ScheduleDailyReminder()
    {
        /*
         * var notification = new AndroidNotification
         * {
         *     Title        = "¡MazeGlow te espera! 🌿",
         *     Text         = "Mantén tu racha diaria y gana recompensas",
         *     SmallIcon    = "app_icon",
         *     FireTime     = DateTime.Now.AddHours(24)
         * };
         * AndroidNotificationCenter.SendNotification(notification, CHANNEL_ID);
         */

        Debug.Log("[NotificationManager] Recordatorio diario programado");
    }

    // ── Notificación de logro disponible ─────────────────────────────────────────
    public void NotifyAchievementReady(string achievementName)
    {
        /*
         * var notification = new AndroidNotification
         * {
         *     Title    = "¡Logro desbloqueado! 🏆",
         *     Text     = $"Completa '{achievementName}' y recoge tu recompensa",
         *     SmallIcon= "app_icon",
         *     FireTime = DateTime.Now.AddSeconds(1)
         * };
         * AndroidNotificationCenter.SendNotification(notification, CHANNEL_ID);
         */

        Debug.Log($"[NotificationManager] Notificación de logro: {achievementName}");
    }

    // ── Notificación de vidas recargadas ─────────────────────────────────────────
    public void ScheduleLivesRefill(int minutesUntilFull)
    {
        /*
         * var notification = new AndroidNotification
         * {
         *     Title    = "¡Vidas recargadas! ❤️",
         *     Text     = "Ya tienes todas tus vidas. ¡Sigue jugando!",
         *     SmallIcon= "app_icon",
         *     FireTime = DateTime.Now.AddMinutes(minutesUntilFull)
         * };
         * AndroidNotificationCenter.SendNotification(notification, CHANNEL_ID);
         */

        Debug.Log($"[NotificationManager] Vidas listas en {minutesUntilFull} minutos");
    }
}

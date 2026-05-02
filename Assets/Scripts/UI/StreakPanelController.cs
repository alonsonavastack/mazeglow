using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class StreakPanelController : MonoBehaviour
{
    [Header("Referencias UI")]
    public TextMeshProUGUI streakCountText;
    public TextMeshProUGUI messageText;
    
    [Header("Iconos de los Días (L, M, M, J, V, S, D)")]
    public Image[] dayIcons = new Image[7]; // Asignar en el inspector, de Lunes a Domingo

    [Header("Colores")]
    public Color completedColor = new Color(1f, 0.6f, 0f); // Naranja
    public Color pendingColor = new Color(0.3f, 0.3f, 0.4f); // Gris oscuro

    private void OnEnable()
    {
        RefreshPanel();
    }

    public void RefreshPanel()
    {
        var gm = GameManager.Instance;
        if (gm == null) return;

        // Actualizar el texto principal
        if (streakCountText != null)
        {
            streakCountText.text = $"Racha de {gm.currentStreak} días";
        }

        // Actualizar el mensaje de abajo
        if (messageText != null)
        {
            if (StreakManager.Instance != null && StreakManager.Instance.StreakClaimedToday)
                messageText.text = "¡Racha extendida! Vuelve mañana para seguir sumando.";
            else
                messageText.text = "¡Gana un nivel hoy para extender tu racha!";
        }

        // Actualizar los iconos de los días (cálculo simplificado basado en el número de racha)
        // Ejemplo: Si la racha es 3, marcamos 3 días consecutivos desde el lunes (o desde el día actual)
        // Para hacerlo como en la imagen (donde Ma está marcado y el resto gris),
        // simularemos pintando el día de la semana actual y los anteriores según la racha.
        
        int currentDayOfWeek = (int)System.DateTime.Now.DayOfWeek;
        // Ajustar DayOfWeek para que Lunes sea 0 y Domingo 6
        int adjustedDay = currentDayOfWeek == 0 ? 6 : currentDayOfWeek - 1;

        for (int i = 0; i < 7; i++)
        {
            if (dayIcons[i] == null) continue;

            // Lógica simple: iluminar los días anteriores que formen parte de la racha actual
            // y el día actual si ya fue reclamado.
            bool isCompleted = false;

            if (i < adjustedDay)
            {
                // Días anteriores en esta semana
                int daysAgo = adjustedDay - i;
                if (gm.currentStreak > daysAgo || (gm.currentStreak == daysAgo && StreakManager.Instance != null && StreakManager.Instance.StreakClaimedToday))
                {
                    isCompleted = true;
                }
            }
            else if (i == adjustedDay)
            {
                // Hoy
                if (StreakManager.Instance != null && StreakManager.Instance.StreakClaimedToday)
                {
                    isCompleted = true;
                }
            }

            dayIcons[i].color = isCompleted ? completedColor : pendingColor;
        }
    }

    public void ClosePanel()
    {
        gameObject.SetActive(false);
    }
}

using UnityEngine;
using TMPro;

public class CollectionPanelController : MonoBehaviour
{
    [Header("Referencias Récords")]
    public TextMeshProUGUI longestStreakText;
    public TextMeshProUGUI maxWinsStreakText;
    public TextMeshProUGUI totalWinsText;

    private void OnEnable()
    {
        RefreshCollection();
    }

    public void RefreshCollection()
    {
        var gm = GameManager.Instance;
        if (gm == null) return;

        // Actualizar la sección superior "Récords"
        if (longestStreakText != null)
        {
            longestStreakText.text = gm.longestStreak.ToString();
        }

        // Para este ejemplo, usamos el currentLevel como victorias totales (ya que cada nivel es una victoria)
        int totalWins = Mathf.Max(0, gm.currentLevel - 1);
        if (totalWinsText != null)
        {
            totalWinsText.text = totalWins.ToString();
        }

        // Mayor racha de victorias puede ser la misma longestStreak, o si el usuario quiere algo específico en el futuro
        if (maxWinsStreakText != null)
        {
            maxWinsStreakText.text = gm.longestStreak.ToString();
        }
    }

    public void CloseCollection()
    {
        gameObject.SetActive(false);
    }
}

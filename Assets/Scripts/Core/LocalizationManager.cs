using UnityEngine;

/// <summary>
/// LocalizationManager — Maneja el idioma del juego.
/// Agrega más textos aquí para traducir toda la app.
/// </summary>
public class LocalizationManager : MonoBehaviour
{
    public static LocalizationManager Instance { get; private set; }

    private string currentLanguage = "es";

    // Diccionario de textos por clave e idioma
    private System.Collections.Generic.Dictionary<string, System.Collections.Generic.Dictionary<string, string>> texts
        = new System.Collections.Generic.Dictionary<string, System.Collections.Generic.Dictionary<string, string>>
    {
        // ── Textos del menú ────────────────────────────────────────────────────────
        { "btn_play",      new() { {"es","Jugar"},         {"en","Play"}           }},
        { "lbl_level",     new() { {"es","Nivel"},         {"en","Level"}          }},
        { "lbl_coins",     new() { {"es","Monedas"},       {"en","Coins"}          }},
        { "lbl_lives",     new() { {"es","Vidas"},         {"en","Lives"}          }},
        { "lbl_hints",     new() { {"es","Pistas"},        {"en","Hints"}          }},

        // ── Textos de configuración ────────────────────────────────────────────────
        { "cfg_language",  new() { {"es","Idioma"},        {"en","Language"}       }},
        { "cfg_vibrations",new() { {"es","Vibraciones"},   {"en","Vibrations"}     }},
        { "cfg_sounds",    new() { {"es","Sonidos"},       {"en","Sounds"}         }},
        { "cfg_darkmode",  new() { {"es","Modo oscuro"},   {"en","Dark mode"}      }},
        { "cfg_account",   new() { {"es","Conexión de cuenta"},{"en","Account"}    }},
        { "cfg_remove_ads",new() { {"es","Eliminar anuncios"},{"en","Remove ads"}  }},
        { "cfg_restore",   new() { {"es","Restaurar compras"},{"en","Restore purchases"}}},
        { "cfg_rate",      new() { {"es","Califícanos"},   {"en","Rate us"}        }},
        { "cfg_contact",   new() { {"es","Escríbenos"},    {"en","Contact us"}     }},
        { "cfg_privacy",   new() { {"es","Privacidad"},    {"en","Privacy"}        }},

        // ── Textos de juego ────────────────────────────────────────────────────────
        { "tap_to_move",   new() { {"es","Toca para mover"},{"en","Tap to move"}   }},
        { "game_over",     new() { {"es","¡Sin vidas!"},   {"en","Game Over!"}     }},
        { "level_complete",new() { {"es","¡Impresionante!"},{"en","Amazing!"}      }},

        // ── Textos de logros ───────────────────────────────────────────────────────
        { "ach_unlocked",  new() { {"es","¡Logro desbloqueado!"},{"en","Achievement Unlocked!"}}},
    };

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void SetLanguage(string lang)
    {
        currentLanguage = lang;
        // Notificar a todos los textos de UI que se actualicen
        // (Si usas un sistema de textos localizados, llama RefreshAll() aquí)
    }

    public string Get(string key)
    {
        if (texts.TryGetValue(key, out var langs))
            if (langs.TryGetValue(currentLanguage, out var text))
                return text;

        return $"[{key}]"; // Devuelve la clave si no encuentra la traducción
    }
}

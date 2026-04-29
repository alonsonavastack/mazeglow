#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

/// <summary>
/// MazeSceneCleaner — Herramienta de editor.
/// Aparece en el menú: MazeGlow > Limpiar Exit duplicado
/// Borra el Exit estático que fue creado manualmente en el editor
/// dentro del objeto MazeGenerator.
/// </summary>
public class MazeSceneCleaner
{
    [MenuItem("MazeGlow/Limpiar Exit duplicado de la escena")]
    public static void CleanStaticExits()
    {
        // Buscar el objeto MazeGenerator en la escena
        var mazeGen = Object.FindAnyObjectByType<MazeGenerator>();

        if (mazeGen == null)
        {
            EditorUtility.DisplayDialog(
                "MazeGlow Cleaner",
                "No se encontró MazeGenerator en la escena activa.\nAbre la escena Game y vuelve a intentarlo.",
                "OK");
            return;
        }

        int count = 0;

        // Recorrer todos los hijos de MazeGenerator
        // Necesitamos una lista porque vamos a destruir mientras iteramos
        var children = new System.Collections.Generic.List<GameObject>();
        foreach (Transform child in mazeGen.transform)
            children.Add(child.gameObject);

        foreach (var child in children)
        {
            if (child == null) continue;

            // Borrar si tiene tag "Exit" O si se llama "Exit"
            if (child.name == "Exit" || child.CompareTag("Exit"))
            {
                Debug.Log("[MazeSceneCleaner] Borrando: " + child.name +
                          " en posición " + child.transform.position);
                Undo.DestroyObjectImmediate(child);
                count++;
            }
        }

        // También buscar cualquier Exit suelto en la escena raíz
        var allExits = GameObject.FindGameObjectsWithTag("Exit");
        foreach (var exit in allExits)
        {
            if (exit == null) continue;
            Debug.Log("[MazeSceneCleaner] Borrando Exit suelto: " + exit.name);
            Undo.DestroyObjectImmediate(exit);
            count++;
        }

        // Guardar la escena
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());

        if (count > 0)
        {
            EditorUtility.DisplayDialog(
                "MazeGlow Cleaner",
                "✅ Se borraron " + count + " objeto(s) Exit duplicados.\n\nGuarda la escena con Cmd+S.",
                "OK");
        }
        else
        {
            EditorUtility.DisplayDialog(
                "MazeGlow Cleaner",
                "No se encontraron objetos Exit duplicados.\nLa escena ya está limpia.",
                "OK");
        }
    }

    [MenuItem("MazeGlow/Verificar configuración de la escena Game")]
    public static void VerifyScene()
    {
        var report = new System.Text.StringBuilder();
        report.AppendLine("=== Verificación de la escena Game ===\n");

        // GameController
        var gc = Object.FindAnyObjectByType<GameController>();
        report.AppendLine(gc != null ? "✅ GameController encontrado" : "❌ FALTA GameController");
        if (gc != null)
        {
            report.AppendLine(gc.mazeGenerator != null ? "  ✅ MazeGenerator asignado" : "  ❌ MazeGenerator NO asignado — arrástralo en el Inspector");
            report.AppendLine(gc.player        != null ? "  ✅ Player asignado"        : "  ❌ Player NO asignado — arrástralo en el Inspector");
            report.AppendLine(gc.celebrationFX != null ? "  ✅ CelebrationFX asignado" : "  ⚠️  CelebrationFX no asignado (opcional)");
        }

        // GameManager
        var gm = Object.FindAnyObjectByType<GameManager>();
        report.AppendLine(gm != null ? "✅ GameManager encontrado" : "⚠️  GameManager no encontrado (se carga desde MainMenu)");

        // MazeGenerator
        var mg = Object.FindAnyObjectByType<MazeGenerator>();
        report.AppendLine(mg != null ? "✅ MazeGenerator encontrado" : "❌ FALTA MazeGenerator en la escena");

        // Player
        var player = Object.FindAnyObjectByType<PlayerController>();
        report.AppendLine(player != null ? "✅ PlayerController encontrado" : "❌ FALTA objeto Player con PlayerController");

        // Exits estáticos
        var exits = GameObject.FindGameObjectsWithTag("Exit");
        if (exits.Length == 0)
            report.AppendLine("✅ No hay Exits estáticos (correcto)");
        else
            report.AppendLine("⚠️  Hay " + exits.Length + " objeto(s) con tag Exit en la escena. Usa MazeGlow > Limpiar Exit duplicado");

        // UIManager
        var ui = Object.FindAnyObjectByType<UIManager>();
        report.AppendLine(ui != null ? "✅ UIManager encontrado" : "⚠️  UIManager no encontrado (se crea automáticamente)");

        Debug.Log(report.ToString());
        EditorUtility.DisplayDialog("Verificación de escena", report.ToString(), "OK");
    }
}
#endif

using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    public static SceneLoader Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void GoToMenu()      { SceneManager.LoadScene(0); }
    public void GoToMainMenu()  { SceneManager.LoadScene(0); }   // alias para UIManager
    public void GoToGame()      { SceneManager.LoadScene(1); }
    public void GoToResults()   { SceneManager.LoadScene(2); }
    public void GoToSettings()  { SceneManager.LoadScene(3); }
}

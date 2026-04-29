using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

/// <summary>
/// AudioManager v3
/// - Música diferente para MainMenu y para Game
/// - Cambia automáticamente al detectar cambio de escena
/// - Play() con null-check de GameManager
/// </summary>
public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [System.Serializable]
    public class Sound
    {
        public string    name;
        public AudioClip clip;
        [Range(0f, 1f)] public float volume = 1f;
        public bool loop = false;
    }

    [Header("Efectos de sonido")]
    public Sound[] sounds;

    [Header("Música — arrastra aquí tus archivos MP3/OGG")]
    public AudioClip musicMenu;   // Música del menú principal
    public AudioClip musicGame;   // Música del laberinto (juego)
    [Range(0f, 1f)]
    public float musicVolume = 0.45f;

    private AudioSource musicSource;
    private AudioClip   currentClip;
    private readonly Dictionary<string, Sound> soundMap = new Dictionary<string, Sound>();
    private readonly List<AudioSource>         sfxPool  = new List<AudioSource>();

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Diccionario de efectos
        foreach (var s in sounds)
            if (s != null && !string.IsNullOrEmpty(s.name))
                soundMap[s.name] = s;

        // Source de música
        musicSource             = gameObject.AddComponent<AudioSource>();
        musicSource.loop        = true;
        musicSource.volume      = musicVolume;
        musicSource.playOnAwake = false;

        // Buscar clips automáticamente si no están asignados en el Inspector
        AutoFindClips();

        // Escuchar cambios de escena
        SceneManager.sceneLoaded += OnSceneLoaded;

        // Reproducir música de la escena actual
        PlayMusicForScene(SceneManager.GetActiveScene().buildIndex);
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    // ── Cambio de escena ──────────────────────────────────────────────────────────
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        PlayMusicForScene(scene.buildIndex);
    }

    /// <summary>
    /// Escena 0 = MainMenu → musicMenu
    /// Escena 1 = Game     → musicGame
    /// Cualquier otra      → silencio
    /// </summary>
    private void PlayMusicForScene(int sceneIndex)
    {
        AudioClip target = null;

        if (sceneIndex == 0)       target = musicMenu;   // MainMenu
        else if (sceneIndex == 1)  target = musicGame;   // Game

        if (target == null)
        {
            StopMusic();
            Debug.LogWarning("[AudioManager] Sin música para escena " + sceneIndex +
                             ". Asigna los clips en el Inspector del _Managers.");
            return;
        }

        // Si ya está sonando el mismo clip, no interrumpir
        if (musicSource.isPlaying && currentClip == target) return;

        currentClip        = target;
        musicSource.clip   = target;
        musicSource.volume = musicVolume;
        musicSource.loop   = true;
        musicSource.Stop();
        musicSource.Play();

        Debug.Log("[AudioManager] Reproduciendo música: " + target.name +
                  " (escena " + sceneIndex + ")");
    }

    // ── Buscar clips por nombre si no están asignados ─────────────────────────────
    private void AutoFindClips()
    {
#if UNITY_EDITOR
        if (musicMenu == null) musicMenu = FindClipInEditor("musicamenu", "menumusic", "mainmenu");
        if (musicGame == null) musicGame = FindClipInEditor("musicajuego", "gamemusic", "audiofondo", "game");

        if (musicMenu != null) Debug.Log("[AudioManager] musicMenu: " + musicMenu.name);
        if (musicGame != null) Debug.Log("[AudioManager] musicGame: " + musicGame.name);
#endif
        // Fallback: Resources folder
        if (musicMenu == null) musicMenu = Resources.Load<AudioClip>("musicamenu");
        if (musicGame == null) musicGame = Resources.Load<AudioClip>("musicajuego");
        if (musicGame == null) musicGame = Resources.Load<AudioClip>("audiofondo");
    }

#if UNITY_EDITOR
    private AudioClip FindClipInEditor(params string[] names)
    {
        foreach (var name in names)
        {
            string[] guids = UnityEditor.AssetDatabase.FindAssets(name + " t:AudioClip");
            if (guids.Length > 0)
            {
                var path = UnityEditor.AssetDatabase.GUIDToAssetPath(guids[0]);
                var clip = UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>(path);
                if (clip != null) return clip;
            }
        }
        return null;
    }
#endif

    // ── Efectos de sonido ─────────────────────────────────────────────────────────
    public void Play(string soundName)
    {
        if (GameManager.Instance != null && !GameManager.Instance.soundEnabled) return;
        if (!soundMap.TryGetValue(soundName, out Sound s) || s.clip == null) return;

        var src = GetFreeSource();
        src.clip   = s.clip;
        src.volume = s.volume;
        src.loop   = s.loop;
        src.Play();
    }

    // ── Control de música ─────────────────────────────────────────────────────────
    public void StopMusic()   => musicSource?.Stop();
    public void PauseMusic()  => musicSource?.Pause();
    public void ResumeMusic() => musicSource?.UnPause();

    public void SetMute(bool mute)
        => AudioListener.volume = mute ? 0f : 1f;

    // ── Pool de AudioSources ──────────────────────────────────────────────────────
    private AudioSource GetFreeSource()
    {
        foreach (var src in sfxPool)
            if (src != null && !src.isPlaying) return src;
        var newSrc = gameObject.AddComponent<AudioSource>();
        sfxPool.Add(newSrc);
        return newSrc;
    }
}

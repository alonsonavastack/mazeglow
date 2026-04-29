using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// MazeGenerator v6 — Usa MazeCameraController para vista cenital + transición isométrica.
/// </summary>
public class MazeGenerator : MonoBehaviour
{
    public static MazeGenerator Instance { get; private set; }

    [Header("Tamaño del laberinto")]
    public int baseWidth  = 9;
    public int baseHeight = 9;
    public int growEvery  = 5;
    public int maxSize    = 47;

    [Header("Prefabs (dejar vacíos — se crean solos)")]
    public GameObject wallPrefab;
    public GameObject floorPrefab;
    public GameObject exitPrefab;

    [Header("Materiales (opcionales)")]
    public Material wallMaterial;
    public Material floorMaterial;
    public Material exitMaterial;

    [Header("Iluminación 3D")]
    public bool  createLights       = true;
    public Color ambientColor       = new Color(0.08f, 0.06f, 0.12f);
    public Color dirLightColor      = new Color(0.60f, 0.55f, 0.90f);
    public Color exitLightColor     = new Color(1.00f, 0.90f, 0.30f);
    [Range(0.5f,5f)]  public float exitLightIntensity = 2.5f;
    [Range(1f, 20f)]  public float exitLightRange     = 6f;

    private static readonly Color ColWall     = new Color(0.10f, 0.09f, 0.28f);
    private static readonly Color ColWallEmit = new Color(0.04f, 0.03f, 0.18f);
    private static readonly Color ColFloor    = new Color(0.18f, 0.16f, 0.30f);
    private static readonly Color ColStart    = new Color(0.10f, 0.70f, 0.20f);
    private static readonly Color ColStartEm  = new Color(0.00f, 0.30f, 0.05f);
    private static readonly Color ColExit     = new Color(1.00f, 0.80f, 0.10f);
    private static readonly Color ColExitEm   = new Color(0.80f, 0.55f, 0.00f);

    private bool[,]  grid;
    private int      W, H;
    private Light    exitLight;
    private Coroutine pulseRoutine;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    // ── API pública ───────────────────────────────────────────────────────────────
    public void GenerateMaze(int level)
    {
        ClearMaze();
        SetupLighting();

        int extra = (level - 1) / growEvery;
        W = Mathf.Min(baseWidth  + extra * 2, maxSize);
        H = Mathf.Min(baseHeight + extra * 2, maxSize);
        if (W % 2 == 0) W++;
        if (H % 2 == 0) H++;

        grid = new bool[W, H];
        Fill(true);
        Carve(1, 1);
        Build();

        // Iniciar secuencia de cámara: cenital 2s → transición → isométrica siguiendo al jugador
        var cam = FindAnyObjectByType<MazeCameraController>();
        if (cam != null)
        {
            Vector3 center = new Vector3(W / 2f, 0f, H / 2f);
            cam.StartLevelCamera(center, W, H);
        }

        Debug.Log("[MazeGen] Nivel " + level + " -> " + W + "x" + H);
    }

    public void ClearMaze()
    {
        if (pulseRoutine != null) { StopCoroutine(pulseRoutine); pulseRoutine = null; }
        if (exitLight    != null) { Destroy(exitLight.gameObject); exitLight = null; }

        var children = new List<GameObject>();
        foreach (Transform child in transform)
            children.Add(child.gameObject);

        foreach (var child in children)
            if (child != null) Destroy(child);

        Debug.Log("[MazeGen] ClearMaze: destruidos " + children.Count + " hijos.");
    }

    public int GetWidth()  => W;
    public int GetHeight() => H;

    // ── Algoritmo DFS ─────────────────────────────────────────────────────────────
    private void Fill(bool val)
    {
        for (int x = 0; x < W; x++)
        for (int z = 0; z < H; z++)
            grid[x, z] = val;
    }

    private void Carve(int cx, int cz)
    {
        int[] dx = { 0, 0, -2,  2 };
        int[] dz = { 2, -2,  0,  0 };
        Shuffle(dx, dz);
        for (int i = 0; i < 4; i++)
        {
            int nx = cx + dx[i], nz = cz + dz[i];
            if (nx > 0 && nx < W-1 && nz > 0 && nz < H-1 && grid[nx, nz])
            {
                grid[cx + dx[i]/2, cz + dz[i]/2] = false;
                grid[nx, nz] = false;
                Carve(nx, nz);
            }
        }
    }

    private void Shuffle(int[] a, int[] b)
    {
        for (int i = a.Length-1; i > 0; i--)
        {
            int j = Random.Range(0, i+1);
            (a[i], a[j]) = (a[j], a[i]);
            (b[i], b[j]) = (b[j], b[i]);
        }
    }

    // ── Construcción 3D ───────────────────────────────────────────────────────────
    private void Build()
    {
        for (int x = 0; x < W; x++)
        for (int z = 0; z < H; z++)
        {
            if (grid[x, z]) MakeWall(x, z);
            else            MakeFloor(x, z);
        }

        MakeStart(1, 1);

        int ex = W - 2, ez = H - 2;
        grid[ex, ez] = false;
        MakeExit(ex, ez);

        if (createLights) CreateExitLight(ex, ez);

        StartCoroutine(PlacePlayer());
    }

    private void MakeWall(int x, int z)
    {
        var obj = wallPrefab != null
            ? Instantiate(wallPrefab, new Vector3(x, 0.5f, z), Quaternion.identity, transform)
            : Cube(new Vector3(x, 0.5f, z), Vector3.one);

        obj.name = "Wall_" + x + "_" + z;
        obj.tag  = "Wall";
        Paint(obj, ColWall, ColWallEmit, wallMaterial, 0.2f);

        var bc = GetOrAdd<BoxCollider>(obj);
        bc.isTrigger = false;
        bc.size      = Vector3.one;
    }

    private void MakeFloor(int x, int z)
    {
        var obj = floorPrefab != null
            ? Instantiate(floorPrefab, new Vector3(x, -0.05f, z), Quaternion.identity, transform)
            : Cube(new Vector3(x, -0.05f, z), new Vector3(1f, 0.1f, 1f));

        obj.name = "Floor_" + x + "_" + z;
        Paint(obj, ColFloor, Color.black, floorMaterial, 0.6f);

        foreach (var c in obj.GetComponents<Collider>()) Destroy(c);
    }

    private void MakeStart(int x, int z)
    {
        var obj = Cube(new Vector3(x, 0.02f, z), new Vector3(0.85f, 0.08f, 0.85f));
        obj.name = "Start";
        Paint(obj, ColStart, ColStartEm, null, 0.8f);
        foreach (var c in obj.GetComponents<Collider>()) Destroy(c);
    }

    private void MakeExit(int x, int z)
    {
        var obj = exitPrefab != null
            ? Instantiate(exitPrefab, new Vector3(x, 0.02f, z), Quaternion.identity, transform)
            : Cube(new Vector3(x, 0.02f, z), new Vector3(0.9f, 0.12f, 0.9f));

        obj.name = "Exit";
        obj.tag  = "Exit";
        Paint(obj, ColExit, ColExitEm, exitMaterial, 1.0f);

        foreach (var c in obj.GetComponents<Collider>()) Destroy(c);
        var bc       = obj.AddComponent<BoxCollider>();
        bc.isTrigger = true;
        bc.center    = new Vector3(0f, 0.75f, 0f);
        bc.size      = new Vector3(0.9f, 1.5f, 0.9f);

        Debug.Log("[MazeGen] Exit creado en (" + x + "," + z + ") isTrigger=" + bc.isTrigger);
    }

    // ── Iluminación ───────────────────────────────────────────────────────────────
    private void SetupLighting()
    {
        if (!createLights) return;
        RenderSettings.ambientMode  = UnityEngine.Rendering.AmbientMode.Flat;
        RenderSettings.ambientLight = ambientColor;

        var dl = FindAnyObjectByType<Light>();
        if (dl != null && dl.type == LightType.Directional)
        {
            dl.color     = dirLightColor;
            dl.intensity = 0.8f;
        }
    }

    private void CreateExitLight(int x, int z)
    {
        var go = new GameObject("ExitLight");
        go.transform.SetParent(transform);
        go.transform.position = new Vector3(x, 2f, z);
        exitLight           = go.AddComponent<Light>();
        exitLight.type      = LightType.Point;
        exitLight.color     = exitLightColor;
        exitLight.intensity = exitLightIntensity;
        exitLight.range     = exitLightRange;
        pulseRoutine = StartCoroutine(PulseLight());
    }

    private IEnumerator PulseLight()
    {
        float t = 0f, baseI = exitLightIntensity;
        while (true)
        {
            t += Time.deltaTime * 1.8f;
            if (exitLight != null) exitLight.intensity = baseI + Mathf.Sin(t) * 0.8f;
            yield return null;
        }
    }

    // ── Helpers ───────────────────────────────────────────────────────────────────
    private GameObject Cube(Vector3 pos, Vector3 scale)
    {
        var obj = GameObject.CreatePrimitive(PrimitiveType.Cube);
        obj.transform.SetParent(transform);
        obj.transform.position   = pos;
        obj.transform.localScale = scale;
        return obj;
    }

    private static void Paint(GameObject obj, Color main, Color emit, Material src, float smooth)
    {
        var rend = obj.GetComponent<Renderer>();
        if (rend == null) return;
        var mat = src != null ? new Material(src) : new Material(Shader.Find("Standard"));
        mat.color = main;
        if (emit != Color.black) { mat.EnableKeyword("_EMISSION"); mat.SetColor("_EmissionColor", emit); }
        mat.SetFloat("_Metallic",   0f);
        mat.SetFloat("_Glossiness", smooth);
        rend.material = mat;
    }

    private static T GetOrAdd<T>(GameObject obj) where T : Component
    {
        return obj.GetComponent<T>() ?? obj.AddComponent<T>();
    }

    private IEnumerator PlacePlayer()
    {
        yield return null;
        var p = FindAnyObjectByType<PlayerController>();
        if (p != null) p.ResetForNewLevel();
    }
}

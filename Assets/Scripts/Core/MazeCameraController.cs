using UnityEngine;
using System.Collections;

/// <summary>
/// MazeCameraController v2 — Robusto. Siempre encuentra al jugador.
/// Fase 1: Vista cenital del laberinto completo (2 segundos).
/// Fase 2: Transición suave a vista isométrica siguiendo al jugador.
/// Fase 3: Seguimiento continuo al jugador.
/// </summary>
public class MazeCameraController : MonoBehaviour
{
    [Header("Objetivo")]
    public Transform player;

    [Header("Vista cenital (inicio de nivel)")]
    public float overviewDuration  = 2.0f;
    public float overviewHeight    = 22f;
    public float overviewTilt      = 88f;

    [Header("Vista isométrica (juego normal)")]
    public float isoHeight         = 12f;
    public float isoDistance       = 9f;
    public float isoTilt           = 52f;
    public float isoYaw            = 35f;

    [Header("Transición")]
    public float transitionDuration = 1.5f;
    public AnimationCurve transitionCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    [Header("Seguimiento")]
    public float followSmooth = 5f;

    private enum CamState { Overview, Transitioning, Following }
    private CamState state = CamState.Overview;

    // ── Buscar jugador en cada llamada ────────────────────────────────────────────
    private Transform GetPlayer()
    {
        if (player != null) return player;
        var pc = FindAnyObjectByType<PlayerController>();
        if (pc != null) { player = pc.transform; }
        return player;
    }

    private Vector3 IsoOffset()
    {
        float rad = isoYaw * Mathf.Deg2Rad;
        return new Vector3(
            Mathf.Sin(rad) * isoDistance * 0.6f,
            isoHeight,
            -Mathf.Cos(rad) * isoDistance);
    }

    private Vector3 IsoPosition()
    {
        var p = GetPlayer();
        if (p == null) return transform.position;
        return p.position + IsoOffset();
    }

    private Quaternion IsoRotation() => Quaternion.Euler(isoTilt, isoYaw, 0f);

    // ── API pública ───────────────────────────────────────────────────────────────
    public void StartLevelCamera(Vector3 mazeCenter, int mazeWidth, int mazeHeight)
    {
        Debug.Log("[MazeCam] StartLevelCamera llamado. Centro=" + mazeCenter + " Size=" + mazeWidth + "x" + mazeHeight);
        StopAllCoroutines();
        state = CamState.Overview;

        float size   = Mathf.Max(mazeWidth, mazeHeight);
        float height = Mathf.Clamp(size * 1.1f, overviewHeight, 40f);

        transform.position = new Vector3(mazeCenter.x, height, mazeCenter.z);
        transform.rotation = Quaternion.Euler(overviewTilt, 0f, 0f);

        StartCoroutine(CameraSequence());
    }

    public void SnapToIso()
    {
        StopAllCoroutines();
        state              = CamState.Following;
        transform.position = IsoPosition();
        transform.rotation = IsoRotation();
    }

    // ── Secuencia ─────────────────────────────────────────────────────────────────
    private IEnumerator CameraSequence()
    {
        // Fase 1: Vista cenital
        state = CamState.Overview;
        yield return new WaitForSeconds(overviewDuration);

        // Esperar al jugador (máx 5 segundos)
        float waited = 0f;
        while (GetPlayer() == null && waited < 5f)
        {
            waited += Time.deltaTime;
            yield return null;
        }

        if (GetPlayer() == null)
        {
            Debug.LogWarning("[MazeCam] Jugador no encontrado. La cámara no transicionará.");
            yield break;
        }

        Debug.Log("[MazeCam] Jugador encontrado: " + player.name + ". Iniciando transición.");

        // Fase 2: Transición a isométrica
        state = CamState.Transitioning;
        Vector3    startPos = transform.position;
        Quaternion startRot = transform.rotation;
        float elapsed = 0f;

        while (elapsed < transitionDuration)
        {
            elapsed += Time.deltaTime;
            float t = transitionCurve.Evaluate(Mathf.Clamp01(elapsed / transitionDuration));
            transform.position = Vector3.Lerp(startPos, IsoPosition(), t);
            transform.rotation = Quaternion.Slerp(startRot, IsoRotation(), t);
            yield return null;
        }

        // Fase 3: Seguimiento
        state = CamState.Following;
        Debug.Log("[MazeCam] Modo Following activado.");
    }

    // ── Seguimiento continuo ──────────────────────────────────────────────────────
    private void LateUpdate()
    {
        if (state != CamState.Following) return;
        if (GetPlayer() == null) return;

        transform.position = Vector3.Lerp(transform.position, IsoPosition(), Time.deltaTime * followSmooth);
        transform.rotation = Quaternion.Slerp(transform.rotation, IsoRotation(), Time.deltaTime * followSmooth);
    }
}

using UnityEngine;

/// <summary>
/// CameraFollow — Cámara que sigue al jugador con vista isométrica 3D.
/// Soporta rotación orbital desde CameraJoystick.
/// </summary>
public class CameraFollow : MonoBehaviour
{
    [Header("Objetivo")]
    public Transform target;

    [Header("Configuración de cámara")]
    public float followHeight   = 10f;
    public float followDistance =  8f;
    public float tiltAngle      = 58f;
    public float suavidad       =  6f;

    // Estado orbital (lo controla CameraJoystick)
    private float orbitAngleY = 0f;
    private float orbitTilt   = 58f;
    private bool  orbitActivo = false;

    private void LateUpdate()
    {
        if (target == null) return;

        Vector3 pivot = target.position;

        if (orbitActivo)
        {
            float rad      = followDistance;
            float angleRad = orbitAngleY * Mathf.Deg2Rad;
            float offsetX  = Mathf.Sin(angleRad) * rad;
            float offsetZ  = -Mathf.Cos(angleRad) * rad;

            Vector3 targetPos = new Vector3(pivot.x + offsetX, followHeight, pivot.z + offsetZ);
            transform.position = Vector3.Lerp(transform.position, targetPos, Time.deltaTime * suavidad);
            transform.rotation = Quaternion.Lerp(transform.rotation,
                Quaternion.Euler(orbitTilt, orbitAngleY, 0f), Time.deltaTime * suavidad);
        }
        else
        {
            Vector3 targetPos = new Vector3(pivot.x, pivot.y + followHeight, pivot.z - followDistance);
            transform.position = Vector3.Lerp(transform.position, targetPos, Time.deltaTime * suavidad);
            transform.LookAt(pivot + Vector3.up * 1.5f);
        }
    }

    /// <summary>Llamado por CameraJoystick para actualizar el ángulo orbital.</summary>
    public void SetOrbitAngle(float angleY, float tilt)
    {
        orbitAngleY = angleY;
        orbitTilt   = tilt;
        orbitActivo = true;
    }

    /// <summary>Volver al modo seguimiento normal sin órbita.</summary>
    public void ResetOrbit()
    {
        orbitActivo = false;
    }

    /// <summary>
    /// Llamado por MazeGenerator con el ancho y alto del laberinto.
    /// Ajusta followHeight y followDistance según el tamaño.
    /// </summary>
    public void SetMazeBounds(int width, int height)
    {
        float size     = Mathf.Max(width, height);
        followHeight   = Mathf.Clamp(size * 0.85f, 8f, 30f);
        followDistance = Mathf.Clamp(size * 0.55f, 6f, 22f);
    }

    /// <summary>
    /// Teletransporta la cámara instantáneamente al jugador (sin lerp).
    /// </summary>
    public void SnapToTarget()
    {
        if (target == null) return;
        Vector3 pivot      = target.position;
        transform.position = new Vector3(pivot.x, pivot.y + followHeight, pivot.z - followDistance);
        transform.LookAt(pivot + Vector3.up * 1.5f);
        orbitActivo        = false;
    }
}

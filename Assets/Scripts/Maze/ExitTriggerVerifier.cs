using UnityEngine;

/// <summary>
/// ExitTriggerVerifier — Verifica que el Exit tenga Rigidbody y BoxCollider como trigger.
/// Se auto-arregla al iniciar.
/// </summary>
public class ExitTriggerVerifier : MonoBehaviour
{
    private void Start()
    {
        FixTrigger();
    }

    private void FixTrigger()
    {
        // Verificar Rigidbody: no es necesario para el trigger de salida.
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            Debug.LogWarning("[ExitVerifier] Quitando Rigidbody innecesario de " + gameObject.name);
            Destroy(rb);
        }
        
        // Verificar BoxCollider
        BoxCollider bc = GetComponent<BoxCollider>();
        if (bc == null)
        {
            Debug.LogWarning("[ExitVerifier] Agregando BoxCollider a " + gameObject.name);
            bc = gameObject.AddComponent<BoxCollider>();
            bc.size = new Vector3(0.9f, 1.5f, 0.9f);
            bc.center = new Vector3(0f, 0.75f, 0f);
        }
        
        if (!bc.isTrigger)
        {
            Debug.LogWarning("[ExitVerifier] Habilitando trigger en " + gameObject.name);
            bc.isTrigger = true;
        }
        
        Debug.Log("[ExitVerifier] ✓ Exit " + gameObject.name + " verificado. Rigidbody=" + (rb != null) + ", Trigger=" + bc.isTrigger);
    }
}

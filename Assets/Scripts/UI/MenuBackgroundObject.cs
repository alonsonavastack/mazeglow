using UnityEngine;

/// <summary>
/// MenuBackgroundObject — Objeto 3D decorativo para el menú principal.
/// Crea una esfera o cubo que rota lentamente con materiales brillantes.
/// </summary>
public class MenuBackgroundObject : MonoBehaviour
{
    public float rotationSpeed = 20f;
    public Material objectMaterial;

    private void Start()
    {
        // Crear una esfera primitiva si no hay mesh
        if (GetComponent<MeshFilter>() == null)
        {
            GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            sphere.transform.SetParent(transform);
            sphere.transform.localPosition = Vector3.zero;
            sphere.transform.localScale = Vector3.one * 2f;

            // Asignar material si existe
            if (objectMaterial != null)
            {
                sphere.GetComponent<Renderer>().material = objectMaterial;
            }
            else
            {
                // Material básico brillante
                Renderer rend = sphere.GetComponent<Renderer>();
                rend.material.color = new Color(0.5f, 0.7f, 1f);
                rend.material.EnableKeyword("_EMISSION");
                rend.material.SetColor("_EmissionColor", new Color(0.1f, 0.2f, 0.4f));
            }
        }
    }

    private void Update()
    {
        // Rotar el objeto
        transform.Rotate(Vector3.up * rotationSpeed * Time.deltaTime);
    }
}
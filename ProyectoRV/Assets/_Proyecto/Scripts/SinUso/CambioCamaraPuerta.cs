using UnityEngine;

// Ponlo en un objeto vacío colocado en el centro del marco de la puerta ("PuertaCamara").
// Cuando la moto entra en el radio, se activa la tercera cámara y se apaga la que había.
public class CambioCamaraPuerta : MonoBehaviour
{
    [Tooltip("El objeto Scooter (el que se mueve).")]
    public Transform moto;
    [Tooltip("La cámara que se está usando ahora (la que va detrás de la moto).")]
    public GameObject camaraActual;
    [Tooltip("La tercera cámara, que empieza desactivada.")]
    public GameObject camaraNueva;
    [Tooltip("Distancia a la que se hace el cambio. Se ve como un círculo amarillo en la Scene.")]
    public float radio = 0.6f;

    bool hecho;

    void Start()
    {
        if (camaraNueva != null) camaraNueva.SetActive(false);
    }

    void Update()
    {
        // No hace nada hasta que la moto esté activa (aparece a los 12 s).
        if (hecho || moto == null || !moto.gameObject.activeInHierarchy) return;

        // Se compara solo en horizontal para ignorar la altura.
        Vector3 a = moto.position;
        Vector3 b = transform.position;
        a.y = 0f;
        b.y = 0f;

        if ((a - b).sqrMagnitude <= radio * radio)
        {
            hecho = true;
            // Primero se enciende la nueva y luego se apaga la vieja, para que nunca falte una cámara.
            if (camaraNueva != null) camaraNueva.SetActive(true);
            if (camaraActual != null) camaraActual.SetActive(false);
        }
    }

    void OnDrawGizmos()
    {
        Gizmos.color = new Color(1f, 0.8f, 0f, 0.9f);
        Gizmos.DrawWireSphere(transform.position, radio);
    }
}

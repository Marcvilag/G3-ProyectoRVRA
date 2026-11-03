using System.Collections;
using UnityEngine;
using UnityEngine.Events;

// Va en la raíz de la mosca, junto a su collider.
// Cuando la toca un collider con la etiqueta "Mano": suena el golpe, salen partículas,
// la mosca desaparece y, tras una pausa corta, se carga la pantalla de fin.
public class MoscaVida : MonoBehaviour
{
    [Header("Pantalla de fin")]
    public MapaEscenas mapa;
    public Destino destino;
    public float esperaAntesDeFin = 1f;   // segundos para que se vea el golpe

    [Header("Efectos")]
    public GameObject efectoPlaf;         // prefab de partículas (opcional)
    public float tamanoEfecto = 1f;       // súbelo o bájalo si las partículas quedan grandes o pequeñas
    public AudioSource sonidoPlaf;        // Audio Source con salida al grupo Efectos del mixer
    public float saltarInicio = 0.55f;   // segundos de silencio que se saltan al principio del sonido
    public UnityEvent alMorir;            // para enganchar otras cosas, como la puntuación

    bool muerta;

    void OnTriggerEnter(Collider otro)
    {
        if (muerta || !otro.CompareTag("Mano")) return;
        StartCoroutine(Morir());
    }

    IEnumerator Morir()
    {
        muerta = true;
        Debug.Log("¡PLAF! La mano ha aplastado a " + name);

        // Tamaño y centro de la mosca, antes de apagar sus colliders
        var col = GetComponent<Collider>();
        Vector3 centro = col ? col.bounds.center : transform.position;
        float radio = col ? col.bounds.extents.magnitude : 1f;

        if (efectoPlaf)
        {
            var efecto = Instantiate(efectoPlaf, centro, Quaternion.identity);
            efecto.transform.localScale = Vector3.one * radio * tamanoEfecto;
        }
        if (sonidoPlaf) { sonidoPlaf.time = saltarInicio; sonidoPlaf.Play(); }   // empieza directamente en el golpe

        // La mosca desaparece: se ocultan sus mallas y se apagan sus colliders
        foreach (var r in GetComponentsInChildren<Renderer>()) r.enabled = false;
        foreach (var c in GetComponentsInChildren<Collider>()) c.enabled = false;

        alMorir.Invoke();

        yield return new WaitForSeconds(esperaAntesDeFin);
        if (mapa) CargadorEscenas.Cargar(mapa.Nombre(destino));
    }
}
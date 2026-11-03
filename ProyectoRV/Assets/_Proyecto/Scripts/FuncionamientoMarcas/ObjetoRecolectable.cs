using UnityEngine;
using System.Collections;

public class ObjetoRecolectable : MonoBehaviour
{
    public int puntos = 50;

    [Header("Comportamiento")]
    public bool desapareceAlChocar = true;
    public float tiempoEspera = 3f;

    private Collider miCollider;
    private Renderer miRenderer;
    private bool bloqueado = false;

    // AHORA GUARDAMOS UN ARRAY DE COLORES, UNO POR CADA MATERIAL
    private Color[] coloresOriginales;

    void Start()
    {
        miCollider = GetComponent<Collider>();
        miRenderer = GetComponentInChildren<Renderer>();

        if (miRenderer != null)
        {
            // Inicializamos la lista de colores con la misma cantidad de materiales que tenga el objeto
            coloresOriginales = new Color[miRenderer.materials.Length];

            // Guardamos el color individual de cada material
            for (int i = 0; i < miRenderer.materials.Length; i++)
            {
                coloresOriginales[i] = miRenderer.materials[i].color;
            }
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        ProcesarChoque(collision.gameObject);
    }

    void OnTriggerEnter(Collider other)
    {
        ProcesarChoque(other.gameObject);
    }

    void ProcesarChoque(GameObject objetoChocado)
    {
        if (bloqueado) return;

        if (objetoChocado.CompareTag("Player"))
        {
            bloqueado = true;

            if (GestorPuntos.instancia != null)
                GestorPuntos.instancia.ModificarPuntos(puntos);

            if (puntos < 0)
            {
                EfectosMosca efectos = objetoChocado.GetComponent<EfectosMosca>();
                if (efectos != null) efectos.RecibirDano();
            }

            if (desapareceAlChocar)
                StartCoroutine(EfectoAparicionGradual());
            else
                StartCoroutine(SoloRecargarCooldown());
        }
    }

    IEnumerator EfectoAparicionGradual()
    {
        if (miCollider != null) miCollider.enabled = false;

        if (miRenderer != null)
        {
            // Ponemos a 0 la opacidad de CADA material usando su color original correspondiente
            for (int i = 0; i < miRenderer.materials.Length; i++)
            {
                Color colorTransparente = coloresOriginales[i];
                colorTransparente.a = 0f;
                miRenderer.materials[i].color = colorTransparente;
            }
        }

        float tiempoPasado = 0f;

        while (tiempoPasado < tiempoEspera)
        {
            tiempoPasado += Time.deltaTime;
            float porcentaje = tiempoPasado / tiempoEspera;

            if (miRenderer != null)
            {
                for (int i = 0; i < miRenderer.materials.Length; i++)
                {
                    Color colorGradual = coloresOriginales[i];
                    colorGradual.a = Mathf.Lerp(0f, 1f, porcentaje);
                    miRenderer.materials[i].color = colorGradual;
                }
            }

            yield return null;
        }

        if (miRenderer != null)
        {
            // Restauramos cada material a su color original exacto (blanco, azul, etc.)
            for (int i = 0; i < miRenderer.materials.Length; i++)
            {
                miRenderer.materials[i].color = coloresOriginales[i];
            }
        }

        if (miCollider != null) miCollider.enabled = true;
        bloqueado = false;
    }

    IEnumerator SoloRecargarCooldown()
    {
        yield return new WaitForSeconds(tiempoEspera);
        bloqueado = false;
    }
}
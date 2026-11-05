using UnityEngine;
using TMPro;

public class GestorPuntos : MonoBehaviour
{
    public static GestorPuntos instancia;

    [Header("Puntuación")]
    public int puntuacionTotal = 0;
    public TextMeshProUGUI textoPuntos;

    [Header("Tiempo de Supervivencia")]
    public float tiempoTranscurrido = 0f;
    public TextMeshProUGUI textoTiempo;
    public bool juegoActivo = false;

    [Header("Referencia Externa")]
    public GameObject moscaJugador; // Arrastra aquí la mosca de la jerarquía

    void Awake()
    {
        if (instancia == null)
        {
            instancia = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        juegoActivo = false;
        tiempoTranscurrido = 0f;
    }

    void Update()
    {
        // Si el juego está parado, comprobamos si tu compañero ha activado la mosca al pulsar Jugar
        if (!juegoActivo && moscaJugador != null && moscaJugador.activeSelf)
        {
            IniciarJuego();
        }

        if (!juegoActivo) return;

        // El contador de tiempo va sumando desde 0
        tiempoTranscurrido += Time.deltaTime;

        ActualizarInterfaz();
    }

    public void IniciarJuego()
    {
        juegoActivo = true;
    }

    public void ModificarPuntos(int cantidad)
    {
        puntuacionTotal += cantidad;
        Debug.Log("Puntuación actual: " + puntuacionTotal);
    }

    public void TerminarJuego()
    {
        juegoActivo = false;
        Debug.Log("¡Partida terminada! Tiempo de supervivencia: " + Mathf.Floor(tiempoTranscurrido) + " segundos.");
    }

    void ActualizarInterfaz()
    {
        if (textoPuntos != null)
        {
            textoPuntos.text = "Puntos: " + puntuacionTotal;
        }

        if (textoTiempo != null)
        {
            int minutos = Mathf.FloorToInt(tiempoTranscurrido / 60f);
            int segundos = Mathf.FloorToInt(tiempoTranscurrido % 60f);

            textoTiempo.text = "Tiempo: " + minutos.ToString("00") + ":" + segundos.ToString("00");
        }
    }
}
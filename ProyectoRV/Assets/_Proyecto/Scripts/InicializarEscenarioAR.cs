
using UnityEngine;
using Vuforia;

public class InicializarEscenarioAR : MonoBehaviour
{
    [Header("Referencias")]
    public ObserverBehaviour marcadorBrazo;
    public Transform centroEscenario;
    public GameObject moscaJugador;
    public GameObject canvasJuego;
    public GameObject canvasInicioAR;

    [Header("Aparicion de la mosca")]
    public Camera camaraAR;
    public float distanciaCamara = 0.6f;

    private bool escenarioInicializado = false;

    void Start()
    {
        moscaJugador.SetActive(false);
        canvasJuego.SetActive(false);
        canvasInicioAR.SetActive(true);

        marcadorBrazo.OnTargetStatusChanged += AlCambiarEstadoMarcador;

        AlCambiarEstadoMarcador(
            marcadorBrazo,
            marcadorBrazo.TargetStatus
        );
    }

    void AlCambiarEstadoMarcador(
        ObserverBehaviour observer,
        TargetStatus status)
    {
        if (escenarioInicializado)
            return;

        if (status.Status == Status.TRACKED)
        {
            // El marcador define el centro del escenario
            centroEscenario.position = marcadorBrazo.transform.position;
            centroEscenario.rotation = marcadorBrazo.transform.rotation;

            // La mosca aparece delante de la camara
            Transform camara = camaraAR.transform;

            moscaJugador.transform.position =
                camara.position +
                camara.forward * distanciaCamara;

            escenarioInicializado = true;

            moscaJugador.SetActive(true);
            canvasJuego.SetActive(true);
            canvasInicioAR.SetActive(false);

            Debug.Log("Escenario AR inicializado correctamente");
        }
    }

    void OnDestroy()
    {
        if (marcadorBrazo != null)
        {
            marcadorBrazo.OnTargetStatusChanged -= AlCambiarEstadoMarcador;
        }
    }
}


using UnityEngine;

public class IniciarPartida : MonoBehaviour
{
    [Header("Referencias")]
    public Camera camaraAR;
    public GameObject moscaJugador;
    public GameObject botonJugar;
    public GameObject joystickMovimiento;
    public GameObject joystickAltitud;
    public Transform centroEscenario;
    public GameObject botonDash;
    public GameObject botonReset;
    public GameObject planoShadowCatcher;

    [Header("Configuracion")]
    public float distanciaCamara = 0.6f;

    void Start()
    {
        moscaJugador.SetActive(false);
        joystickMovimiento.SetActive(false);
        joystickAltitud.SetActive(false);
        botonJugar.SetActive(true);
        botonDash.SetActive(false);
        planoShadowCatcher.SetActive(false);
        botonReset.SetActive(false);
    }

    public void Jugar()
    {
        // Colocar la mosca delante de la camara
        Vector3 posicionInicial =
            camaraAR.transform.position +
            camaraAR.transform.forward * distanciaCamara;

        // Establecer el centro del escenario
        centroEscenario.position = posicionInicial;
        centroEscenario.rotation = Quaternion.identity;

        // Colocar la mosca
        moscaJugador.transform.position = posicionInicial;

        // Colocar el Shadow Catcher debajo de la mosca
        Vector3 posicionPlano = posicionInicial;
        posicionPlano.y = centroEscenario.position.y - 0.1f;

        planoShadowCatcher.transform.position = posicionPlano;

        // Mostrar el juego
        moscaJugador.SetActive(true);
        joystickMovimiento.SetActive(true);
        joystickAltitud.SetActive(true);
        botonJugar.SetActive(false);
        botonDash.SetActive(true);
        botonReset.SetActive(true);
        planoShadowCatcher.SetActive(true);

        Debug.Log("Partida iniciada");
    }
}

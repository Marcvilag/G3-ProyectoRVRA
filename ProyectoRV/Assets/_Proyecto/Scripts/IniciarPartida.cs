using UnityEngine;

public class IniciarPartida : MonoBehaviour
{
    [Header("Referencias")]
    public Camera camaraAR;
    public GameObject moscaJugador;
    public GameObject botonJugar;
    public Transform centroEscenario;
    public GameObject botonDash;
    public GameObject botonReset;
    public GameObject planoShadowCatcher;

    [Header("Configuracion")]
    public float distanciaCamara = 0.6f;

    void Start()
    {
        moscaJugador.SetActive(false);
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

        // Mostrar el juego (al activarse, la mosca calcula su plano de control)
        moscaJugador.SetActive(true);
        botonJugar.SetActive(false);
        botonDash.SetActive(true);
        botonReset.SetActive(true);

        // La mosca ya esta activa y colocada (su script fija su altura): el Shadow Catcher va debajo
        Vector3 posicionPlano = moscaJugador.transform.position;
        posicionPlano.y -= 0.1f;
        planoShadowCatcher.transform.position = posicionPlano;
        planoShadowCatcher.SetActive(true);

        Debug.Log("Partida iniciada");
    }
}
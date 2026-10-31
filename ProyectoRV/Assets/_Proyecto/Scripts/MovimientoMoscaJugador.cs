
using UnityEngine;

public class MovimientoMoscaJugador : MonoBehaviour
{
    [Header("Joysticks")]
    public JoystickMovimientoUI joystickMovimiento;
    public JoystickAltitudUI joystickAltitud;

    [Header("Movimiento")]
    public float velocidad = 1f;
    public float velocidadVertical = 0.7f;
    public float velocidadGiro = 8f;

    [Header("Limites del escenario")]
    public Transform centroEscenario;
    public float radioMaximo = 2f;
    public float alturaMinima = 0.1f;
    public float alturaMaxima = 2f;

    [Header("Dash")]
    public float velocidadDash = 5f;
    public float duracionDash = 0.2f;
    public float recargaDash = 1.5f;

    private float tiempoDash = 0f;
    private float tiempoRecarga = 0f;
    private Vector3 direccionDash;

    void Update()
    {
        if (joystickMovimiento == null ||
            joystickAltitud == null ||
            centroEscenario == null)
            return;

        Vector2 direccion = joystickMovimiento.Direccion;
        float vertical = joystickAltitud.Vertical;

        // Movimiento horizontal
        Vector3 movimiento = new Vector3(
            direccion.x,
            0f,
            direccion.y
        );

        // Movimiento vertical y horizontal
        Vector3 desplazamiento =
            movimiento * velocidad +
            Vector3.up * vertical * velocidadVertical;

        // Aplicar el impulso del DASH
        if (tiempoDash > 0f)
        {
            desplazamiento += direccionDash * velocidadDash;
            tiempoDash -= Time.deltaTime;
        }

        // Actualizar recarga del DASH
        if (tiempoRecarga > 0f)
        {
            tiempoRecarga -= Time.deltaTime;
        }

        Vector3 nuevaPosicion =
            transform.position + desplazamiento * Time.deltaTime;

        // Limitar altura
        float suelo = centroEscenario.position.y + alturaMinima;
        float techo = centroEscenario.position.y + alturaMaxima;

        nuevaPosicion.y = Mathf.Clamp(
            nuevaPosicion.y, suelo, techo
        );

        // Limitar distancia horizontal
        Vector3 centro = centroEscenario.position;

        Vector2 distancia = new Vector2(
            nuevaPosicion.x - centro.x,
            nuevaPosicion.z - centro.z
        );

        distancia = Vector2.ClampMagnitude(
            distancia, radioMaximo
        );

        nuevaPosicion.x = centro.x + distancia.x;
        nuevaPosicion.z = centro.z + distancia.y;

        transform.position = nuevaPosicion;

        // Girar hacia la direccion de vuelo
        if (movimiento.sqrMagnitude > 0.01f)
        {
            Quaternion rotacionObjetivo =
                Quaternion.LookRotation(movimiento, Vector3.up);

            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                rotacionObjetivo,
                velocidadGiro * Time.deltaTime
            );
        }
    }

    // Este metodo lo llama el boton DASH
    public void ActivarDash()
    {
        if (tiempoRecarga > 0f || tiempoDash > 0f)
            return;

        direccionDash = transform.forward;
        direccionDash.y = 0f;
        direccionDash.Normalize();

        tiempoDash = duracionDash;
        tiempoRecarga = recargaDash;

        Debug.Log("DASH activado");
    }

    // Este metodo lo llama el boton RESET
    public void ResetearPosicion()
    {
        if (centroEscenario == null)
            return;

        // Cancelar el DASH
        tiempoDash = 0f;

        // Volver a la posicion inicial del escenario
        Vector3 posicionReset = centroEscenario.position;

        transform.position = posicionReset;
        transform.rotation = Quaternion.identity;

        Debug.Log("Posicion de la mosca restablecida");
    }
}

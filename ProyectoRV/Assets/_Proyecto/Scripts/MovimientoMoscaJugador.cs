using UnityEngine;

/// <summary>
/// Control de la mosca moviendo el movil, solo en el plano X-Z.
/// Desde el centro de la camara AR sale un raycast que choca con un Plane (3D Object)
/// invisible (sin Mesh Renderer, con Mesh Collider). La mosca persigue el punto de impacto.
///
/// El plano se coloca a una altura fija POR DEBAJO de la camara (alturaBajoCamara) al activarse
/// la mosca y al pulsar RESET. La mosca vuela en ese mismo plano, y hay que apuntar el movil
/// hacia abajo (hacia la mesa) para que el rayo lo toque.
/// </summary>
public class MovimientoMoscaJugador : MonoBehaviour
{
    [Header("Control por camara")]
    public Camera camaraAR;
    [Tooltip("3D Object > Plane SIN Mesh Renderer, con Mesh Collider (no Convex, no Trigger).")]
    public Collider planoControl;
    public float distanciaMaximaRayo = 20f;
    [Tooltip("A cuantos metros por debajo de la camara esta el plano (y la mosca) al empezar.")]
    public float alturaBajoCamara = 0.4f;
    [Tooltip("Distancia horizontal delante de la camara a la que aparece la mosca.")]
    public float distanciaInicial = 0.6f;
    [Tooltip("Si apuntas mas horizontal que este angulo (grados bajo el horizonte) se ignora el rayo. Evita saltos enormes.")]
    public float anguloMinimoRayo = 20f;
    public bool empezarMirandoAlJugador = true;

    [Header("Punto de mira")]
    [Tooltip("Muestra una bolita en el punto al que apuntas. Si no asignas objeto, se crea una automaticamente.")]
    public bool mostrarMarcador = true;
    public Transform marcadorObjetivo;
    public float tamanoMarcador = 0.03f;

    [Header("Movimiento")]
    [Tooltip("Velocidad maxima de la mosca (m/s)")]
    public float velocidad = 0.8f;
    [Tooltip("Cuanto mas bajo, mas tarda en alcanzar el punto (mas sensacion de persecucion). Valores 1.5-4.")]
    public float respuesta = 2.5f;
    public float velocidadGiro = 8f;

    [Header("Limites del escenario")]
    public Transform centroEscenario;
    public float radioMaximo = 2f;

    [Header("Dash")]
    public float velocidadDash = 5f;
    public float duracionDash = 0.2f;
    public float recargaDash = 1.5f;

    private float tiempoDash = 0f;
    private float tiempoRecarga = 0f;
    private Vector3 direccionDash;

    private float alturaVuelo;
    private Vector3 objetivo;
    private bool marcadorCreado = false;

    void OnEnable()
    {
        if (camaraAR == null)
            camaraAR = Camera.main;

        IniciarControl(false);
        PrepararMarcador();
    }

    void OnDisable()
    {
        if (marcadorObjetivo != null)
            marcadorObjetivo.gameObject.SetActive(false);
    }

    void OnDestroy()
    {
        if (marcadorCreado && marcadorObjetivo != null)
            Destroy(marcadorObjetivo.gameObject);
    }

    void Update()
    {
        if (camaraAR == null || centroEscenario == null || planoControl == null)
            return;

        ActualizarObjetivo();

        // Persecucion en el plano X-Z: velocidad proporcional a la distancia, limitada a "velocidad"
        Vector3 haciaObjetivo = objetivo - transform.position;
        haciaObjetivo.y = 0f;
        Vector3 desplazamiento = Vector3.ClampMagnitude(haciaObjetivo * respuesta, velocidad);

        // Aplicar el impulso del DASH
        if (tiempoDash > 0f)
        {
            desplazamiento += direccionDash * velocidadDash;
            tiempoDash -= Time.deltaTime;
        }

        // Actualizar recarga del DASH
        if (tiempoRecarga > 0f)
            tiempoRecarga -= Time.deltaTime;

        transform.position = LimitarAlEscenario(
            transform.position + desplazamiento * Time.deltaTime
        );

        // Girar hacia donde vuela
        if (haciaObjetivo.sqrMagnitude > 0.0025f)
        {
            Quaternion rotacionObjetivo = Quaternion.LookRotation(haciaObjetivo, Vector3.up);
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                rotacionObjetivo,
                velocidadGiro * Time.deltaTime
            );
        }
    }

    // Raycast desde el centro de la camara contra el collider del plano
    void ActualizarObjetivo()
    {
        Ray rayo = new Ray(
            camaraAR.transform.position,
            camaraAR.transform.forward
        );

        // Intentar encontrar el punto donde miramos
        if (planoControl.Raycast(rayo, out RaycastHit impacto, distanciaMaximaRayo))
        {
            objetivo = LimitarAlEscenario(impacto.point);
        }
        else
        {
            // Si el rayo no toca el plano, proyectamos
            // la dirección de la cámara sobre el plano horizontal
            Vector3 direccion = camaraAR.transform.forward;
            direccion.y = 0f;

            if (direccion.sqrMagnitude > 0.001f)
            {
                direccion.Normalize();

                Vector3 nuevoObjetivo =
                    camaraAR.transform.position +
                    direccion * distanciaInicial;

                objetivo = LimitarAlEscenario(nuevoObjetivo);
            }
        }

        if (marcadorObjetivo != null)
            marcadorObjetivo.position = objetivo;
    }

    // Fija la altura de vuelo, coloca el plano y (al empezar) la mosca delante de la camara
    void IniciarControl(bool enCentro)
    {
        if (camaraAR == null)
            return;

        alturaVuelo = camaraAR.transform.position.y - alturaBajoCamara;

        // Direccion horizontal en la que mira la camara
        Vector3 adelante = camaraAR.transform.forward;
        adelante.y = 0f;
        if (adelante.sqrMagnitude < 0.001f)
        {
            adelante = camaraAR.transform.up; // camara mirando casi recta hacia abajo
            adelante.y = 0f;
        }
        adelante.Normalize();

        Vector3 inicio = (enCentro && centroEscenario != null)
            ? centroEscenario.position
            : camaraAR.transform.position + adelante * distanciaInicial;
        inicio.y = alturaVuelo;

        transform.position = LimitarAlEscenario(inicio);

        if (empezarMirandoAlJugador)
            transform.rotation = Quaternion.LookRotation(-adelante, Vector3.up);

        objetivo = transform.position;
        ColocarPlano();
    }

    void ColocarPlano()
    {
        if (planoControl == null || camaraAR == null)
            return;

        // El Mesh Collider de un Plane solo detecta impactos por su cara superior.
        // Si la camara quedara por debajo del plano, se le da la vuelta.
        bool camaraPorEncima = camaraAR.transform.position.y > alturaVuelo;

        planoControl.transform.SetPositionAndRotation(
            new Vector3(camaraAR.transform.position.x, alturaVuelo, camaraAR.transform.position.z),
            camaraPorEncima ? Quaternion.identity : Quaternion.Euler(180f, 0f, 0f)
        );
    }

    void PrepararMarcador()
    {
        if (!mostrarMarcador)
        {
            if (marcadorObjetivo != null)
                marcadorObjetivo.gameObject.SetActive(false);
            return;
        }

        if (marcadorObjetivo == null)
        {
            GameObject esfera = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            esfera.name = "MarcadorObjetivo (auto)";
            Destroy(esfera.GetComponent<Collider>());
            esfera.transform.localScale = Vector3.one * tamanoMarcador;
            esfera.GetComponent<Renderer>().material.color = Color.red;
            marcadorObjetivo = esfera.transform;
            marcadorCreado = true;
        }

        marcadorObjetivo.position = objetivo;
        marcadorObjetivo.gameObject.SetActive(true);
    }

    Vector3 LimitarAlEscenario(Vector3 posicion)
    {
        posicion.y = alturaVuelo;

        if (centroEscenario == null)
            return posicion;

        Vector3 centro = centroEscenario.position;
        Vector2 distancia = new Vector2(posicion.x - centro.x, posicion.z - centro.z);
        distancia = Vector2.ClampMagnitude(distancia, radioMaximo);
        posicion.x = centro.x + distancia.x;
        posicion.z = centro.z + distancia.y;

        return posicion;
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

    // Este metodo lo llama el boton RESET: la mosca vuelve al centro del escenario
    // y el plano se recalibra segun la altura actual de la camara.
    public void ResetearPosicion()
    {
        if (centroEscenario == null)
            return;

        // Cancelar el DASH
        tiempoDash = 0f;

        // Volver al centro manteniendo la altura de vuelo
        Vector3 posicionInicial = centroEscenario.position;
        posicionInicial.y = alturaVuelo;

        transform.position = posicionInicial;

        // Evitar que siga persiguiendo el objetivo anterior
        objetivo = posicionInicial;

        Debug.Log("Posicion de la mosca restablecida");
    }   
}
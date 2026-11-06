
using UnityEngine;
using Vuforia;

public class MovimientoMoscaJugador : MonoBehaviour
{
    [Header("Control por camara")]
    public Camera camaraAR;
    public Collider planoControl;
    public float distanciaMaximaRayo = 20f;
    public float distanciaInicial = 0.6f;
    public float anguloMinimoRayo = 20f;
    public bool empezarMirandoAlJugador = true;

    [Header("Altura respecto a la mesa")]
    [Tooltip("Marcas Vuforia colocadas sobre la misma mesa.")]
    public ObserverBehaviour[] marcasMesa;

    [Tooltip("Altura de vuelo sobre las marcas, en metros.")]
    public float alturaSobreMesa = 0.03f;

    [Header("Punto de mira")]
    public bool mostrarMarcador = true;
    public Transform marcadorObjetivo;
    public float tamanoMarcador = 0.03f;

    [Header("Movimiento")]
    public float velocidad = 0.8f;
    public float respuesta = 2.5f;
    public float velocidadGiro = 8f;

    [Header("Limites del escenario")]
    public Transform centroEscenario;
    public float radioMaximo = 2f;

    [Header("Dash")]
    public float velocidadDash = 5f;
    public float duracionDash = 0.2f;
    public float recargaDash = 1.5f;

    private float tiempoDash;
    private float tiempoRecarga;
    private Vector3 direccionDash;

    private float alturaVuelo;
    private Vector3 objetivo;
    private bool marcadorCreado;
    private bool alturaCalibrada;

    void OnEnable()
    {
        if (camaraAR == null)
            camaraAR = Camera.main;

        alturaCalibrada = false;
        tiempoDash = 0f;
        tiempoRecarga = 0f;

        PrepararMarcador();

        if (marcadorObjetivo != null)
            marcadorObjetivo.gameObject.SetActive(false);

        IntentarCalibrarAltura(false);
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

        // No mover la mosca hasta detectar una marca valida.
        if (!alturaCalibrada)
        {
            IntentarCalibrarAltura(false);
            return;
        }

        ActualizarObjetivo();

        Vector3 haciaObjetivo = objetivo - transform.position;
        haciaObjetivo.y = 0f;

        Vector3 desplazamiento =
            Vector3.ClampMagnitude(haciaObjetivo * respuesta, velocidad);

        if (tiempoDash > 0f)
        {
            desplazamiento += direccionDash * velocidadDash;
            tiempoDash -= Time.deltaTime;
        }

        if (tiempoRecarga > 0f)
            tiempoRecarga -= Time.deltaTime;

        transform.position = LimitarAlEscenario(
            transform.position + desplazamiento * Time.deltaTime
        );

        if (haciaObjetivo.sqrMagnitude > 0.0025f)
        {
            Quaternion rotacionObjetivo =
                Quaternion.LookRotation(haciaObjetivo, Vector3.up);

            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                rotacionObjetivo,
                velocidadGiro * Time.deltaTime
            );
        }
    }

    bool BuscarAlturaMesa(out float alturaMesa)
    {
        alturaMesa = 0f;

        if (marcasMesa == null)
            return false;

        foreach (ObserverBehaviour marca in marcasMesa)
        {
            if (marca == null || !marca.isActiveAndEnabled)
                continue;

            var estado = marca.TargetStatus.Status;

            // Utilizamos una marca realmente detectada.
            if (estado == Status.TRACKED ||
                estado == Status.EXTENDED_TRACKED)
            {
                alturaMesa = marca.transform.position.y;
                return true;
            }
        }

        return false;
    }

    bool IntentarCalibrarAltura(bool enCentro)
    {
        if (camaraAR == null)
            return false;

        if (!BuscarAlturaMesa(out float alturaMesa))
            return false;

        alturaVuelo = alturaMesa + alturaSobreMesa;
        alturaCalibrada = true;

        IniciarControl(enCentro);

        if (marcadorObjetivo != null && mostrarMarcador)
            marcadorObjetivo.gameObject.SetActive(true);

        Debug.Log(
            "Altura calibrada con Vuforia. Mesa: " +
            alturaMesa.ToString("F3") +
            " | Vuelo: " + alturaVuelo.ToString("F3")
        );

        return true;
    }

    void ActualizarObjetivo()
    {
        Ray rayo = new Ray(
            camaraAR.transform.position,
            camaraAR.transform.forward
        );

        Vector3 direccionCamara = camaraAR.transform.forward;

        // Evita objetivos demasiado lejanos al mirar al horizonte.
        float limite = -Mathf.Sin(
            anguloMinimoRayo * Mathf.Deg2Rad
        );

        RaycastHit impacto = default(RaycastHit);

        bool impactoValido = false;

        if (direccionCamara.y < limite)
        {
            impactoValido = planoControl.Raycast(
                rayo,
                out impacto,
                distanciaMaximaRayo
            );
}

        if (impactoValido)
        {
            objetivo = LimitarAlEscenario(impacto.point);
        }
        else
        {
            // Conservamos el comportamiento alternativo original.
            Vector3 direccion = direccionCamara;
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

    void IniciarControl(bool enCentro)
    {
        Vector3 adelante = camaraAR.transform.forward;
        adelante.y = 0f;

        if (adelante.sqrMagnitude < 0.001f)
        {
            adelante = camaraAR.transform.up;
            adelante.y = 0f;
        }

        if (adelante.sqrMagnitude < 0.001f)
            adelante = Vector3.forward;

        adelante.Normalize();

        Vector3 inicio = (enCentro && centroEscenario != null)
            ? centroEscenario.position
            : camaraAR.transform.position +
              adelante * distanciaInicial;

        inicio.y = alturaVuelo;

        transform.position = LimitarAlEscenario(inicio);

        if (empezarMirandoAlJugador)
        {
            transform.rotation =
                Quaternion.LookRotation(-adelante, Vector3.up);
        }

        objetivo = transform.position;

        ColocarPlano();
    }

    void ColocarPlano()
    {
        if (planoControl == null || camaraAR == null)
            return;

        bool camaraPorEncima =
            camaraAR.transform.position.y > alturaVuelo;

        planoControl.transform.SetPositionAndRotation(
            new Vector3(
                camaraAR.transform.position.x,
                alturaVuelo,
                camaraAR.transform.position.z
            ),
            camaraPorEncima
                ? Quaternion.identity
                : Quaternion.Euler(180f, 0f, 0f)
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
            GameObject esfera =
                GameObject.CreatePrimitive(PrimitiveType.Sphere);

            esfera.name = "MarcadorObjetivo (auto)";

            Destroy(esfera.GetComponent<Collider>());

            esfera.transform.localScale =
                Vector3.one * tamanoMarcador;

            esfera.GetComponent<Renderer>().material.color =
                Color.red;

            marcadorObjetivo = esfera.transform;
            marcadorCreado = true;
        }

        marcadorObjetivo.position = objetivo;
    }

    Vector3 LimitarAlEscenario(Vector3 posicion)
    {
        posicion.y = alturaVuelo;

        if (centroEscenario == null)
            return posicion;

        Vector3 centro = centroEscenario.position;

        Vector2 distancia = new Vector2(
            posicion.x - centro.x,
            posicion.z - centro.z
        );

        distancia = Vector2.ClampMagnitude(
            distancia,
            radioMaximo
        );

        posicion.x = centro.x + distancia.x;
        posicion.z = centro.z + distancia.y;

        return posicion;
    }

    public void ActivarDash()
    {
        if (!alturaCalibrada ||
            tiempoRecarga > 0f ||
            tiempoDash > 0f)
            return;

        direccionDash = transform.forward;
        direccionDash.y = 0f;

        if (direccionDash.sqrMagnitude < 0.001f)
            return;

        direccionDash.Normalize();

        tiempoDash = duracionDash;
        tiempoRecarga = recargaDash;

        Debug.Log("DASH activado");
    }

    public void ResetearPosicion()
    {
        if (centroEscenario == null)
            return;

        tiempoDash = 0f;

        // Recalibramos si hay alguna marca visible.
        if (IntentarCalibrarAltura(true))
        {
            Debug.Log("Mosca reseteada y altura recalibrada");
            return;
        }

        // Si se perdieron las marcas, conservamos la
        // ultima altura valida.
        if (!alturaCalibrada)
            return;

        Vector3 posicionInicial = centroEscenario.position;
        posicionInicial.y = alturaVuelo;

        transform.position = LimitarAlEscenario(posicionInicial);
        objetivo = transform.position;

        Debug.Log("Mosca reseteada a la altura anterior");
    }
}

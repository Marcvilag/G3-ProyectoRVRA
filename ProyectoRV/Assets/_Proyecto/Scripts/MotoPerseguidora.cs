using UnityEngine;
using UnityEngine.Splines;

// Añádelo al objeto "Scooter" (el padre que contiene la moto y a YayaMoto).
// La moto se queda quieta mientras dura la animación y después avanza por su propia spline
// (Ruta Moto). Si no pones ninguna, usa la de las moscas.
public class MotoPerseguidora : MonoBehaviour
{
    [Header("Recorrido")]
    [Tooltip("Spline propia de la moto (un SplineContainer nuevo). Si se deja vacía se usa la de las moscas.")]
    public SplineContainer rutaMoto;
    [Tooltip("Solo hace falta si no usas Ruta Moto.")]
    public FlySwarm enjambre;

    [Header("Espera (la moto no se mueve mientras dura la animación)")]
    [Tooltip("Opcional: arrastra aquí el clip Moto y la espera será exactamente su duración.")]
    public AnimationClip clipMoto;
    [Tooltip("Se usa solo si no has puesto el clip. El clip Moto dura 10,83 s.")]
    public float esperaManual = 10.83f;
    [Tooltip("Segundos de arranque suave al terminar la espera.")]
    public float arranque = 1.5f;

    [Header("Avance por la spline (valores de 0 a 1)")]
    [Tooltip("Dónde empieza. -1 = automático: 0 si usas Ruta Moto, o el punto más cercano a la moto si usas la de las moscas.")]
    public float progresoInicial = -1f;
    [Tooltip("Dónde termina. Con Ruta Moto pon 1 para llegar al final de tu spline.")]
    public float progresoFinal = 1f;
    [Tooltip("Segundos que tarda en ir del inicio al final.")]
    public float duracionRecorrido = 7f;
    [Tooltip("Cómo acelera: horizontal es el tiempo, vertical el avance.")]
    public AnimationCurve curvaAvance = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    [Header("Posición y giro")]
    [Tooltip("Si está marcado, la moto mantiene la altura a la que la colocaste (va pegada al suelo).")]
    public bool mantenerAltura = true;
    [Tooltip("Desplazamiento respecto a la spline (en Y solo cuenta si no mantienes la altura).")]
    public Vector3 desplazamiento = Vector3.zero;
    [Tooltip("Corrige hacia dónde mira el modelo (por ejemplo 0, 90, 0).")]
    public Vector3 rotacionModelo = Vector3.zero;
    public float suavizadoGiro = 8f;

    [Header("Ruedas (opcional)")]
    public Transform[] ruedas;
    public Vector3 ejeRueda = Vector3.right;
    public float radioRueda = 0.3f;

    float t;        // segundos desde que se activó la moto
    float reloj;    // avance de la moto: solo cuenta cuando ya se mueve
    float p0;       // punto inicial en la spline
    Vector3 posReposo;
    Vector3 ultimaPos;
    bool iniciada;

    float Espera
    {
        get { return clipMoto != null ? clipMoto.length : esperaManual; }
    }

    bool HayRuta
    {
        get { return rutaMoto != null || (enjambre != null && enjambre.path != null); }
    }

    // Evalúa la spline que toque (la de la moto o la de las moscas), en coordenadas de mundo.
    void Evaluar(float s, out Unity.Mathematics.float3 pos, out Unity.Mathematics.float3 tangente)
    {
        if (rutaMoto != null) rutaMoto.Evaluate(s, out pos, out tangente, out _);
        else enjambre.path.Evaluate(s, out pos, out tangente, out _);
    }

    void OnEnable()
    {
        t = 0f;
        reloj = 0f;
        iniciada = false;
    }

    void Update()
    {
        if (!HayRuta) return;

        t += Time.deltaTime;

        if (!iniciada)
        {
            posReposo = transform.position;   // donde la hayas colocado en la escena
            ultimaPos = posReposo;
            if (progresoInicial >= 0f) p0 = progresoInicial;
            else p0 = rutaMoto != null ? 0f : BuscarMasCercano(posReposo);
            iniciada = true;
        }

        float espera = Espera;
        if (t < espera) return;               // quieta mientras dura la animación

        float k = arranque > 0.001f ? Mathf.SmoothStep(0f, 1f, (t - espera) / arranque) : 1f;
        reloj += Time.deltaTime * k;

        float u = duracionRecorrido > 0.001f ? Mathf.Clamp01(reloj / duracionRecorrido) : 1f;
        float p = Mathf.Lerp(p0, progresoFinal, curvaAvance.Evaluate(u));

        Evaluar(Mathf.Clamp01(p), out Unity.Mathematics.float3 pos, out Unity.Mathematics.float3 tangente);

        Vector3 objetivo = mantenerAltura
            ? new Vector3(pos.x + desplazamiento.x, posReposo.y, pos.z + desplazamiento.z)
            : (Vector3)pos + desplazamiento;
        Vector3 destino = Vector3.Lerp(posReposo, objetivo, k);

        // Dirección de avance en horizontal; si recorre la spline hacia atrás, se invierte.
        float signo = progresoFinal >= p0 ? 1f : -1f;
        Vector3 dir = new Vector3(tangente.x, 0f, tangente.z) * signo;
        if (dir.sqrMagnitude > 0.0001f)
        {
            Quaternion rot = Quaternion.LookRotation(dir.normalized, Vector3.up) * Quaternion.Euler(rotacionModelo);
            transform.rotation = Quaternion.Slerp(transform.rotation, rot, suavizadoGiro * Time.deltaTime);
        }

        transform.position = destino;

        float distancia = (destino - ultimaPos).magnitude;
        if (ruedas != null && radioRueda > 0.001f && distancia > 0f)
        {
            float angulo = distancia / radioRueda * Mathf.Rad2Deg;
            foreach (Transform rueda in ruedas)
                if (rueda != null) rueda.Rotate(ejeRueda, angulo, Space.Self);
        }

        ultimaPos = destino;
    }

    // Punto de la spline (0 a 1) más cercano a "punto", comparando solo en horizontal.
    float BuscarMasCercano(Vector3 punto)
    {
        const int muestras = 400;
        float mejorDist = float.MaxValue;
        float mejorT = 0f;
        for (int i = 0; i <= muestras; i++)
        {
            float s = i / (float)muestras;
            Evaluar(s, out Unity.Mathematics.float3 pos, out _);
            float dx = pos.x - punto.x;
            float dz = pos.z - punto.z;
            float d = dx * dx + dz * dz;
            if (d < mejorDist) { mejorDist = d; mejorT = s; }
        }
        return mejorT;
    }
}

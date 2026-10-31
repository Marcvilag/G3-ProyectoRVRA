using UnityEngine;
using TMPro;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

public class HintIcon : MonoBehaviour
{
    [Header("Aparición")]
    public bool alwaysVisible = false;
    public float idleSeconds = 4f;
    public float fadeSpeed = 2f;

    [Header("Flotar ")]
    public float bobHeight = 0.08f;
    public float bobSpeed = 2f;

    [Header("Giro suave")]
    public float rollAngle = 8f;      // balanceo tipo péndulo (eje Z)
    public float yawAngle = 10f;      // giro lateral (eje Y)
    public float swaySpeed = 1.5f;

    [Header("Latido")]
    public float pulseAmount = 0.08f; // 0.08 = crece y encoge un 8 %
    public float pulseSpeed = 3f;

    [Header("Color del icono")]
    public Gradient colorCycle;       // si está vacío, usa uno por defecto
    public float colorSpeed = 0.3f;   // ciclos por segundo

    [Header("Etiqueta de texto")]
    public TMP_Text label;            // si está vacío, busca un TMP_Text hijo
    public Color labelColor = Color.white;
    public bool keepLabelUpright = true;   // el texto no se inclina con el icono

    SpriteRenderer sr;
    Vector3 basePos, baseScale;
    float lastTouch, alpha, phase;
    Camera cam;

    void Reset()
    {
        colorCycle = DefaultGradient();
    }

    void Awake()
    {
        sr = GetComponentInChildren<SpriteRenderer>();
        if (!label) label = GetComponentInChildren<TMP_Text>();

        basePos = transform.localPosition;
        baseScale = transform.localScale;
        cam = Camera.main;

        phase = Random.value * 10f;   // cada icono va desfasado
        alpha = alwaysVisible ? 1f : 0f;

        if (colorCycle == null || colorCycle.colorKeys.Length == 0)
            colorCycle = DefaultGradient();
    }

    static Gradient DefaultGradient()
    {
        var g = new Gradient { mode = GradientMode.Blend };
        g.SetKeys(
            new[] {
                new GradientColorKey(Color.white, 0f),
                new GradientColorKey(new Color(1f, 0.85f, 0.3f), 0.5f),
                new GradientColorKey(Color.white, 1f)
            },
            new[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(1f, 1f) });
        return g;
    }

    bool Touching()
    {
#if ENABLE_INPUT_SYSTEM
        if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.isPressed) return true;
        if (Mouse.current != null && Mouse.current.leftButton.isPressed) return true;
        return false;
#else
        return Input.touchCount > 0 || Input.GetMouseButton(0);
#endif
    }

    void Update()
    {
        if (!cam) { cam = Camera.main; if (!cam) return; }

        float t = Time.time + phase;

        // 1. Visibilidad: aparece tras unos segundos sin tocar la pantalla
        if (Touching()) lastTouch = Time.time;
        float target = (alwaysVisible || Time.time - lastTouch > idleSeconds) ? 1f : 0f;
        alpha = Mathf.MoveTowards(alpha, target, Time.deltaTime * fadeSpeed);

        // 2. Posición: sube y baja en metros reales, sin depender de la escala o rotación del padre
        Vector3 worldOffset = Vector3.up * Mathf.Sin(t * bobSpeed) * bobHeight;
        transform.localPosition = basePos + (transform.parent
            ? transform.parent.InverseTransformVector(worldOffset)
            : worldOffset);

        // 3. Rotación: mira a cámara + balanceo + giro lateral
        float roll = Mathf.Sin(t * swaySpeed) * rollAngle;
        float yaw = Mathf.Sin(t * swaySpeed * 0.7f + 1.3f) * yawAngle;
        transform.rotation = cam.transform.rotation * Quaternion.Euler(0f, yaw, roll);

        // 4. Latido
        float pulse = 1f + Mathf.Sin(t * pulseSpeed) * pulseAmount;
        transform.localScale = baseScale * pulse;

        // 5. Color del icono, con el alfa de la aparición
        if (sr)
        {
            Color c = colorCycle.Evaluate(Mathf.Repeat(t * colorSpeed, 1f));
            c.a = alpha;
            sr.color = c;
        }

        // 6. Etiqueta: mismo alfa y, si se quiere, siempre derecha
        if (label)
        {
            Color lc = labelColor;
            lc.a = alpha;
            label.color = lc;

            if (keepLabelUpright)
                label.transform.rotation = cam.transform.rotation;
        }
    }
}
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit.Inputs.Haptics;
using UnityEngine.XR.Interaction.Toolkit.UI;

/// <summary>
/// Reacciones de un botón de UI en VR (XRI 3.x):
///  - Hover:    outline dorado (fade), escala 1.05, inclinación + vaivén suave, tinte cálido.
///  - Pressed:  escala 0.92 (3 ejes), se hunde en Z, tinte oscuro.
///  - Released: rebote elástico (OutBack).
///  - Exit:     vuelta a reposo (si se estaba pulsando, se cancela visualmente).
///
/// Este script SOLO se ocupa del feedback. La acción real va en Button.onClick,
/// que Unity ya dispara únicamente si pulsas y sueltas sobre el mismo botón.
///
/// Requisitos: Button + Image en el mismo GameObject. El Outline se añade solo si falta.
/// </summary>
[DisallowMultipleComponent]
[RequireComponent(typeof(Button))]
public class ButtonVRReaction : MonoBehaviour,
    IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{
    [Header("Referencias (se autocompletan)")]
    [SerializeField] private Image targetImage;
    [SerializeField] private Outline outline;
    [SerializeField] private TMP_Text label;

    [Header("Outline (hover)")]
    [SerializeField] private Color outlineColor = new Color(1f, 0.78f, 0.24f, 1f); // #FFC83D
    [SerializeField] private Vector2 outlineDistance = new Vector2(4f, -4f);

    [Header("Escala")]
    [SerializeField] private float hoverScale = 1.05f;
    [SerializeField] private float pressedScale = 0.92f;

    [Header("Movimiento")]
    [Tooltip("Inclinación en Z (grados) al hacer hover.")]
    [SerializeField] private float hoverTilt = 1.5f;
    [Tooltip("Amplitud del vaivén (grados) mientras dura el hover.")]
    [SerializeField] private float swayAmplitude = 0.8f;
    [SerializeField] private float swaySpeed = 2f;
    [Tooltip("Hundimiento en Z al pulsar, en unidades locales del Canvas (con escala 0.001, 10 = 1 cm).")]
    [SerializeField] private float pressedDepth = 10f;

    [Header("Colores")]
    [SerializeField] private Color hoverTint = new Color(1f, 0.96f, 0.85f, 1f);   // crema
    [SerializeField] private Color pressedTint = new Color(0.85f, 0.85f, 0.85f, 1f);
    [SerializeField] private Color labelHoverColor = new Color(1f, 0.9f, 0.6f, 1f);

    [Header("Duraciones (s)")]
    [SerializeField] private float hoverDuration = 0.15f;
    [SerializeField] private float pressDuration = 0.08f;
    [SerializeField] private float releaseDuration = 0.2f;

    [Header("Háptica (opcional)")]
    [SerializeField] private bool useHaptics = true;
    [SerializeField] private Vector2 hoverHaptic = new Vector2(0.1f, 0.05f);   // (amplitud, duración)
    [SerializeField] private Vector2 pressHaptic = new Vector2(0.4f, 0.1f);
    [SerializeField] private Vector2 releaseHaptic = new Vector2(0.2f, 0.06f);

    [Header("Sonido (opcional: deja vacío lo que no quieras)")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip hoverClip;
    [SerializeField] private AudioClip pressClip;
    [SerializeField] private AudioClip releaseClip;
    [SerializeField, Range(0f, 0.2f)] private float pitchVariation = 0.05f;

    private Button button;
    private RectTransform rt;
    private Vector3 baseScale;
    private float baseZ;   // Solo la Z: X/Y los controla el Layout Group
    private Quaternion baseRotation;
    private Color baseImageColor, baseLabelColor;

    // 0 = reposo, 1 = estado completo. Pueden pasarse de 1 (overshoot) o bajar de 0.
    private float hover, press;
    private bool isHovered, isPressed;
    private Coroutine hoverRoutine, pressRoutine;

    private void Reset() => AutoAssign();

    private void Awake()
    {
        button = GetComponent<Button>();
        rt = (RectTransform)transform;
        AutoAssign();

        if (outline == null && targetImage != null)
            outline = targetImage.gameObject.AddComponent<Outline>();

        if (outline != null)
        {
            outline.effectDistance = outlineDistance;
            outline.useGraphicAlpha = false;
            outline.effectColor = new Color(outlineColor.r, outlineColor.g, outlineColor.b, 0f);
        }

        baseScale = rt.localScale;
        baseZ = rt.localPosition.z;
        baseRotation = rt.localRotation;
        baseImageColor = targetImage != null ? targetImage.color : Color.white;
        baseLabelColor = label != null ? label.color : Color.white;
    }

    private void AutoAssign()
    {
        if (targetImage == null) targetImage = GetComponent<Image>();
        if (outline == null) outline = GetComponent<Outline>();
        if (label == null) label = GetComponentInChildren<TMP_Text>(true);
        if (audioSource == null) audioSource = GetComponent<AudioSource>();
    }

    private void OnDisable()
    {
        StopAllCoroutines();
        hoverRoutine = pressRoutine = null;
        isHovered = isPressed = false;
        hover = press = 0f;
        if (rt != null) Apply();
    }

    private void Update()
    {
        // El vaivén necesita refrescarse cada frame mientras hay hover
        if (hover > 0.001f) Apply();
    }

    // ───────────────────────── Eventos de puntero ─────────────────────────

    public void OnPointerEnter(PointerEventData e)
    {
        if (!button.interactable) return;

        isHovered = true;
        StartHover(1f, hoverDuration, Ease.OutQuad);
        Feedback(e, hoverClip, hoverHaptic);

        // Si el gatillo ya estaba pulsado sobre este botón y el rayo vuelve a entrar
        if (e.pointerPress == gameObject && !isPressed)
        {
            isPressed = true;
            StartPress(1f, pressDuration, Ease.OutQuad);
        }
    }

    public void OnPointerExit(PointerEventData e)
    {
        isHovered = false;
        StartHover(0f, hoverDuration, Ease.OutQuad);

        if (isPressed)
        {
            // El rayo sale con el gatillo pulsado: se cancela visualmente y no habrá click
            isPressed = false;
            StartPress(0f, hoverDuration, Ease.OutQuad);
        }
    }

    public void OnPointerDown(PointerEventData e)
    {
        if (!button.interactable) return;

        isPressed = true;
        StartPress(1f, pressDuration, Ease.OutQuad);
        Feedback(e, pressClip, pressHaptic);
    }

    public void OnPointerUp(PointerEventData e)
    {
        if (!isPressed) return;

        isPressed = false;
        StartPress(0f, releaseDuration, Ease.OutBack);   // rebote elástico
        if (isHovered) Feedback(e, releaseClip, releaseHaptic);
    }

    // ───────────────────────── Animación ─────────────────────────

    private void StartHover(float target, float duration, System.Func<float, float> ease)
    {
        if (!isActiveAndEnabled) return;
        if (hoverRoutine != null) StopCoroutine(hoverRoutine);
        hoverRoutine = StartCoroutine(Tween(hover, target, duration, ease, v => { hover = v; Apply(); }));
    }

    private void StartPress(float target, float duration, System.Func<float, float> ease)
    {
        if (!isActiveAndEnabled) return;
        if (pressRoutine != null) StopCoroutine(pressRoutine);
        pressRoutine = StartCoroutine(Tween(press, target, duration, ease, v => { press = v; Apply(); }));
    }

    private static IEnumerator Tween(float from, float to, float duration,
        System.Func<float, float> ease, System.Action<float> set)
    {
        float t = 0f;
        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            set(Mathf.LerpUnclamped(from, to, ease(Mathf.Clamp01(t / duration))));
            yield return null;
        }
        set(to);
    }

    private void Apply()
    {
        float h = hover, p = press;
        float h01 = Mathf.Clamp01(h), p01 = Mathf.Clamp01(p);

        // Escala uniforme en los 3 ejes
        float s = Mathf.LerpUnclamped(Mathf.LerpUnclamped(1f, hoverScale, h), pressedScale, p);
        rt.localScale = baseScale * s;

        // Hundimiento en Z (hacia dentro del canvas)
        // Solo se toca la Z; se conserva la X/Y actual que haya calculado el Layout Group
        Vector3 pos = rt.localPosition;
        pos.z = baseZ + pressedDepth * p;
        rt.localPosition = pos;

        // Inclinación + vaivén (el vaivén se apaga al pulsar)
        float sway = Mathf.Sin(Time.unscaledTime * swaySpeed) * swayAmplitude * h01 * (1f - p01);
        rt.localRotation = baseRotation * Quaternion.Euler(0f, 0f, hoverTilt * h + sway);

        // Tinte del sprite
        if (targetImage != null)
        {
            Color c = Color.Lerp(baseImageColor, baseImageColor * hoverTint, h01);
            targetImage.color = Color.Lerp(c, baseImageColor * pressedTint, p01);
        }

        // Outline: fade del alfa
        if (outline != null)
            outline.effectColor = new Color(outlineColor.r, outlineColor.g, outlineColor.b, outlineColor.a * h01);

        // Color del texto
        if (label != null)
            label.color = Color.Lerp(baseLabelColor, labelHoverColor, h01);
    }

    // ───────────────────────── Háptica y sonido ─────────────────────────

    private void Feedback(PointerEventData e, AudioClip clip, Vector2 haptic)
    {
        if (clip != null && audioSource != null)
        {
            audioSource.pitch = 1f + Random.Range(-pitchVariation, pitchVariation);
            audioSource.PlayOneShot(clip);
        }

        if (useHaptics && haptic.x > 0f && haptic.y > 0f &&
            e is TrackedDeviceEventData tracked && tracked.interactor is Component interactor)
        {
            // El HapticImpulsePlayer vive en el controlador (padre del interactor) en el rig de Starter Assets
            var player = interactor.GetComponentInParent<HapticImpulsePlayer>();
            if (player != null) player.SendHapticImpulse(haptic.x, haptic.y);
        }
    }

    // ───────────────────────── Easing ─────────────────────────

    private static class Ease
    {
        public static float OutQuad(float t) => 1f - (1f - t) * (1f - t);

        public static float OutBack(float t)
        {
            const float c1 = 1.9f;
            const float c3 = c1 + 1f;
            return 1f + c3 * Mathf.Pow(t - 1f, 3f) + c1 * Mathf.Pow(t - 1f, 2f);
        }
    }
}
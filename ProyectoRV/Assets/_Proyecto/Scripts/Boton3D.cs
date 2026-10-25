using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using System.Collections;

[RequireComponent(typeof(Collider))]
public class Boton3D : MonoBehaviour,
    IPointerDownHandler, IPointerUpHandler, IPointerExitHandler, IPointerClickHandler
{
    [Tooltip("Objeto que se escala. Mejor un hijo con el modelo; si está vacío, este mismo.")]
    public Transform visual;
    public float pressedScale = 0.9f;
    public float bounceScale = 1.12f;
    public float clickDelay = 0.25f;
    public AudioClip clickSound;
    public UnityEvent onClick;

    Vector3 baseScale, targetScale;
    bool busy;

    void Awake()
    {
        if (!visual) visual = transform;
        baseScale = targetScale = visual.localScale;
    }

    void Update()
    {
        visual.localScale = Vector3.Lerp(visual.localScale, targetScale,
                                         1f - Mathf.Exp(-18f * Time.deltaTime));
    }

    public void OnPointerDown(PointerEventData e) { if (!busy) targetScale = baseScale * pressedScale; }
    public void OnPointerUp(PointerEventData e)   { if (!busy) targetScale = baseScale; }
    public void OnPointerExit(PointerEventData e) { if (!busy) targetScale = baseScale; }

    public void OnPointerClick(PointerEventData e)
    {
        if (!busy) StartCoroutine(ClickRoutine());
    }

    IEnumerator ClickRoutine()
    {
        busy = true;
        targetScale = baseScale * bounceScale;

        if (clickSound) AudioSource.PlayClipAtPoint(clickSound, transform.position);
#if UNITY_ANDROID || UNITY_IOS
        Handheld.Vibrate();
#endif

        yield return new WaitForSeconds(clickDelay);
        targetScale = baseScale;
        onClick.Invoke();
        busy = false;
    }
}

using UnityEngine;
using UnityEngine.EventSystems;

public class JoystickMovimientoUI : MonoBehaviour,
    IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    public RectTransform palanca;

    public Vector2 Direccion { get; private set; }

    private RectTransform baseJoystick;

    void Awake()
    {
        baseJoystick = GetComponent<RectTransform>();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        OnDrag(eventData);
    }

    public void OnDrag(PointerEventData eventData)
    {
        Vector2 posicionLocal;

        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
            baseJoystick,
            eventData.position,
            eventData.pressEventCamera,
            out posicionLocal))
        {
            float radio = Mathf.Min(
                baseJoystick.rect.width,
                baseJoystick.rect.height
            ) / 2f;

            Direccion = Vector2.ClampMagnitude(
                posicionLocal / radio, 1f
            );

            palanca.anchoredPosition = Direccion * radio;
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        Direccion = Vector2.zero;
        palanca.anchoredPosition = Vector2.zero;
    }

    void OnDisable()
    {
        Direccion = Vector2.zero;

        if (palanca != null)
            palanca.anchoredPosition = Vector2.zero;
    }
}

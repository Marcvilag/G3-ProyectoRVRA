
using UnityEngine;
using UnityEngine.EventSystems;

public class JoystickAltitudUI : MonoBehaviour,
    IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    public RectTransform palanca;

    public float Vertical { get; private set; }

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
            float recorrido = (baseJoystick.rect.height -
                               palanca.rect.height) / 2f;

            if (recorrido <= 0f)
                return;

            Vertical = Mathf.Clamp(
                posicionLocal.y / recorrido, -1f, 1f
            );

            palanca.anchoredPosition =
                new Vector2(0f, Vertical * recorrido);
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        Vertical = 0f;
        palanca.anchoredPosition = Vector2.zero;
    }

    void OnDisable()
    {
        Vertical = 0f;

        if (palanca != null)
            palanca.anchoredPosition = Vector2.zero;
    }
}

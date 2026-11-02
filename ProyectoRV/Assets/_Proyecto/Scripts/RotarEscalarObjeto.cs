using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.EnhancedTouch;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;

public class RotarEscalarObjeto : MonoBehaviour
{
    [SerializeField] private float velocidadRotacionRaton = 0.5f;
    [SerializeField] private float velocidadRotacionTactil = 0.4f;

    [SerializeField] private float escalaMinima = 0.5f;
    [SerializeField] private float escalaMaxima = 2.0f;
    [SerializeField] private float velocidadEscala = 0.4f;

    private Vector3 escalaInicial;
    private Vector2 ultimaPosicionRaton;
    private bool estaRotandoRaton = false;
    private Camera camaraPrincipal;

    private void OnEnable()
    {
        EnhancedTouchSupport.Enable();
    }

    private void OnDisable()
    {
        EnhancedTouchSupport.Disable();
    }

    private void Start()
    {
        escalaInicial = transform.localScale;
        camaraPrincipal = Camera.main;
    }

    private void Update()
    {
        var touches = Touch.activeTouches;

        if (touches.Count > 0)
        {
            ManejarInputTactil(touches);
        }
        else if (Mouse.current != null)
        {
            ManejarInputRaton();
        }
    }

    private void ManejarInputTactil(System.Collections.Generic.IReadOnlyList<Touch> touches)
    {
        if (touches.Count == 1)
        {
            var touch = touches[0];
            Vector2 delta = touch.delta;

            if (delta.sqrMagnitude > 0.01f)
            {
                float rotX = -delta.y * velocidadRotacionTactil;
                float rotY = delta.x * velocidadRotacionTactil;

                transform.Rotate(camaraPrincipal.transform.up, rotY, Space.World);
                transform.Rotate(camaraPrincipal.transform.right, rotX, Space.World);
            }
        }
        else if (touches.Count >= 2)
        {
            var t1 = touches[0];
            var t2 = touches[1];

            Vector2 pos1 = t1.screenPosition;
            Vector2 pos2 = t2.screenPosition;
            Vector2 prevPos1 = pos1 - t1.delta;
            Vector2 prevPos2 = pos2 - t2.delta;

            float distanciaAnterior = (prevPos1 - prevPos2).magnitude;
            float distanciaActual = (pos1 - pos2).magnitude;

            float diferencia = distanciaActual - distanciaAnterior;
            if (Mathf.Abs(diferencia) > 0.01f)
            {
                float incremento = Mathf.Sign(diferencia) * velocidadEscala * 0.1f;
                AplicarEscala(incremento);
            }
        }
    }

    private void ManejarInputRaton()
    {
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            ultimaPosicionRaton = Mouse.current.position.ReadValue();
            estaRotandoRaton = true;
        }

        if (Mouse.current.leftButton.wasReleasedThisFrame)
        {
            estaRotandoRaton = false;
        }

        if (estaRotandoRaton && Mouse.current.leftButton.isPressed)
        {
            Vector2 posicionActual = Mouse.current.position.ReadValue();
            Vector2 diferencia = posicionActual - ultimaPosicionRaton;

            float rotX = -diferencia.y * velocidadRotacionRaton;
            float rotY = diferencia.x * velocidadRotacionRaton;

            transform.Rotate(camaraPrincipal.transform.up, rotY, Space.World);
            transform.Rotate(camaraPrincipal.transform.right, rotX, Space.World);

            ultimaPosicionRaton = posicionActual;
        }

        float scroll = Mouse.current.scroll.ReadValue().y;
        if (Mathf.Abs(scroll) > 0.01f)
        {
            float incremento = Mathf.Sign(scroll) * velocidadEscala;
            AplicarEscala(incremento);
        }
    }

    private void AplicarEscala(float incremento)
    {
        Vector3 nuevaEscala = transform.localScale + Vector3.one * incremento;

        float escalaClampeadaX = Mathf.Clamp(nuevaEscala.x, escalaInicial.x * escalaMinima, escalaInicial.x * escalaMaxima);
        float escalaClampeadaY = Mathf.Clamp(nuevaEscala.y, escalaInicial.y * escalaMinima, escalaInicial.y * escalaMaxima);
        float escalaClampeadaZ = Mathf.Clamp(nuevaEscala.z, escalaInicial.z * escalaMinima, escalaInicial.z * escalaMaxima);

        transform.localScale = new Vector3(escalaClampeadaX, escalaClampeadaY, escalaClampeadaZ);
    }
}
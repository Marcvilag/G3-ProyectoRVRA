using UnityEngine;

public class BotonOpciones : MonoBehaviour
{
    void Awake()
    {
        var b = GetComponent<Boton3D>();
        if (b) b.onClick.AddListener(Abrir);
    }

    // Para botones de UI (Canvas): arrastra este objeto al OnClick y elige Abrir
    public void Abrir() => OpcionesEscena.Abrir();
}
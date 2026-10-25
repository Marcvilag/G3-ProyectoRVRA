using UnityEngine;

[RequireComponent(typeof(Boton3D))]
public class BotonEscena : MonoBehaviour
{
    public MapaEscenas mapa;
    public Destino destino;

    void Awake() => GetComponent<Boton3D>().onClick.AddListener(Ir);

    void Ir() => CargadorEscenas.Cargar(mapa.Nombre(destino));
}
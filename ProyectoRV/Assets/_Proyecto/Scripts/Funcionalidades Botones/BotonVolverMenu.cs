using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Botón de UI (Canvas) que vuelve al menú principal usando el sistema de escenas del proyecto
/// (MapaEscenas + CargadorEscenas). Añadir al Button y asignar el asset MapaEscenas.
/// </summary>
[RequireComponent(typeof(Button))]
public class BotonVolverMenu : MonoBehaviour
{
    public MapaEscenas mapa;
    public Destino destino = Destino.Menu;

    void Awake() => GetComponent<Button>().onClick.AddListener(Ir);

    void Ir() => CargadorEscenas.Cargar(mapa.Nombre(destino));
}

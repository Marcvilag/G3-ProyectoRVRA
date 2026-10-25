using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public enum Destino { Menu, Minijuego, ProbarMoscas, Video }

[CreateAssetMenu(menuName = "Proyecto/Mapa de escenas")]
public class MapaEscenas : ScriptableObject
{
    [System.Serializable]
    public class Entrada
    {
        public Destino destino;
#if UNITY_EDITOR
        public SceneAsset escena;           // se arrastra la escena aquí
#endif
        [HideInInspector] public string nombre;
    }

    public Entrada[] entradas;

    public string Nombre(Destino d)
    {
        foreach (var e in entradas)
            if (e.destino == d) return e.nombre;
        return null;
    }

#if UNITY_EDITOR
    void OnValidate()
    {
        foreach (var e in entradas)
            e.nombre = e.escena ? e.escena.name : "";
    }
#endif
}
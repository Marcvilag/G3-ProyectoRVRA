using UnityEngine;

public class GestorPuntos : MonoBehaviour
{
    public static GestorPuntos instancia;
    public int puntuacionTotal = 0;

    void Awake()
    {
        instancia = this;
    }

    public void ModificarPuntos(int cantidad)
    {
        puntuacionTotal += cantidad;
        Debug.Log("Puntuación actual: " + puntuacionTotal);
    }
}
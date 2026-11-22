using UnityEngine;

public class PanelOpcionesFlotante : MonoBehaviour
{
    [SerializeField] GameObject panel;           // PanelOpciones
    [SerializeField] float distancia = 1.2f;
    [SerializeField] float alturaOffset = -0.1f; // respecto a los ojos

    void Awake()
    {
        if (panel != null) panel.SetActive(false);
    }

    public void Abrir()
    {
        var cam = Camera.main;
        if (cam == null) return;

        // Dirección horizontal hacia donde mira el jugador (ignora si mira arriba o abajo)
        Vector3 dir = cam.transform.forward;
        dir.y = 0f;
        if (dir.sqrMagnitude < 0.001f) dir = cam.transform.up; // por si mira recto arriba/abajo
        dir.Normalize();

        Vector3 pos = cam.transform.position + dir * distancia;
        pos.y = cam.transform.position.y + alturaOffset;

        transform.position = pos;
        // El frente del canvas (+Z) debe apuntar en la misma dirección que la mirada
        transform.rotation = Quaternion.LookRotation(pos - cam.transform.position);

        panel.SetActive(true);
    }

    public void Cerrar()
    {
        panel.SetActive(false);
    }
}
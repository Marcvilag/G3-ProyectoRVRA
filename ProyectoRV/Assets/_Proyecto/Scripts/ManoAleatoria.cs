using UnityEngine;

// Reproduce al azar una de las animaciones de golpe, con una pausa aleatoria entre golpes.
// Los nombres tienen que coincidir con los estados del Animator Controller.
[RequireComponent(typeof(Animator))]
public class ManoAleatoria : MonoBehaviour
{
    public string[] animaciones = { "Golpe_Simple", "Golpe_Doble", "Golpe_Amago" };
    public float pausaMin = 0.5f;
    public float pausaMax = 2f;

    Animator animator;
    float esperaHasta = -1f;
    int ultima = 0;   // la primera que suena es Golpe_Simple (estado por defecto)

    void Awake()
    {
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        if (animator.IsInTransition(0)) return;

        var estado = animator.GetCurrentAnimatorStateInfo(0);
        if (estado.normalizedTime < 1f) return;          // todavía está golpeando

        if (esperaHasta < 0f)                             // acaba de terminar: decide la pausa
        {
            esperaHasta = Time.time + Random.Range(pausaMin, pausaMax);
            return;
        }
        if (Time.time < esperaHasta) return;

        int i;                                            // elige otra distinta a la anterior
        do { i = Random.Range(0, animaciones.Length); }
        while (animaciones.Length > 1 && i == ultima);

        ultima = i;
        animator.CrossFade(animaciones[i], 0.1f, 0, 0f);
        esperaHasta = -1f;
    }
}
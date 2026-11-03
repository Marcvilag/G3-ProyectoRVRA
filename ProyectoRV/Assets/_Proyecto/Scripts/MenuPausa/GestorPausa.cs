using System;
using System.Collections;
using UnityEngine;

public class GestorPausa : MonoBehaviour
{
    public static GestorPausa Instancia { get; private set; }

    // Para consultar desde cualquier script: if (GestorPausa.EnPausa) return;
    public static bool EnPausa => Instancia != null && Instancia.pausado;

    public event Action<bool> PausaCambiada;   // true = pausado
    public event Action<int> CuentaAtras;      // 3, 2, 1 y 0 al terminar

    [Min(0)] public int segundosCuentaAtras = 3;   // 0 mientras desarrollas
    public bool pausarAudio = true;

    bool pausado;
    Coroutine cuenta;

    public bool Pausado => pausado;
    public bool Reanudando => cuenta != null;

    void Awake() => Instancia = this;

    void OnDestroy()
    {
        if (Instancia != this) return;
        Instancia = null;
        Time.timeScale = 1f;            // al salir de la escena nunca debe quedar el juego parado
        AudioListener.pause = false;
    }

    public void Pausar()
    {
        if (cuenta != null) { StopCoroutine(cuenta); cuenta = null; CuentaAtras?.Invoke(0); }
        Aplicar(true);
    }

    public void Reanudar()
    {
        if (!pausado || cuenta != null) return;
        cuenta = StartCoroutine(ReanudarRoutine());
    }

    IEnumerator ReanudarRoutine()
    {
        for (int i = segundosCuentaAtras; i > 0; i--)
        {
            CuentaAtras?.Invoke(i);
            yield return new WaitForSecondsRealtime(1f);
        }
        cuenta = null;
        CuentaAtras?.Invoke(0);
        Aplicar(false);
    }

    void Aplicar(bool p)
    {
        if (pausado == p) return;
        pausado = p;
        Time.timeScale = p ? 0f : 1f;
        if (pausarAudio) AudioListener.pause = p;
        PausaCambiada?.Invoke(p);
    }
}
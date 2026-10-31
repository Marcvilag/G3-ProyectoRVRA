using UnityEngine;
using UnityEngine.Audio;

public enum Canal { General, Musica, Efectos }

public static class AjustesAudio
{
    static string Parametro(Canal c)
    {
        switch (c)
        {
            case Canal.General: return "VolMaster";
            case Canal.Musica:  return "VolMusica";
            default:            return "VolEfectos";
        }
    }

    static string Clave(Canal c) => "vol_" + c;

    public static float Obtener(Canal c) => PlayerPrefs.GetFloat(Clave(c), 1f);

    // lineal: 0 = silencio, 1 = nivel original
    public static void Establecer(AudioMixer mixer, Canal c, float lineal, bool guardar = true)
    {
        lineal = Mathf.Clamp01(lineal);
        float db = lineal <= 0.0001f ? -80f : Mathf.Log10(lineal) * 20f;

        if (!mixer.SetFloat(Parametro(c), db))
            Debug.LogWarning($"AjustesAudio: el parámetro '{Parametro(c)}' no está expuesto en el mixer o el nombre no coincide.");

        if (guardar) PlayerPrefs.SetFloat(Clave(c), lineal);
    }

    public static void AplicarGuardados(AudioMixer mixer)
    {
        foreach (Canal c in System.Enum.GetValues(typeof(Canal)))
        {
            // Si el parámetro no está expuesto (por ejemplo VolMaster), se omite sin avisar
            if (!mixer.GetFloat(Parametro(c), out _)) continue;
            Establecer(mixer, c, Obtener(c), false);
        }
    }

    public static void Guardar() => PlayerPrefs.Save();
}
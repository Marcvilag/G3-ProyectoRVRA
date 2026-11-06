using UnityEngine;
using TMPro;

public class GestorPantallaFin : MonoBehaviour
{
    [Header("Referencias de Texto del Post-it")]
    public TextMeshProUGUI textoTiempo;
    public TextMeshProUGUI textoPuntosObjetos;
    public TextMeshProUGUI textoTotal;
    public TextMeshProUGUI textoRecord;      // Nuevo texto para mostrar el récord histórico

    [Header("Configuración de Puntuación")]
    public int puntosPorCadaSegundo = 2;

    void Start()
    {
        // 1. Recuperamos los datos de la partida actual
        int puntosObjetos = PlayerPrefs.GetInt("PuntuacionFinal", 0);
        float tiempoTotalSegundos = PlayerPrefs.GetFloat("TiempoFinal", 0f);

        // 2. Cálculos
        int minutos = Mathf.FloorToInt(tiempoTotalSegundos / 60f);
        int segundos = Mathf.FloorToInt(tiempoTotalSegundos % 60f);
        int puntosPorTiempo = Mathf.FloorToInt(tiempoTotalSegundos * puntosPorCadaSegundo);
        int puntuacionTotalConjunta = puntosObjetos + puntosPorTiempo;

        // 3. Sistema de Récord (High Score)
        // Obtenemos el récord guardado anteriormente (si no hay ninguno, vale 0)
        int recordActual = PlayerPrefs.GetInt("RecordPuntuacion", 0);

        // Si nuestra puntuación de esta partida es mayor que el récord, lo actualizamos
        if (puntuacionTotalConjunta > recordActual)
        {
            recordActual = puntuacionTotalConjunta;
            PlayerPrefs.SetInt("RecordPuntuacion", recordActual);
            PlayerPrefs.Save();
            Debug.Log("¡Nuevo récord histórico guardado: " + recordActual + "!");
        }

        // 4. Rellenamos los textos en el diseño de la nevera
        if (textoTiempo != null)
        {
            textoTiempo.text = "Tiempo: " + minutos.ToString("00") + ":" + segundos.ToString("00");
        }

        if (textoPuntosObjetos != null)
        {
            textoPuntosObjetos.text = "Objetos: " + puntosObjetos + " pts";
        }

        if (textoTotal != null)
        {
            textoTotal.text = "TOTAL: " + puntuacionTotalConjunta + " pts";
        }

        if (textoRecord != null)
        {
            textoRecord.text = "RÉCORD: " + recordActual + " pts";
        }
    }
}
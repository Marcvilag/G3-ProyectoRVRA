using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

// Un solo script para toda la secuencia, sin Timeline.
// Ponlo en un objeto vacío (por ejemplo "Secuencia") y arrastra las casillas.
public class SecuenciaVideo : MonoBehaviour
{
    [Header("Yaya en la cama")]
    public GameObject yayaCama;
    public AnimationClip clipCama;     // el clip "Cama" (dentro de animacion-yaya-cama.fbx)

    [Header("Yaya en la moto")]
    public GameObject scooter;         // el padre que contiene la moto y YayaMoto (empieza desactivado)
    public GameObject yayaMoto;
    public AnimationClip clipMoto;     // el clip "Moto" (dentro de animacion_yaya_moto.fbx)

    [Header("Moscas")]
    public GameObject enjambre;        // SwarmManager

    [Header("Cámaras")]
    public GameObject camaraHabitacion;  // la Main Camera
    public GameObject camaraMoto;        // una cámara hija de Scooter, colocada detrás

    [Header("Tiempos")]
    [Tooltip("Segundo en que la yaya se sube a la moto y cambia la cámara.")]
    public float inicioMoto = 12f;

    PlayableGraph grafoCama;
    PlayableGraph grafoMoto;
    float t;
    bool cambioHecho;

    void Start()
    {
        // Estado inicial
        scooter.SetActive(false);
        yayaCama.SetActive(true);
        camaraHabitacion.SetActive(true);
        if (camaraMoto != null) camaraMoto.SetActive(false);
        if (enjambre != null) enjambre.SetActive(true);

        Animator animCama = yayaCama.GetComponentInChildren<Animator>();
        if (animCama != null && clipCama != null)
            AnimationPlayableUtilities.PlayClip(animCama, clipCama, out grafoCama);
        else
            Debug.LogWarning("SecuenciaVideo: falta el Animator de YayaCama o el clip Cama.");
    }

    void Update()
    {
        t += Time.deltaTime;

        if (!cambioHecho && t >= inicioMoto)
        {
            cambioHecho = true;

            // Primero se enciende lo nuevo, luego se apaga lo viejo, para que nunca falte una cámara.
            scooter.SetActive(true);
            if (camaraMoto != null) camaraMoto.SetActive(true);
            yayaCama.SetActive(false);
            camaraHabitacion.SetActive(false);

            Animator animMoto = yayaMoto.GetComponentInChildren<Animator>();
            if (animMoto != null && clipMoto != null)
                AnimationPlayableUtilities.PlayClip(animMoto, clipMoto, out grafoMoto);
            else
                Debug.LogWarning("SecuenciaVideo: falta el Animator de YayaMoto o el clip Moto.");
        }
    }

    void OnDestroy()
    {
        if (grafoCama.IsValid()) grafoCama.Destroy();
        if (grafoMoto.IsValid()) grafoMoto.Destroy();
    }
}

using UnityEngine;

public class LogTick : MonoBehaviour
{
    float next;
    void Update()
    {
        if (Time.unscaledTime < next) return;
        next = Time.unscaledTime + 0.5f;
        Debug.Log($"TICK t={Time.unscaledTime:0.0} frame={Time.frameCount}");
    }
}
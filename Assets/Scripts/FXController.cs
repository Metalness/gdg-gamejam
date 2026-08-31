using UnityEngine;

public class FXController : MonoBehaviour
{
    [Header("Sanity")]
    [Range(0f, 1f)]
    public float sanity = 1f;

    [Header("Camera Shake")]
    public CameraShake cameraShake;

    [Header("Heartbeat")]
    public AudioSource slowHeartbeat;
    public AudioSource fastHeartbeat;

    [Header("Background Static")]
    public AudioSource backgroundStatic;

    [Header("Intensity")]
    public float maxHeartbeatVolume = 1f;
    public float maxStaticVolume = 1f;
    public float maxShakeIntensity = 1f;

    void Update()
    {
        // 1 = sane, 0 = insane
        float intensity = 1f - Mathf.Clamp01(sanity);

        // Camera shake
        if (cameraShake != null)
        {
            cameraShake.intensity = intensity * maxShakeIntensity;
        }

        // Background static
        if (backgroundStatic != null)
        {
            backgroundStatic.volume = intensity * maxStaticVolume;
        }

        // Choose heartbeat based on sanity
        if (sanity > 0.5f)
        {
            if (fastHeartbeat != null)
                fastHeartbeat.Stop();

            if (slowHeartbeat != null)
            {
                slowHeartbeat.volume = intensity * maxHeartbeatVolume;

                if (!slowHeartbeat.isPlaying)
                    slowHeartbeat.Play();
            }
        }
        else
        {
            if (slowHeartbeat != null)
                slowHeartbeat.Stop();

            if (fastHeartbeat != null)
            {
                fastHeartbeat.volume = intensity * maxHeartbeatVolume;

                if (!fastHeartbeat.isPlaying)
                    fastHeartbeat.Play();
            }
        }
    }
}
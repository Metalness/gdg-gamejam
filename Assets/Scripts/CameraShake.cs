using UnityEngine;

public class CameraShake : MonoBehaviour
{
    [Header("Shake")]
    public float intensity = 0f;
    public float frequency = 20f;

    private Vector3 originalPosition;
    private Quaternion originalRotation;

    void Start()
    {
        originalPosition = transform.localPosition;
        originalRotation = transform.localRotation;
    }

    void LateUpdate()
    {
        if (intensity <= 0f)
        {
            transform.localPosition = originalPosition;
            transform.localRotation = originalRotation;
            return;
        }

        float x = (Mathf.PerlinNoise(Time.time * frequency, 0f) - 0.5f) * 2f;
        float y = (Mathf.PerlinNoise(0f, Time.time * frequency) - 0.5f) * 2f;
        float z = (Mathf.PerlinNoise(Time.time * frequency, Time.time * frequency) - 0.5f) * 2f;

        Vector3 shake = new Vector3(x, y, z) * intensity;

        transform.localPosition = originalPosition + shake;

        // Small rotational shake
        transform.localRotation = originalRotation *
            Quaternion.Euler(
                shake.y * 2f,
                shake.x * 2f,
                shake.z * 2f
            );
    }
}
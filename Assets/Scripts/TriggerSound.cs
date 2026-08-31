using System.Collections;
using UnityEngine;

public class TriggerSound : MonoBehaviour
{
    [Header("Trigger")]
    public Collider triggerCollider;

    [Header("Sound")]
    public AudioSource audioSource;

    [Header("Camera Shake")]
    public CameraShake cameraShake;
    public float shakeIntensity = 1f;
    public float shakeDuration = 0.5f;

    [Header("Sanity")]
    public float targetSanity = 0.5f;
    public float sanityDuration = 5f;

    private bool triggered = false;
    private FXController fxController;

    void Start()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");

        if (player != null)
        {
            fxController = player.GetComponent<FXController>();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (triggered)
            return;

        if (other.CompareTag("Player"))
        {
            triggered = true;

            if (audioSource != null && !audioSource.isPlaying)
            {
                audioSource.Play();

                if (fxController != null)
                    StartCoroutine(ReduceSanity());
            }

            if (cameraShake != null)
            {
                StartCoroutine(ShakeCamera());
            }
        }
    }

    private IEnumerator ShakeCamera()
    {
        float originalIntensity = cameraShake.intensity;

        cameraShake.intensity = shakeIntensity;

        yield return new WaitForSeconds(shakeDuration);

        cameraShake.intensity = originalIntensity;
    }

    private IEnumerator ReduceSanity()
    {
        float startSanity = fxController.sanity;
        float elapsed = 0f;

        while (elapsed < sanityDuration)
        {
            elapsed += Time.deltaTime;

            float t = elapsed / sanityDuration;

            fxController.sanity = Mathf.Lerp(
                startSanity,
                targetSanity,
                t
            );

            yield return null;
        }

        fxController.sanity = targetSanity;
    }
}
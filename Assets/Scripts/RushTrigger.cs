using System.Collections;
using UnityEngine;

public class RushTrigger : MonoBehaviour
{
    [Header("Trigger")]
    public Collider triggerCollider;

    [Header("Target")]
    public Transform targetTransform;

    [Header("Object")]
    public GameObject prefab;

    [Header("Rush Settings")]
    public float spawnDistance = 15f;
    public float rushSpeed = 50f;
    public float destroyDistance = 100f;

    [Header("Camera Shake")]
    public CameraShake cameraShake;
    public float shakeIntensity = 1f;
    public float shakeDuration = 0.5f;

    [Header("Sanity")]
    public float targetSanity = 0.9f;
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

    void Update()
    {
        if (triggered || triggerCollider == null || targetTransform == null || prefab == null)
            return;

        if (triggerCollider.bounds.Contains(targetTransform.position))
        {
            triggered = true;
            SpawnAndRush();
        }
    }

    void SpawnAndRush()
    {
        Transform target = targetTransform;

        Vector3 spawnPosition =
            target.position + target.right * spawnDistance;

        GameObject obj = Instantiate(
            prefab,
            spawnPosition,
            Quaternion.identity
        );

        StartCoroutine(RushObject(obj, target));
    }

    IEnumerator RushObject(GameObject obj, Transform target)
    {
        Vector3 finalDirection = Vector3.zero;
        bool reachedTarget = false;

        while (obj != null)
        {
            if (!reachedTarget)
            {
                Vector3 direction =
                    (target.position - obj.transform.position).normalized;

                obj.transform.position +=
                    direction * rushSpeed * Time.deltaTime;

                if (Vector3.Distance(
                    obj.transform.position,
                    target.position) < 0.5f)
                {
                    finalDirection = direction;
                    reachedTarget = true;

                    if (cameraShake != null)
                    {
                        StartCoroutine(ShakeCamera());
                    }

                    if (fxController != null)
                    {
                        StartCoroutine(ReduceSanity());
                    }
                }
            }
            else
            {
                obj.transform.position +=
                    finalDirection * rushSpeed * Time.deltaTime;
            }

            if (Vector3.Distance(
                obj.transform.position,
                target.position) > destroyDistance)
            {
                Destroy(obj);
                yield break;
            }

            yield return null;
        }
    }

    IEnumerator ShakeCamera()
    {
        float originalIntensity = cameraShake.intensity;

        cameraShake.intensity = shakeIntensity;

        yield return new WaitForSeconds(shakeDuration);

        cameraShake.intensity = originalIntensity;
    }

    IEnumerator ReduceSanity()
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
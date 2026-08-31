using System.Collections;
using UnityEngine;

public class MovePrefabTwoPoints : MonoBehaviour
{
    [Header("Trigger")]
    public Collider triggerCollider;

    [Header("Player")]
    public Transform player;

    [Header("Points")]
    public Transform startPoint;
    public Transform endPoint;

    [Header("Object")]
    public GameObject prefab;
    public float speed = 10f;

    [Header("Sanity")]
    public float targetSanity = 0.75f;
    public float sanityDuration = 5f;

    private GameObject spawnedObject;
    private bool triggered = false;
    private FXController fxController;

    void Start()
    {
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");

        if (playerObject != null)
        {
            fxController = playerObject.GetComponent<FXController>();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (triggered)
            return;

        if (other.transform == player || other.CompareTag("Player"))
        {
            triggered = true;
            StartMovement();

            if (fxController != null)
            {
                StartCoroutine(ReduceSanity());
            }
        }
    }

    void Update()
    {
        if (spawnedObject == null)
            return;

        spawnedObject.transform.position = Vector3.MoveTowards(
            spawnedObject.transform.position,
            endPoint.position,
            speed * Time.deltaTime
        );

        if (Vector3.Distance(
            spawnedObject.transform.position,
            endPoint.position) < 0.01f)
        {
            Destroy(spawnedObject);
            spawnedObject = null;
        }
    }

    void StartMovement()
    {
        spawnedObject = Instantiate(
            prefab,
            startPoint.position,
            startPoint.rotation
        );
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
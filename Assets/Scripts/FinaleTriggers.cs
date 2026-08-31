using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class FinaleTriggers : MonoBehaviour
{
    [Header("Triggers")]
    public Collider sanityTrigger;
    public Collider sceneTrigger;

    [Header("Sanity")]
    public float sanityDuration = 5f;

    [Header("Scene")]
    public string nextScene;

    private FXController fxController;
    private Transform player;

    private bool sanityTriggered = false;
    private bool sceneTriggered = false;

    void Start()
    {
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");

        if (playerObject != null)
        {
            player = playerObject.transform;
            fxController = playerObject.GetComponent<FXController>();
        }
    }

    void Update()
    {
        if (player == null)
            return;

        // Sanity trigger
        if (!sanityTriggered &&
            sanityTrigger != null &&
            sanityTrigger.bounds.Contains(player.position))
        {
            sanityTriggered = true;
            StartCoroutine(LowerSanity());
        }

        // Scene trigger
        if (!sceneTriggered &&
            sceneTrigger != null &&
            sceneTrigger.bounds.Contains(player.position))
        {
            sceneTriggered = true;

            if (!string.IsNullOrEmpty(nextScene))
                SceneManager.LoadScene(nextScene);
        }
    }

    IEnumerator LowerSanity()
    {
        if (fxController == null)
            yield break;

        float startSanity = fxController.sanity;
        float elapsed = 0f;

        while (elapsed < sanityDuration)
        {
            elapsed += Time.deltaTime;

            fxController.sanity = Mathf.Lerp(
                startSanity,
                0f,
                elapsed / sanityDuration
            );

            yield return null;
        }

        fxController.sanity = 0f;
    }
}
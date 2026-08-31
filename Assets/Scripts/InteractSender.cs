using UnityEngine;
using TMPro;

public class InteractSender : MonoBehaviour
{
    [Header("Player")]
    public Camera playerCamera;

    [Header("Interaction")]
    public float interactionDistance = 3f;
    public KeyCode interactionKey = KeyCode.E;
    public LayerMask interactionLayers = ~0;

    [Header("UI")]
    public TMP_Text interactionText;

    private InteractReceiver currentReceiver;

    void Update()
    {
        CheckForInteraction();

        if (currentReceiver != null &&
            Input.GetKeyDown(interactionKey))
        {
            currentReceiver.Interact();
        }
    }

    void CheckForInteraction()
    {
        currentReceiver = null;

        Ray ray = new Ray(
            playerCamera.transform.position,
            playerCamera.transform.forward
        );

        if (Physics.Raycast(
            ray,
            out RaycastHit hit,
            interactionDistance,
            interactionLayers
        ))
        {
            currentReceiver = hit.collider.GetComponentInParent<InteractReceiver>();
        }

        // Update interaction text
        if (currentReceiver != null)
        {
            interactionText.text =
                currentReceiver.GetInteractionText();

            interactionText.gameObject.SetActive(true);
        }
        else
        {
            interactionText.gameObject.SetActive(false);
        }
    }
}
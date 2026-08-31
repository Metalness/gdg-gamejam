using UnityEngine;
using UnityEngine.Events;

public class InteractReceiver : MonoBehaviour
{
    [Header("Interaction")]
    [TextArea]
    public string interactionText = "Interact";

    public UnityEvent onInteract;

    public void Interact()
    {
        onInteract?.Invoke();
    }

    public string GetInteractionText()
    {
        return interactionText;
    }
}
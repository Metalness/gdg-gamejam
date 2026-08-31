using UnityEngine;
using UnityEngine.Rendering;

public class VisionController : MonoBehaviour
{
    public bool pressureVision = false;

    public Volume volume;
    public VolumeProfile normalVisionProfile;
    public VolumeProfile pressureVisionProfile;
    public Light flashLight;

    [Header("Toggle Settings")]
    public float toggleCooldown = 5f;

    private float cooldownTimer = 0f;

    void Start()
    {
        ApplyVision();
    }

    void Update()
    {
        // Count down cooldown
        if (cooldownTimer > 0f)
            cooldownTimer -= Time.deltaTime;

        // Toggle with Tab if cooldown is ready
        if (Input.GetKeyDown(KeyCode.Tab) && cooldownTimer <= 0f)
        {
            pressureVision = !pressureVision;
            cooldownTimer = toggleCooldown;

            ApplyVision();
        }
    }

    void ApplyVision()
    {
        if (pressureVision)
        {
            volume.profile = pressureVisionProfile;
            flashLight.enabled = false;
        }
        else
        {
            volume.profile = normalVisionProfile;
            flashLight.enabled = true;
        }
    }
}
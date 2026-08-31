using UnityEngine;

public class VisionControllerObject : MonoBehaviour
{
    private GameObject player;
    private ParticleSystem targetParticleSystem;
    private MeshRenderer meshRenderer;
    private Rigidbody rb;

    public float emissionMultiplier = 20;

    private int originalLayer;
    private int ignoreFogLayer;

    public bool useVelocity = false;

    void Start()
    {
        player = GameObject.FindWithTag("Player");

        targetParticleSystem = GetComponent<ParticleSystem>();
        meshRenderer = GetComponent<MeshRenderer>();
        rb = GetComponent<Rigidbody>();

        originalLayer = gameObject.layer;
        ignoreFogLayer = LayerMask.NameToLayer("Ignore Fog");
    }

    void Update()
    {
        if (player.GetComponent<VisionController>().pressureVision)
        {
            // Move object to the fog bypass layer
            gameObject.layer = ignoreFogLayer;

            // Only disable renderer if this object has one
            if (meshRenderer != null)
            {
                meshRenderer.enabled = false;
            }

            if (targetParticleSystem != null)
            {
                if (!targetParticleSystem.isPlaying)
                {
                    targetParticleSystem.Play();
                }

                var emission = targetParticleSystem.emission;

                if (useVelocity)
                {
                    emission.rateOverTime = emissionMultiplier;
                }
                else if (rb != null)
                {
                    emission.rateOverTime =
                        rb.linearVelocity.magnitude * emissionMultiplier;
                }
                else
                {
                    emission.rateOverTime = emissionMultiplier;
                }
            }
        }
        else
        {
            // Restore original layer
            gameObject.layer = originalLayer;

            // Only enable renderer if this object has one
            if (meshRenderer != null)
            {
                meshRenderer.enabled = true;
            }

            if (targetParticleSystem != null)
            {
                targetParticleSystem.Clear();
                targetParticleSystem.Stop();
            }
        }
    }
}
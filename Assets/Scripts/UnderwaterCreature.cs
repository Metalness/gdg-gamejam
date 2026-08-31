using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Boid-style movement for fish / underwater creatures.
/// Features:
///  - Schooling (separation / alignment / cohesion) with nearby agents in the same "school" group
///  - Preferred swim depth (target Y level)
///  - Terrain / obstacle avoidance via raycasts
///  - External target support (other scripts can call SetTarget/ClearTarget)
///  - Roaming mode with wander behaviour when no target is set
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class FishSchoolAgent : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 3f;
    public float maxSteerForce = 4f;
    public float rotationSpeed = 4f;

    [Header("Depth (Target Y)")]
    [Tooltip("Preferred Y level to swim at when roaming / no explicit target overrides it.")]
    public float targetYLevel = 0f;
    public float depthWeight = 1f;
    [Tooltip("If true, targetYLevel is ignored while an external target is active (target's Y is used instead).")]
    public bool depthOverriddenByTarget = true;

    [Header("Schooling")]
    public bool enableSchooling = true;
    public string schoolTag = "Fish";
    public float neighborRadius = 5f;
    public float separationRadius = 1.5f;
    public float separationWeight = 1.5f;
    public float alignmentWeight = 1f;
    public float cohesionWeight = 1f;
    public LayerMask schoolLayer;

    [Header("Roaming / Wander")]
    public float wanderRadius = 3f;
    public float wanderDistance = 6f;
    public float wanderJitter = 1f;
    public float roamAreaRadius = 25f;
    public Vector3 roamCenter;
    private Vector3 wanderTarget;

    [Header("Obstacle / Terrain Avoidance")]
    public LayerMask obstacleMask;
    public float avoidDistance = 3f;
    public float avoidWeight = 3f;
    [Tooltip("Number of whisker rays cast forward to detect obstacles/terrain.")]
    public int whiskerCount = 5;
    public float whiskerSpreadAngle = 60f;

    [Header("External Target")]
    [SerializeField] private Transform target;      // assign via SetTarget()
    public float targetWeight = 2f;
    public float arriveRadius = 1f; // slows down as it approaches, optional

    private Rigidbody rb;
    private static readonly List<FishSchoolAgent> allAgents = new List<FishSchoolAgent>();

    void OnEnable()  => allAgents.Add(this);
    void OnDisable() => allAgents.Remove(this);

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = false;
        rb.linearDamping = 1f; // Unity 6 uses linearDamping; use rb.drag = 1f on older Unity versions
        wanderTarget = transform.position + transform.forward * wanderDistance;
        if (roamCenter == Vector3.zero) roamCenter = transform.position;
    }

    void FixedUpdate()
    {
        Vector3 steer = Vector3.zero;

        if (target != null)
        {
            steer += Seek(target.position) * targetWeight;
        }
        else
        {
            steer += Wander() * 1f;
            steer += KeepInRoamArea() * 1f;
        }

        if (enableSchooling)
        {
            steer += Schooling() * 1f;
        }

        // Depth control: only apply own targetYLevel if no target, or if not overridden
        if (target == null || !depthOverriddenByTarget)
        {
            steer += DepthHold(targetYLevel) * depthWeight;
        }
        else
        {
            // gently match target's Y too (already covered by Seek, but reinforce)
            steer += DepthHold(target.position.y) * depthWeight * 0.5f;
        }

        steer += AvoidObstacles() * avoidWeight;

        steer = Vector3.ClampMagnitude(steer, maxSteerForce);

        Vector3 desiredVelocity = rb.linearVelocity + steer * Time.fixedDeltaTime;
        desiredVelocity = Vector3.ClampMagnitude(desiredVelocity, moveSpeed);
        rb.linearVelocity = desiredVelocity;

        // Face movement direction
        if (rb.linearVelocity.sqrMagnitude > 0.01f)
        {
            Quaternion lookRot = Quaternion.LookRotation(rb.linearVelocity.normalized);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRot, rotationSpeed * Time.fixedDeltaTime);
        }
    }

    // ---------- Public API for other scripts ----------

    /// <summary>Call from another script to make this fish chase/follow a target.</summary>
    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }

    /// <summary>Clears the target, returning the fish to roaming mode.</summary>
    public void ClearTarget()
    {
        target = null;
    }

    public bool HasTarget => target != null;
    public Transform CurrentTarget => target;

    // ---------- Steering behaviours ----------

    private Vector3 Seek(Vector3 targetPos)
    {
        Vector3 desired = targetPos - transform.position;
        float dist = desired.magnitude;
        desired = desired.normalized * moveSpeed;

        // Optional slow-down on arrival
        if (dist < arriveRadius && arriveRadius > 0f)
        {
            desired *= (dist / arriveRadius);
        }

        return desired - rb.linearVelocity;
    }

    private Vector3 DepthHold(float yLevel)
    {
        float diff = yLevel - transform.position.y;
        return new Vector3(0f, diff, 0f);
    }

    private Vector3 Wander()
    {
        wanderTarget += new Vector3(
            Random.Range(-1f, 1f) * wanderJitter,
            Random.Range(-1f, 1f) * wanderJitter * 0.3f,
            Random.Range(-1f, 1f) * wanderJitter
        );
        wanderTarget = wanderTarget.normalized * wanderRadius;

        Vector3 localTarget = wanderTarget + new Vector3(0, 0, wanderDistance);
        Vector3 worldTarget = transform.TransformPoint(localTarget);

        return Seek(worldTarget) * 0.5f;
    }

    private Vector3 KeepInRoamArea()
    {
        Vector3 offset = transform.position - roamCenter;
        if (offset.magnitude > roamAreaRadius)
        {
            return Seek(roamCenter) * 1.5f;
        }
        return Vector3.zero;
    }

    private Vector3 Schooling()
    {
        Vector3 separation = Vector3.zero;
        Vector3 alignment = Vector3.zero;
        Vector3 cohesion = Vector3.zero;
        int count = 0;

        foreach (var other in allAgents)
        {
            if (other == this) continue;
            if (!string.IsNullOrEmpty(schoolTag) && other.schoolTag != schoolTag) continue;

            float dist = Vector3.Distance(transform.position, other.transform.position);
            if (dist > 0f && dist < neighborRadius)
            {
                count++;
                cohesion += other.transform.position;
                alignment += other.rb.linearVelocity;

                if (dist < separationRadius)
                {
                    Vector3 away = transform.position - other.transform.position;
                    separation += away.normalized / dist;
                }
            }
        }

        Vector3 result = Vector3.zero;
        if (count > 0)
        {
            cohesion = (cohesion / count) - transform.position;
            alignment = alignment / count;

            result += cohesion.normalized * cohesionWeight;
            result += alignment.normalized * alignmentWeight;
            result += separation.normalized * separationWeight;
        }
        return result;
    }

    private Vector3 AvoidObstacles()
    {
        Vector3 avoidance = Vector3.zero;
        Vector3 forward = rb.linearVelocity.sqrMagnitude > 0.01f ? rb.linearVelocity.normalized : transform.forward;

        for (int i = 0; i < whiskerCount; i++)
        {
            float t = whiskerCount == 1 ? 0f : (float)i / (whiskerCount - 1) - 0.5f; // -0.5..0.5
            float angle = t * whiskerSpreadAngle;
            Vector3 dir = Quaternion.AngleAxis(angle, transform.up) * forward;

            if (Physics.Raycast(transform.position, dir, out RaycastHit hit, avoidDistance, obstacleMask))
            {
                float strength = 1f - (hit.distance / avoidDistance);
                avoidance += hit.normal * strength;
            }
        }

        // Extra downward ray to specifically avoid terrain/seafloor
        if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit floorHit, avoidDistance, obstacleMask))
        {
            float strength = 1f - (floorHit.distance / avoidDistance);
            avoidance += Vector3.up * strength * 2f;
        }

        return avoidance;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, neighborRadius);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, separationRadius);
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(roamCenter, roamAreaRadius);
    }
}
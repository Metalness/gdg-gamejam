using System.Collections.Generic;
using UnityEngine;

public class CurrentCurve : MonoBehaviour
{
    [Header("Bezier Curve")]
    public Transform startPoint;
    public Transform handlePoint;
    public Transform endPoint;

    [Header("Current Settings")]
    public float currentRadius = 5f;
    public float currentForce = 10f;
    public ForceMode forceMode = ForceMode.Force;

    [Header("Flowing Objects")]
    public List<GameObject> flowingObjects = new List<GameObject>();
    public float objectMoveSpeed = 5f;

    private void FixedUpdate()
    {
        ApplyCurrentForce();
    }

    private void Update()
    {
        MoveFlowingObjects();
    }

    // --------------------------------------------------
    // Current force
    // --------------------------------------------------

    private void ApplyCurrentForce()
    {
        Collider[] colliders = Physics.OverlapSphere(
            transform.position,
            GetCurveBoundsRadius()
        );

        HashSet<Rigidbody> affectedBodies = new HashSet<Rigidbody>();

        foreach (Collider col in colliders)
        {
            Rigidbody rb = col.attachedRigidbody;

            if (rb == null || affectedBodies.Contains(rb))
                continue;

            Vector3 closestPoint;
            Vector3 tangent;
            float distance;

            GetClosestPointOnCurve(rb.worldCenterOfMass, out closestPoint, out tangent, out distance);

            if (distance <= currentRadius)
            {
                // Apply force along the direction of the curve
                rb.AddForce(tangent.normalized * currentForce, forceMode);
                affectedBodies.Add(rb);
            }
        }
    }

    // --------------------------------------------------
    // Flowing objects
    // --------------------------------------------------

private bool objectsInitialized = false;

private void Start()
{
    InitializeFlowingObjects();
}

private void InitializeFlowingObjects()
{
    int count = flowingObjects.Count;

    if (count == 0)
        return;

    for (int i = 0; i < count; i++)
    {
        GameObject obj = flowingObjects[i];

        if (obj == null)
            continue;

        CurrentObjectProgress progress = obj.GetComponent<CurrentObjectProgress>();

        if (progress == null)
            progress = obj.AddComponent<CurrentObjectProgress>();

        // Evenly space objects around the curve
        progress.t = i / (float)count;

        obj.transform.position = EvaluateBezier(progress.t);

        Vector3 tangent = EvaluateBezierTangent(progress.t);

        if (tangent.sqrMagnitude > 0.001f)
        {
            obj.transform.rotation = Quaternion.LookRotation(
                tangent.normalized,
                Vector3.up
            );
        }
    }

    objectsInitialized = true;
}

private void MoveFlowingObjects()
{
    if (!objectsInitialized)
        return;

    float curveLength = GetCurveLength();

    foreach (GameObject obj in flowingObjects)
    {
        if (obj == null)
            continue;

        CurrentObjectProgress progress =
            obj.GetComponent<CurrentObjectProgress>();

        if (progress == null)
            continue;

        progress.t += (objectMoveSpeed / curveLength) * Time.deltaTime;

        if (progress.t >= 1f)
            progress.t -= 1f;

        obj.transform.position = EvaluateBezier(progress.t);

        Vector3 tangent = EvaluateBezierTangent(progress.t);

        if (tangent.sqrMagnitude > 0.001f)
        {
            obj.transform.rotation = Quaternion.LookRotation(
                tangent.normalized,
                Vector3.up
            );
        }
    }
}

    // --------------------------------------------------
    // Quadratic Bezier
    // --------------------------------------------------

    public Vector3 EvaluateBezier(float t)
    {
        t = Mathf.Clamp01(t);

        Vector3 p0 = startPoint.position;
        Vector3 p1 = handlePoint.position;
        Vector3 p2 = endPoint.position;

        float u = 1f - t;

        return
            u * u * p0 +
            2f * u * t * p1 +
            t * t * p2;
    }

    public Vector3 EvaluateBezierTangent(float t)
    {
        t = Mathf.Clamp01(t);

        Vector3 p0 = startPoint.position;
        Vector3 p1 = handlePoint.position;
        Vector3 p2 = endPoint.position;

        return
            2f * (1f - t) * (p1 - p0) +
            2f * t * (p2 - p1);
    }

    // --------------------------------------------------
    // Find closest position on curve
    // --------------------------------------------------

    private void GetClosestPointOnCurve(
        Vector3 point,
        out Vector3 closestPoint,
        out Vector3 tangent,
        out float distance
    )
    {
        float bestT = 0f;
        float bestDistance = float.MaxValue;

        // Sample the curve to get a good initial approximation
        const int samples = 50;

        for (int i = 0; i <= samples; i++)
        {
            float t = i / (float)samples;

            Vector3 curvePoint = EvaluateBezier(t);
            float d = (point - curvePoint).sqrMagnitude;

            if (d < bestDistance)
            {
                bestDistance = d;
                bestT = t;
            }
        }

        // Refine around the best sample
        float range = 1f / samples;

        for (int i = 0; i < 5; i++)
        {
            float minT = Mathf.Clamp01(bestT - range);
            float maxT = Mathf.Clamp01(bestT + range);

            float middleT = (minT + maxT) * 0.5f;

            float d1 = (point - EvaluateBezier(minT)).sqrMagnitude;
            float d2 = (point - EvaluateBezier(middleT)).sqrMagnitude;
            float d3 = (point - EvaluateBezier(maxT)).sqrMagnitude;

            if (d1 < d2 && d1 < d3)
                bestT = minT;
            else if (d3 < d2)
                bestT = maxT;
            else
                bestT = middleT;

            range *= 0.5f;
        }

        closestPoint = EvaluateBezier(bestT);
        tangent = EvaluateBezierTangent(bestT);
        distance = Vector3.Distance(point, closestPoint);
    }

    // --------------------------------------------------
    // Curve length
    // --------------------------------------------------

    private float GetCurveLength()
    {
        float length = 0f;
        const int samples = 50;

        Vector3 previous = EvaluateBezier(0f);

        for (int i = 1; i <= samples; i++)
        {
            float t = i / (float)samples;
            Vector3 current = EvaluateBezier(t);

            length += Vector3.Distance(previous, current);
            previous = current;
        }

        return Mathf.Max(length, 0.01f);
    }

    private float GetCurveBoundsRadius()
    {
        float maxDistance = 0f;

        maxDistance = Mathf.Max(
            maxDistance,
            Vector3.Distance(transform.position, startPoint.position)
        );

        maxDistance = Mathf.Max(
            maxDistance,
            Vector3.Distance(transform.position, handlePoint.position)
        );

        maxDistance = Mathf.Max(
            maxDistance,
            Vector3.Distance(transform.position, endPoint.position)
        );

        return maxDistance + currentRadius;
    }

    // --------------------------------------------------
    // Debug visualization
    // --------------------------------------------------

    private void OnDrawGizmos()
    {
        if (startPoint == null || handlePoint == null || endPoint == null)
            return;

        const int segments = 30;

        Gizmos.color = Color.cyan;

        Vector3 previous = EvaluateBezier(0f);

        for (int i = 1; i <= segments; i++)
        {
            float t = i / (float)segments;

            Vector3 current = EvaluateBezier(t);

            Gizmos.DrawLine(previous, current);

            previous = current;
        }

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(startPoint.position, currentRadius);
        Gizmos.DrawWireSphere(endPoint.position, currentRadius);

        // Handle
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(handlePoint.position, 0.2f);

        Gizmos.color = Color.gray;
        Gizmos.DrawLine(startPoint.position, handlePoint.position);
        Gizmos.DrawLine(handlePoint.position, endPoint.position);
    }
}


// Stores movement progress for each flowing object
public class CurrentObjectProgress : MonoBehaviour
{
    [Range(0f, 1f)]
    public float t = 0f;
}
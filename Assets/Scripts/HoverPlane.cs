using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DebugHelper;

public class HoverPlane : MonoBehaviour
{
    [SerializeField] float maxHoverThrust = 10f;
    [SerializeField] float targetHeight = 2f;
    [SerializeField] float stabalizeForce = 100f;

    [SerializeField] int divisionCount = 4;

    [SerializeField] Vector3 planeSize;

    HoverPoint bestHoverPoint;
    HoverPoint [] points;

    [SerializeField] HoverPoint hoverPointPrefab;

    Rigidbody rb;

    private void Awake()
    {
        rb = GetComponentInParent<Rigidbody>();

        rb.centerOfMass = new Vector3(0, 0, 0);
    }

    private void FixedUpdate()
    {
        ApplyHoverForce();
        Stabalize(Vector3.up);
    }

    private void UpdateHoverPoints()
    {
        int pointsRequired = divisionCount * divisionCount;
        if (points.Length == pointsRequired)
        {
            return;
        }

        Vector3 min = -planeSize / 2;
        Vector3 max = planeSize / 2;
        Vector3 range = max - min;

        Vector3 segmentBounds = range / divisionCount;

        for (int i = 0; i < divisionCount; i++)
        {
            float xPos = ((i * segmentBounds.x) + ((i + 1) * segmentBounds.x)) / 2;

            for (int j = 0; j < divisionCount; j++)
            {
                HoverPoint point = points[i * divisionCount + j];

                if (point == bestHoverPoint)
                {
                    continue;
                }

                float zPos = ((j * segmentBounds.z) + ((j + 1) * segmentBounds.z)) / 2;

                point.transform.localPosition = new Vector3(min.x + xPos, min.y, min.z + zPos);
            }
        }
    }

    private void ApplyHoverForce()
    {
        UpdateHoverPoints();

        if (bestHoverPoint != null)
        {
            bestHoverPoint.Recalculate(targetHeight);
        }

        for (int i = 0; i < points.Length; i++)
        { 
            HoverPoint point = points[i];
            if (point == bestHoverPoint) continue;

            bool isHit = Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, targetHeight, LayerMask.GetMask("Terrain"));

            if (!isHit)
            {
                DistanceFromGround = Mathf.Infinity;
                HitPos = transform.position + Vector3.down * maxDistance;
            }
            else
            {
                DistanceFromGround = hit.distance;
                HitPos = hit.point;
            }

            if (bestHoverPoint == null || point.DistanceFromGround < bestHoverPoint.DistanceFromGround)
            {
                bestHoverPoint = point;
            }
        }

        if (bestHoverPoint.DistanceFromGround != Mathf.Infinity)
        {
            rb.AddForce(maxHoverThrust * (1f - (bestHoverPoint.DistanceFromGround / targetHeight)) * Vector3.up, ForceMode.Acceleration);
        }
    }

    private void Stabalize(Vector3 groundNormal)
    {
        var cross = Vector3.Cross(transform.up, groundNormal);

        var turnDir = transform.InverseTransformDirection(cross);

        rb.AddTorque(transform.forward * turnDir.z * stabalizeForce);
    }

    private void OnDrawGizmos()
    {
        if (points == null)
        {
            return;
        }

        for (int i = 0; i < points.Length; i++)
        {
            HoverPoint point = points[i];
            if (point == bestHoverPoint) continue;

            if (point.DistanceFromGround != Mathf.Infinity)
            {
                Gizmos.color = Color.yellow;
            }
            else
            {
                Gizmos.color = Color.white;
            }

            Gizmos.DrawSphere(point.HitPos, 1f);
            Gizmos.DrawLine(point.transform.position, point.HitPos);
        }

        if (bestHoverPoint.DistanceFromGround != Mathf.Infinity)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(bestHoverPoint.HitPos, 1.1f);
            Gizmos.DrawLine(bestHoverPoint.transform.position, bestHoverPoint.HitPos);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Matrix4x4 currentMatrix = Gizmos.matrix;
        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.DrawCube(Vector3.zero, planeSize);
        Gizmos.matrix = currentMatrix;
    }
}

public struct HoverPoint
{
    public Vector3 position;
    public Vector3 hitPos;
    public float distanceFromGround;
}
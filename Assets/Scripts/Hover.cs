using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DebugHelper;

public class Hover : MonoBehaviour
{
    [SerializeField] float maxHoverThrust = 10f;
    [SerializeField] float targetHeight = 2f;
    [SerializeField] float stabalizeForce = 100f;

    [SerializeField] int divisionCount = 4;

    [SerializeField] Vector3 hoverSize;

    HoverPoint bestHoverPoint;
    HoverPoint [,] points;

    [SerializeField] HoverPoint hoverPointPrefab;

    GizmoHelper gizmoHelper;
    Rigidbody rb;

    private void Awake()
    {
        rb = GetComponentInParent<Rigidbody>();
        gizmoHelper = FindObjectOfType<GizmoHelper>();

        rb.centerOfMass = new Vector3(0, 0, 0);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawCube(transform.position, hoverSize); 
    }

    private void FixedUpdate()
    {
        ApplyHoverForce();
        Stabalize(Vector3.up);
    }

    private void ScrambleHoverPoints()
    {
        Vector3 min = -hoverSize / 2;
        Vector3 max = hoverSize / 2;
        Vector3 range = max - min;

        Vector3 part = range / divisionCount;

        if (points == null)
        {
            points = new HoverPoint[divisionCount, divisionCount];
            bestHoverPoint = null;
        }
        else if (points.GetLength(0) != divisionCount)
        {
            points = new HoverPoint[divisionCount, divisionCount];

            for (int i = 0; i < divisionCount; i++)
            {
                for (int j = 0; j < divisionCount; j++)
                {
                    HoverPoint point = points[i,j];
                    if (point != null)
                    {
                        Destroy(point);
                    }
                }
            }

            bestHoverPoint = null;
        }

        for (int i = 0; i < divisionCount; i++)
        {
            float randomX = Random.Range(i * part.x, (i + 1) * part.x);

            for (int j = 0; j < divisionCount; j++)
            {
                if (points[i, j] == null)
                {
                    points[i, j] = Instantiate(hoverPointPrefab, transform);
                }
                else if (points[i, j] == bestHoverPoint)
                {
                    continue;
                }

                float randomZ = Random.Range(j * part.z, (j + 1) * part.z);

                points[i, j].transform.localPosition = new Vector3(min.x + randomX, min.y, min.z + randomZ);
            }
        }
    }

    private void ApplyHoverForce()
    {
        ScrambleHoverPoints();

        if (bestHoverPoint != null)
        {
            bestHoverPoint.Recalculate(targetHeight);
        }

        for (int i = 0; i < points.GetLength(0); i++)
        {
            for (int j = 0; j < points.GetLength(1); j++)
            {
                HoverPoint point = points[i, j];
                if (point == bestHoverPoint) continue;

                point.Recalculate(targetHeight);

                if (point.DistanceFromGround != Mathf.Infinity)
                {
                    gizmoHelper.Colour = Color.white;
                    gizmoHelper.DrawSphere(point.HitPos, 1f);
                    gizmoHelper.DrawLine(point.transform.position, point.HitPos);
                }               

                if (bestHoverPoint == null || point.DistanceFromGround < bestHoverPoint.DistanceFromGround)
                {
                    bestHoverPoint = point;
                }
            }
                
        }

        if (bestHoverPoint.DistanceFromGround != Mathf.Infinity)
        {
            rb.AddForce(maxHoverThrust * (1f - (bestHoverPoint.DistanceFromGround / targetHeight)) * Vector3.up, ForceMode.Acceleration);

            gizmoHelper.Colour = Color.green;
            gizmoHelper.DrawSphere(bestHoverPoint.HitPos, 1.1f);
            gizmoHelper.DrawLine(bestHoverPoint.transform.position, bestHoverPoint.HitPos);
        }
    }

    private void Stabalize(Vector3 groundNormal)
    {
        var cross = Vector3.Cross(transform.up, groundNormal);

        var turnDir = transform.InverseTransformDirection(cross);

        rb.AddTorque(transform.forward * turnDir.z * stabalizeForce);
    }
}
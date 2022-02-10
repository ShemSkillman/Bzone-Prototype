using System.Collections.Generic;
using UnityEngine;

namespace HoverSystem
{
    public class HoverPlane : MonoBehaviour
    {
        [SerializeField] float maxHoverThrust = 10f;
        [SerializeField] float targetHeight = 2f;
        [SerializeField] float stabalizeForce = 100f;

        [SerializeField] int divisionCount = 4;

        [SerializeField] Vector3 planeSize;

        HoverPoint bestHoverPoint;
        HoverPoint[,] points;

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
            if (points != null && points.GetLength(0) == divisionCount)
            {
                return;
            }

            Vector3 min = -planeSize / 2;
            Vector3 max = planeSize / 2;
            Vector3 range = max - min;

            Vector3 segmentBounds = range / divisionCount;

            points = new HoverPoint[divisionCount, divisionCount];
            List<HoverPoint> reserves = new List<HoverPoint>(GetComponentsInChildren<HoverPoint>());

            bestHoverPoint = null;

            for (int i = 0; i < divisionCount; i++)
            {
                float xPos = ((i * segmentBounds.x) + ((i + 1) * segmentBounds.x)) / 2;

                for (int j = 0; j < divisionCount; j++)
                {
                    HoverPoint point;
                    if (reserves.Count < 1)
                    {
                        point = Instantiate(hoverPointPrefab, transform);
                    }
                    else
                    {
                        point = reserves[0];
                        reserves.RemoveAt(0);
                    }

                    float zPos = ((j * segmentBounds.z) + ((j + 1) * segmentBounds.z)) / 2;

                    point.transform.localPosition = new Vector3(min.x + xPos, min.y, min.z + zPos);

                    points[i, j] = point;
                }
            }

            while (reserves.Count > 0)
            {
                HoverPoint toDestroy = reserves[0];
                reserves.RemoveAt(0);
                Destroy(toDestroy.gameObject);
            }
        }

        private void ApplyHoverForce()
        {
            UpdateHoverPoints();

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

                    if (bestHoverPoint == null || point.DistanceFromGround < bestHoverPoint.DistanceFromGround)
                    {
                        bestHoverPoint = point;
                    }
                }
            }

            if (bestHoverPoint == null)
            {
                return;
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

            for (int i = 0; i < points.GetLength(0); i++)
            {
                for (int j = 0; j < points.GetLength(1); j++)
                {
                    HoverPoint point = points[i, j];

                    Gizmos.color = Color.white;
                    Gizmos.DrawLine(point.transform.position, point.HitPos);

                    if (point == bestHoverPoint && point.DistanceFromGround != Mathf.Infinity)
                    {
                        Gizmos.color = Color.green;
                    }
                    else if (point.DistanceFromGround != Mathf.Infinity)
                    {
                        Gizmos.color = Color.yellow;
                    }

                    Gizmos.DrawSphere(point.HitPos, 1f);
                }
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
}
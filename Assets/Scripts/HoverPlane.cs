﻿using System.Collections.Generic;
using UnityEngine;

namespace HoverSystem
{
    public class HoverPlane : MonoBehaviour
    {
        [SerializeField] float maxHoverThrust = 10f;
        [SerializeField] float targetHeight = 2f;

        [Header("Plane Settings")]
        [SerializeField] Vector3 planeSize;
        [SerializeField] int divisionCount = 4;
        [SerializeField] Color planeColor = Color.cyan;

        [Header("Stabalization")]
        [SerializeField] float stabalizeForce = 100f;
        [SerializeField] bool stabalizeZ = true;
        [SerializeField] bool stabalizeX = false;

        HoverPoint bestHoverPoint;
        HoverPoint[,] points;

        Rigidbody rb;

        private void Awake()
        {
            rb = GetComponentInParent<Rigidbody>();
            rb.centerOfMass = new Vector3(0, 0, 0);

            GenerateHoverPoints();
        }

        private void FixedUpdate()
        {
            // Hover points must be regenerated if division count is changed
            if (points.GetLength(0) != divisionCount)
            {
                GenerateHoverPoints();
            }

            FindBestHoverPoint();
            ApplyHoverForce();
            Stabalize(Vector3.up);
        }

        private void GenerateHoverPoints()
        {
            Vector3 min = -planeSize / 2;
            Vector3 max = planeSize / 2;
            Vector3 range = max - min;

            Vector3 segmentBounds = range / divisionCount;

            points = new HoverPoint[divisionCount, divisionCount];

            // Reuse hoverpoints if any are available
            List<HoverPoint> reserves = new List<HoverPoint>(GetComponentsInChildren<HoverPoint>());

            bestHoverPoint = null;

            // Creates hover points when needed
            // Positions each point in a grid-like fashion on the plane
            for (int i = 0; i < divisionCount; i++)
            {
                float xPos = ((i * segmentBounds.x) + ((i + 1) * segmentBounds.x)) / 2;

                for (int j = 0; j < divisionCount; j++)
                {
                    HoverPoint point;
                    if (reserves.Count < 1)
                    {
                        GameObject instance = new GameObject("Auto Generated Hover Point");
                        instance.transform.parent = transform;
                        point = instance.AddComponent<HoverPoint>();
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

            // Detroy extra hover points that are not in use
            while (reserves.Count > 0)
            {
                HoverPoint toDestroy = reserves[0];
                reserves.RemoveAt(0);
                Destroy(toDestroy.gameObject);
            }
        }

        // The 'best' hover point is one which has the closest hit distance to the ground
        // It determines the height of the object above the ground
        private void FindBestHoverPoint()
        {
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
        }

        private void ApplyHoverForce()
        {
            if (bestHoverPoint == null)
            {
                return;
            }

            if (bestHoverPoint.DistanceFromGround != Mathf.Infinity)
            {
                rb.AddForce(maxHoverThrust * (1f - (bestHoverPoint.DistanceFromGround / targetHeight)) * Vector3.up, ForceMode.Acceleration);
            }
        }

        // Tries to keep object level with the ground
        private void Stabalize(Vector3 groundNormal)
        {
            if (!stabalizeX && !stabalizeZ)
            {
                return;
            }

            var cross = Vector3.Cross(transform.up, groundNormal);

            var turnDir = transform.InverseTransformDirection(cross);

            if (stabalizeZ)
            {
                rb.AddTorque(transform.forward * turnDir.z * stabalizeForce);
            }
            
            if (stabalizeX)
            {
                rb.AddTorque(transform.right * turnDir.x * stabalizeForce);
            }            
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
            Gizmos.color = planeColor;
            Matrix4x4 currentMatrix = Gizmos.matrix;
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawCube(Vector3.zero, planeSize);
            Gizmos.matrix = currentMatrix;
        }
    }
}
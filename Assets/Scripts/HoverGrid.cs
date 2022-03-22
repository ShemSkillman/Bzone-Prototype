using System.Collections.Generic;
using UnityEngine;

namespace HoverSystem
{
    [ExecuteInEditMode]
    public class HoverGrid : MonoBehaviour
    {
        [Header("Hover Settings")]
        [SerializeField] float maxHoverThrust = 30f;
        [SerializeField] float targetHeight = 4f;
        public float TargetHeight { get { return targetHeight; } }
        [SerializeField] LayerMask hoverableLayers = 1;

        [Header("Grid Settings")]
        [SerializeField] Vector3 gridSize = new Vector3(10, 0.2f, 10);

        [Range(1, 100)]
        [SerializeField] int colCount = 3;

        [Range(1, 100)]
        [SerializeField] int rowCount = 3;

        [SerializeField] Color gridColour = new Color(0, 1, 1, 0.5f);

        [Header("Stabalization")]
        [SerializeField] float stabalizeForce = 100f;
        [SerializeField] bool stabalizeZ = true;
        [SerializeField] bool stabalizeX = false;

        [Header("Gizmo Settings")]
        [SerializeField] bool alwaysRenderGizmos = false;

        HoverPoint bestHoverPoint;
        HoverPoint[,] points;
        Vector3 usedGridSize;
        Vector3 gridSquareBounds;

        Rigidbody rb;

        private void Awake()
        {
            rb = GetComponentInParent<Rigidbody>();
            if (rb == null)
            {
                Debug.LogError(nameof(HoverGrid) + " component could not find a " + nameof(Rigidbody) + " component on the gameobject " + gameObject.name +
                    " or on any of its parent gameobjects. Please add a " + nameof(Rigidbody) + " component so that physics can be applied.");
                return;
            }
            rb.centerOfMass = Vector3.zero;

            GenerateHoverPoints();
        }

        private void FixedUpdate()
        {
            if (rb == null)
            {
                return;
            }

            FindBestHoverPoint();
            ApplyHoverForce();
            Stabalize(Vector3.up);
        }

        private void Update()
        {
            // Hover points must be regenerated if division count is changed
            if (points == null ||
                points.GetLength(0) != colCount ||
                points.GetLength(1) != rowCount ||
                usedGridSize != gridSize ||
                points[0,0].HoverableLayers.value != hoverableLayers.value)
            {
                GenerateHoverPoints();
            }

            if (Application.isEditor)
            {
                FindBestHoverPoint();
            }
        }

        private void GenerateHoverPoints()
        {
            Vector3 min = -gridSize / 2;
            Vector3 max = gridSize / 2;
            Vector3 range = max - min;

            gridSquareBounds = new Vector3(range.x / colCount, gridSize.y, range.z / rowCount);

            points = new HoverPoint[colCount, rowCount];

            // Reuse hoverpoints if any are available
            List<HoverPoint> reserves = new List<HoverPoint>(GetComponentsInChildren<HoverPoint>());

            bestHoverPoint = null;

            // Creates hover points when needed
            // Positions each point in a grid-like fashion on the plane
            for (int i = 0; i < colCount; i++)
            {
                float xPos = ((i * gridSquareBounds.x) + ((i + 1) * gridSquareBounds.x)) / 2;

                for (int j = 0; j < rowCount; j++)
                {
                    HoverPoint point;
                    if (reserves.Count < 1)
                    {
                        GameObject instance = new GameObject("Auto Generated Hover Point");
                        instance.transform.rotation = transform.rotation;
                        instance.transform.parent = transform;
                        point = instance.AddComponent<HoverPoint>();
                    }
                    else
                    {
                        point = reserves[0];
                        reserves.RemoveAt(0);
                    }

                    point.HoverableLayers = hoverableLayers;

                    float zPos = ((j * gridSquareBounds.z) + ((j + 1) * gridSquareBounds.z)) / 2;

                    point.transform.localPosition = new Vector3(min.x + xPos, 0.0f, min.z + zPos);

                    points[i, j] = point;
                }
            }

            // Detroy extra hover points that are not in use
            while (reserves.Count > 0)
            {
                HoverPoint toDestroy = reserves[0];
                reserves.RemoveAt(0);
                DestroyImmediate(toDestroy.gameObject);
            }

            usedGridSize = gridSize;
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
            if (alwaysRenderGizmos)
            {
                RenderGizmos();
            }            
        }

        private void OnDrawGizmosSelected()
        {
            if (!alwaysRenderGizmos)
            {
                RenderGizmos();
            }
        }

        private void RenderGizmos()
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

                    DrawRaycastLine(point);
                    DrawHitSphere(point);
                }
            }

            for (int i = 0; i < points.GetLength(0); i++)
            {
                for (int j = 0; j < points.GetLength(1); j++)
                {
                    HoverPoint point = points[i, j];
                    DrawGridSquare(point);
                }
            }
        }        

        private void DrawRaycastLine(HoverPoint point)
        {
            Gizmos.color = Color.white;
            Gizmos.DrawLine(point.transform.position, point.HitPos);
        }

        private void DrawHitSphere(HoverPoint point)
        {
            Gizmos.color = Color.white;
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

        private void DrawGridSquare(HoverPoint point)
        {
            Gizmos.color = gridColour;
            Matrix4x4 currentMatrix = Gizmos.matrix;
            Gizmos.matrix = point.transform.localToWorldMatrix;
            Gizmos.DrawCube(Vector3.zero, gridSquareBounds * 0.95f);
            Gizmos.matrix = currentMatrix;
        }
    }
}
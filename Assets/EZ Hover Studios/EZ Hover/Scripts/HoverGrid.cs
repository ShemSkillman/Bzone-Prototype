using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace EZHover
{
    [ExecuteInEditMode]
    public class HoverGrid : MonoBehaviour
    {
        [Header("Hover Settings")]
        [SerializeField] private float maxHoverThrust = 30f;
        public float MaxHoverThrust { get { return maxHoverThrust; } set { maxHoverThrust = value; } }

        [SerializeField] private float targetHeight = 4f;
        public float TargetHeight { get { return targetHeight; } set { targetHeight = value; } }

        [SerializeField] private LayerMask hoverableLayers = 1;
        public LayerMask HoverableLayers { get { return hoverableLayers; } set { hoverableLayers = value; }}

        [Header("Grid Settings")]
        [SerializeField] private Vector3 gridSize = new Vector3(10, 0.2f, 10);
        public Vector3 GridSize { get { return gridSize; } set { gridSize = value; } }

        [Range(1, 100)]
        [SerializeField] private int columnCount = 3;
        public int ColumnCount { get { return columnCount; } set { columnCount = value; } }

        [Range(1, 100)]
        [SerializeField] private int rowCount = 3;
        public int RowCount { get { return rowCount; } set { rowCount = value; } }

        [Header("Stabalization")]

        [Tooltip("Amount of torque force applied to stabalize the hover object.")]
        [SerializeField] private float stabalizeForce = 10f;
        public float StabalizeForce { get { return stabalizeForce; } set { stabalizeForce = value; } }

        [SerializeField] private bool stabalizeZ = true;
        public bool StabalizeZ { get { return stabalizeZ; } set { stabalizeZ = value; } }

        [SerializeField] private bool stabalizeX = false;
        public bool StabalizeX { get { return stabalizeX; } set { stabalizeX = value; } }

        [Header("Gizmo Settings")]
        [SerializeField] bool alwaysRenderGizmos = false;

        [SerializeField] Color gridColour = new Color(0, 1, 1, 0.5f);

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
                Debug.LogError(nameof(HoverGrid) + " component could not find a rigidbody component on the gameobject " + gameObject.name +
                    " or on any of its parent gameobjects. Please add a rigidbody component so that physics can be applied.");
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
            if (IsPrefab())
                return;

            // Hover points must be regenerated if division count is changed
            if (points == null ||
                points.GetLength(0) != columnCount ||
                points.GetLength(1) != rowCount ||
                points[0, 0] == null ||
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

        private bool IsPrefab()
        {
            return gameObject.scene.path == "";
        }

        private void GenerateHoverPoints()
        {
            Vector3 min = -gridSize / 2;
            Vector3 max = gridSize / 2;
            Vector3 range = max - min;

            gridSquareBounds = new Vector3(range.x / columnCount, gridSize.y, range.z / rowCount);

            points = new HoverPoint[columnCount, rowCount];

            // Reuse hoverpoints if any are available
            List<HoverPoint> reserves = new List<HoverPoint>(GetComponentsInChildren<HoverPoint>());

            bestHoverPoint = null;

            // Creates hover points when needed
            // Positions each point in a grid-like fashion on the plane
            for (int i = 0; i < columnCount; i++)
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
                bestHoverPoint.Recalculate(targetHeight, rb);
            }

            for (int i = 0; i < points.GetLength(0); i++)
            {
                for (int j = 0; j < points.GetLength(1); j++)
                {
                    HoverPoint point = points[i, j];
                    if (point == null )
                    {
                        continue;
                    }

                    if (point == bestHoverPoint) continue;

                    point.Recalculate(targetHeight, rb);

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
                rb.AddTorque(transform.forward * turnDir.z * stabalizeForce * rb.mass);
            }
            
            if (stabalizeX)
            {
                rb.AddTorque(transform.right * turnDir.x * stabalizeForce * rb.mass);
            }            
        }

        public Vector3 GetDirectionPointOnGridBounds(Vector3 dir)
        {
            return transform.position + new Vector3(dir.x * (gridSize.x / 2), 0.0f, dir.z * (gridSize.z / 2));
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
            if (points == null || IsPrefab())
            {
                return;
            }

            for (int i = 0; i < points.GetLength(0); i++)
            {
                for (int j = 0; j < points.GetLength(1); j++)
                {
                    HoverPoint point = points[i, j];
                    if (point == null)
                    {
                        continue;
                    }

                    DrawRaycastLine(point);
                    DrawHitSphere(point);
                }
            }

            for (int i = 0; i < points.GetLength(0); i++)
            {
                for (int j = 0; j < points.GetLength(1); j++)
                {
                    HoverPoint point = points[i, j];
                    if (point == null)
                    {
                        continue;
                    }

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
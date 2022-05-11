using UnityEngine;

namespace EZHover
{
    public class HoverMovement : MonoBehaviour
    {
        [Header("Movement Settings")]

        [SerializeField] private bool enableInput = true;
        public bool EnableInput { get { return enableInput; } set { enableInput = value; } }        

        [SerializeField] private float moveSpeed = 10f;
        public float MoveSpeed { get { return moveSpeed; } set { moveSpeed = value; } }

        [Header("Obstacle Avoidance")]

        [SerializeField] private bool enableObstacleAvoidance = true;

        [SerializeField] LayerMask obstacleLayers = 1;
        public LayerMask ObstacleLayers { get { return obstacleLayers; } }

        [Tooltip("Amount of vertical force applied to rise above oncoming obstacles.")]
        [SerializeField] private float hoverBoost = 30f;
        public float HoverBoost { get { return hoverBoost; } set { hoverBoost = value; } }

        [Tooltip("Detection distance to oncoming obstacle before applying hover boost and repulsion.")]
        [SerializeField] private float obstacleDetectionRange = 10f;
        public float ObstacleDetectionRange { get { return obstacleDetectionRange; } set { obstacleDetectionRange = value; } }

        [Tooltip("Amount of repulsion force applied to prevent collision with oncoming obstacle within detection range.")]
        [SerializeField] private float repulsionSpeed = 10f;
        public float RepulsionSpeed { get { return repulsionSpeed; } set { repulsionSpeed = value; } }

        [Header("Gizmo Settings")]
        [SerializeField] bool drawMoveDirectionLine = true;

        Rigidbody rb;
        HoverGrid hoverGrid;

        Vector3 moveDir;

        private void Awake()
        {
            rb = GetComponentInParent<Rigidbody>();
            if (rb == null)
            {
                Debug.LogError(nameof(HoverMovement) + " component could not find a rigidbody component on the gameobject " + gameObject.name +
                    " or on any of its parent gameobjects. Please add a rigidbody component so that physics can be applied.");
                return;
            }
            else
            {
                rb.centerOfMass = Vector3.zero;
            }

            hoverGrid = GetComponent<HoverGrid>();
            if (hoverGrid == null)
            {
                Debug.LogError(nameof(HoverMovement) + " component could not find a " + nameof(HoverGrid) + " component on the gameobject " + gameObject.name
                    + ". Please add a " + nameof(HoverGrid) + " component so that movement can occur.");
            }
        }

        private void FixedUpdate()
        {
            if (hoverGrid == null || rb == null || !enableInput)
            {
                return;
            }

            Vector3 moveDir = GetMoveDirection();

            Vector3 start = hoverGrid.GetDirectionPointOnGridBounds(moveDir);

            if (drawMoveDirectionLine)
            {
                Debug.DrawLine(start, start + (moveDir * obstacleDetectionRange), Color.red);
            }
            
            if (!enableObstacleAvoidance)
            {
                rb.AddForce(moveDir * moveSpeed, ForceMode.Acceleration);
                return;
            }

            bool isHit = Physics.Raycast(start, moveDir, out RaycastHit hit, obstacleDetectionRange, obstacleLayers);

            if (isHit)
            {
                // Apply more repulsion and hover boost when closer to obstacle
                float closenessMult = (1 - (hit.distance / obstacleDetectionRange));

                Vector3 moveForce = moveDir * moveSpeed;

                // More repulsion force applied when facing a steep incline
                float steepnessMult = 1- Mathf.Clamp(Vector3.Dot(hit.normal, Vector3.up), 0.0f, 1.0f);

                Vector3 repulsionForce = -moveDir * closenessMult * repulsionSpeed * steepnessMult;
                Vector3 hoverForce = new Vector3(0, hoverBoost * closenessMult, 0);

                rb.AddForce(repulsionForce + hoverForce + moveForce, ForceMode.Acceleration);
            }
            else
            {
                rb.AddForce(moveDir * moveSpeed, ForceMode.Acceleration);
            }

        }
        private Vector3 GetMoveDirection()
        {
            Vector3 verticalDir = new Vector3(transform.forward.x, 0f, transform.forward.z).normalized;
            Vector3 horizontalDir = new Vector3(transform.right.x, 0f, transform.right.z).normalized;

            Vector3 verticalForce = verticalDir * moveDir.y;
            Vector3 horizontalForce = horizontalDir * moveDir.x;

            moveDir = Vector3.zero;

            return verticalForce + horizontalForce;
        }

        public void Move(Vector2 moveDirection)
        {
            if (!enableInput)
            {
                return;
            }

            moveDir = moveDirection.normalized;
        }
    }
}
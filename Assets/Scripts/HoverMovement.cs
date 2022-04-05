using UnityEngine;

namespace HoverSystem
{
    public class HoverMovement : MonoBehaviour
    {
        [SerializeField] float moveSpeed = 10f;

        [Header("Obstacle Avoidance")]

        [Tooltip("Amount of vertical force applied to rise above oncoming obstacles.")]
        [SerializeField] float hoverBoost = 30f;

        [Tooltip("Detection distance to oncoming obstacle before applying hover boost and repulsion.")]
        [SerializeField] float obstacleDetectionRange = 10f;

        [Tooltip("Amount of repulsion force applied to prevent collision with oncoming obstacle within detection range.")]
        [SerializeField] float repulsionSpeed = 10f;

        Rigidbody rb;
        HoverGrid hoverGrid;

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
            if (hoverGrid == null || rb == null)
            {
                return;
            }

            Vector3 moveDir = GetMoveDirection();

            Vector3 start = hoverGrid.GetDirectionPointOnGridBounds(moveDir);

            Debug.DrawLine(start, start + (moveDir * obstacleDetectionRange), Color.red);

            bool isHit = Physics.Raycast(start, moveDir, out RaycastHit hit, obstacleDetectionRange, hoverGrid.HoverableLayers);

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

            Vector3 verticalForce = verticalDir * Input.GetAxis("Vertical");
            Vector3 horizontalForce = horizontalDir * Input.GetAxis("Horizontal");

            return verticalForce + horizontalForce;
        }
    }
}
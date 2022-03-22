using UnityEngine;

namespace HoverSystem
{
    public class HoverMovement : MonoBehaviour
    {
        [SerializeField] float moveSpeed = 10f;

        [SerializeField] float verticalSpeed = 40f;

        [SerializeField] float terrainDetectionRange = 30f;
        [SerializeField] float terrainRepulsionDistance = 15f;
        [SerializeField] float repulsionSpeed = 5f;

        Rigidbody rb;
        HoverGrid hoverGrid;
        LayerMask terrainLayer;

        private void Awake()
        {
            rb = GetComponentInParent<Rigidbody>();

            hoverGrid = GetComponent<HoverGrid>();
            if (hoverGrid == null)
            {
                Debug.LogError(nameof(HoverMovement) + " component could not find a " + nameof(HoverGrid) + " component on the gameobject " + gameObject.name
                    + ". Please add a " + nameof(HoverGrid) + " component so that movement can occur.");
                return;
            }
            rb.centerOfMass = Vector3.zero;

            terrainLayer = LayerMask.GetMask("Terrain");
        }

        private void FixedUpdate()
        {
            if (hoverGrid == null || rb == null)
            {
                return;
            }

            Vector3 start = transform.position;

            Vector3 moveForce = GetMoveForce();

            Debug.DrawLine(start, start + (moveForce * terrainDetectionRange), Color.red);

            bool isHit = Physics.Raycast(start, moveForce, out RaycastHit hit, terrainDetectionRange, terrainLayer);

            if (isHit)
            {
                Vector3 target = hit.point + (Vector3.up * hoverGrid.TargetHeight);

                Vector3 targetDir = target - start;
                targetDir.Normalize();

                if (hit.distance <= terrainRepulsionDistance)
                {
                    float horizontalRepulsionMult = (1 - (hit.distance / terrainRepulsionDistance)) * repulsionSpeed;

                    targetDir = new Vector3(-targetDir.x * horizontalRepulsionMult, 0f, -targetDir.z * horizontalRepulsionMult);
                }
                else
                {
                    float horizontalMoveMult = ((hit.distance - terrainRepulsionDistance) / (terrainDetectionRange - terrainRepulsionDistance)) * moveSpeed;

                    targetDir = new Vector3(targetDir.x * horizontalMoveMult, 0f, targetDir.z * horizontalMoveMult);
                }

                hoverGrid.ApplyMaxHoverForce = true;

                rb.AddForce(targetDir, ForceMode.Force);
            }
            else
            {
                hoverGrid.ApplyMaxHoverForce = false;
                rb.AddForce(moveForce * moveSpeed, ForceMode.Force);
            }

        }

        private Vector3 GetMoveForce()
        {
            Vector3 verticalDir = new Vector3(transform.forward.x, 0f, transform.forward.z).normalized;
            Vector3 horizontalDir = new Vector3(transform.right.x, 0f, transform.right.z).normalized;

            Vector3 verticalForce = verticalDir * Input.GetAxis("Vertical");
            Vector3 horizontalForce = horizontalDir * Input.GetAxis("Horizontal");

            return verticalForce + horizontalForce;
        }
    }
}
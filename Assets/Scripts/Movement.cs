using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DebugHelper;

public class Movement : MonoBehaviour
{
    [SerializeField] float speed = 10f;
    [SerializeField] float terrainDetectionRange = 30f;
    [SerializeField] float targetHeight = 2f;
    [SerializeField] float terrainRepulsionDistance = 15f;
    [SerializeField] float repulsionSpeed = 5f;

    Rigidbody rb;
    MeshCollider meshCollider;
    LayerMask terrainLayer;

    GizmoHelper gizmoHelper;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        meshCollider = GetComponent<MeshCollider>();
        terrainLayer = LayerMask.GetMask("Terrain");

        gizmoHelper = FindObjectOfType<GizmoHelper>();
    }

    private void FixedUpdate()
    {
        Vector3 lowestPoint = meshCollider.bounds.min;
        Vector3 start = new Vector3(transform.position.x, lowestPoint.y, transform.position.z);

        Vector3 moveForce = GetMoveForce();

        Debug.DrawLine(start, start + (moveForce * terrainDetectionRange), Color.red);

        bool isHit = Physics.Raycast(start, moveForce, out RaycastHit hit, terrainDetectionRange, terrainLayer);

        if (isHit)
        { 
            Vector3 target = hit.point + (Vector3.up * targetHeight);

            gizmoHelper.Colour = Color.yellow;
            gizmoHelper.DrawSphere(target, 1f);

            Vector3 targetDir = target - start;
            targetDir.Normalize();

            if (hit.distance <= terrainRepulsionDistance)
            {
                float horizontalRepulsionMult = (1 - (hit.distance / terrainRepulsionDistance)) * repulsionSpeed;

                targetDir = new Vector3(-targetDir.x * horizontalRepulsionMult, targetDir.y * speed * (hit.distance / terrainRepulsionDistance), -targetDir.z * horizontalRepulsionMult);
            }
            else
            {
                float horizontalMoveMult = (hit.distance - terrainRepulsionDistance) / (terrainDetectionRange - terrainRepulsionDistance);

                targetDir = new Vector3(targetDir.x * horizontalMoveMult, targetDir.y, targetDir.z * horizontalMoveMult);

                targetDir *= speed;
            }

            rb.AddForce(targetDir, ForceMode.Force);
        }
        else
        {
            rb.AddForce(moveForce * speed, ForceMode.Force);
        }
        
    }

    private Vector3 GetMoveForce()
    {
        Vector3 verticalDir = new Vector3(transform.forward.x, 0f, transform.forward.z);
        Vector3 horizontalDir = new Vector3(transform.right.x, 0f, transform.right.z);

        Vector3 verticalForce = verticalDir * Input.GetAxis("Vertical");
        Vector3 horizontalForce = horizontalDir * Input.GetAxis("Horizontal");

        return verticalForce + horizontalForce;
    }
}
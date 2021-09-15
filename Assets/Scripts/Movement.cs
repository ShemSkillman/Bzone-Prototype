using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DebugHelper;

public class Movement : MonoBehaviour
{
    [SerializeField] float horizontalSpeed = 10f;
    [SerializeField] float verticalSpeed = 40f;

    [SerializeField] float terrainDetectionRange = 30f;
    [SerializeField] float targetHeight = 2f;
    [SerializeField] float terrainRepulsionDistance = 15f;
    [SerializeField] float repulsionSpeed = 5f;

    Rigidbody rb;
    LayerMask terrainLayer;

    private void Awake()
    {
        rb = GetComponentInParent<Rigidbody>();
        terrainLayer = LayerMask.GetMask("Terrain");
    }

    private void FixedUpdate()
    {
        Vector3 start = transform.position;

        Vector3 moveForce = GetMoveForce();

        Debug.DrawLine(start, start + (moveForce * terrainDetectionRange), Color.red);

        bool isHit = Physics.Raycast(start, moveForce, out RaycastHit hit, terrainDetectionRange, terrainLayer);

        if (isHit)
        {
            Vector3 target = hit.point + (Vector3.up * targetHeight);

            Vector3 targetDir = target - start;
            targetDir.Normalize();

            if (hit.distance <= terrainRepulsionDistance)
            {
                float horizontalRepulsionMult = (1 - (hit.distance / terrainRepulsionDistance)) * repulsionSpeed;

                targetDir = new Vector3(-targetDir.x * horizontalRepulsionMult, targetDir.y * verticalSpeed * (hit.distance / terrainRepulsionDistance), -targetDir.z * horizontalRepulsionMult);
            }
            else
            {
                float horizontalMoveMult = ((hit.distance - terrainRepulsionDistance) / (terrainDetectionRange - terrainRepulsionDistance)) * horizontalSpeed;

                targetDir = new Vector3(targetDir.x * horizontalMoveMult, targetDir.y * verticalSpeed, targetDir.z * horizontalMoveMult);
            }

            rb.AddForce(targetDir, ForceMode.Force);
        }
        else
        {
            rb.AddForce(moveForce * horizontalSpeed, ForceMode.Force);
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
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Movement : MonoBehaviour
{
    [SerializeField] float speed = 10f;
    [SerializeField] float terrianDetectionRange = 30f;
    [SerializeField] float targetHeight = 2f;

    Rigidbody rb;
    MeshCollider meshCollider;
    LayerMask terrainLayer;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        meshCollider = GetComponent<MeshCollider>();
        terrainLayer = LayerMask.GetMask("Terrain");
    }

    private void FixedUpdate()
    {
        Vector3 lowestPoint = meshCollider.bounds.min;
        Vector3 start = new Vector3(transform.position.x, lowestPoint.y, transform.position.z);

        Vector3 moveForce = GetMoveForce();

        Debug.DrawLine(start, start + (moveForce * terrianDetectionRange), Color.red);

        bool isHit = Physics.Raycast(start, moveForce, out RaycastHit hit, terrianDetectionRange, terrainLayer);

        if (isHit)
        {
            Vector3 target = hit.point + (Vector3.up * targetHeight);

            Vector3 targetDir = target - start;

            rb.AddForce(targetDir.normalized * speed, ForceMode.Force);
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
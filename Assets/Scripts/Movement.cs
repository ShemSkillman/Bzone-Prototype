using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Movement : MonoBehaviour
{
    [SerializeField] float speed = 10f;

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
        Vector3 lowestPointY = meshCollider.bounds.min;

        Vector3 start = new Vector3(transform.position.x, lowestPointY.y, transform.position.z);

        //Vector3 dir = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));

        Vector3 forwardDir = new Vector3(transform.forward.x, 0, transform.forward.z);
        Vector3 rightDir = new Vector3(transform.right.x, 0, transform.right.z);

        Vector3 dir = (forwardDir * Input.GetAxis("Vertical")) + (rightDir * Input.GetAxis("Horizontal"));

        Debug.DrawLine(start, start + dir * 30f, Color.red);

        bool isHit = Physics.Raycast(start, dir, out RaycastHit hit, 30f, terrainLayer);

        if (isHit && dir.magnitude > 0)
        {
            Vector3 target = hit.point + (Vector3.up * 3f);

            Vector3 moveDir = target - start;

            rb.AddForce(moveDir.normalized * speed, ForceMode.Force);
        }
        else
        {
            Move();
        }
        
    }

    private void Move()
    {
        Vector3 verticalDir = new Vector3(transform.forward.x, 0f, transform.forward.z);
        rb.AddForce(verticalDir * Input.GetAxis("Vertical") * speed, ForceMode.Force);

        Vector3 horizontal = new Vector3(transform.right.x, 0f, transform.right.z);
        rb.AddForce(horizontal * Input.GetAxis("Horizontal") * speed, ForceMode.Force);
    }
}
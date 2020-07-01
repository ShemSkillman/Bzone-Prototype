using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Movement : MonoBehaviour
{
    [SerializeField] float speed = 10f;

    Rigidbody rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void FixedUpdate()
    {
        Move();
    }

    private void Move()
    {
        Vector3 verticalDir = new Vector3(transform.forward.x, 0f, transform.forward.z);
        rb.AddForce(verticalDir * Input.GetAxis("Vertical") * speed, ForceMode.Force);

        Vector3 horizontal = new Vector3(transform.right.x, 0f, transform.right.z);
        rb.AddForce(horizontal * Input.GetAxis("Horizontal") * speed, ForceMode.Force);
    }
}

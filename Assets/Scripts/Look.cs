using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Look : MonoBehaviour
{

    [SerializeField] float turnSpeed = 5f;
    [SerializeField] float minAngle = -30;
    [SerializeField] float maxAngle = 60;

    Rigidbody rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void FixedUpdate()
    {
        float x = Mathf.Clamp(Input.GetAxisRaw("Mouse X"), -1, 1);
        rb.AddTorque(Vector3.up * x * turnSpeed);

        float y = Mathf.Clamp(Input.GetAxisRaw("Mouse Y"), -1, 1);

        var right = new Vector3(transform.right.x, 0f, transform.right.z);
        rb.AddTorque(right * y * turnSpeed * -1);
        

    }
}

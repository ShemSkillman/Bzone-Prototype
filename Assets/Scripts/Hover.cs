using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hover : MonoBehaviour
{
    [SerializeField] float maxHoverThrust = 10f;
    [SerializeField] float targetHeight = 2f;
    [SerializeField] float stabalizeForce = 100f;

    Rigidbody rb;
    LayerMask terrainLayer;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();

        rb.centerOfMass = new Vector3(0, 0, 0);

        terrainLayer = LayerMask.GetMask("Terrain");
    }

    private void FixedUpdate()
    {
        bool isHit = Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, targetHeight, terrainLayer);

        if (!isHit) return;
        
        Stabalize(hit.normal);

        rb.AddForce(maxHoverThrust * (1f - (hit.distance / targetHeight)) * Vector3.up, ForceMode.Acceleration);

    }

    private void Stabalize(Vector3 groundNormal)
    {

        var cross = Vector3.Cross(transform.up, groundNormal);

        var turnDir = transform.InverseTransformDirection(cross);


        rb.AddTorque(transform.forward * turnDir.z * stabalizeForce);

    }
}

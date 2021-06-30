using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hover : MonoBehaviour
{
    [SerializeField] float maxHoverThrust = 10f;
    [SerializeField] float targetHeight = 2f;
    [SerializeField] float stabalizeForce = 100f;

    [SerializeField] Transform frontHoverPoint;
    [SerializeField] Transform backHoverPoint;
    [SerializeField] Transform leftHoverPoint;
    [SerializeField] Transform rightHoverPoint;
    [SerializeField] Transform centerHoverPoint;

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
        Vector3[] hoverPoints = {
            frontHoverPoint.position,
            backHoverPoint.position,
            leftHoverPoint.position,
            rightHoverPoint.position,
            centerHoverPoint.position};

        RaycastHit closestHit = new RaycastHit();
        closestHit.distance = Mathf.Infinity;

        foreach (Vector3 point in hoverPoints)
        {
            bool isHit = Physics.Raycast(point, Vector3.down, out RaycastHit hit, targetHeight, terrainLayer);

            if (!isHit) continue;

            if (hit.distance < closestHit.distance)
            {
                closestHit = hit;
            }
        }

        if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit1, targetHeight, terrainLayer))
        {
            Stabalize(hit1.normal);
        }
        else
        {
            Stabalize(Vector3.up);
        }

        if (closestHit.distance == Mathf.Infinity) return;

        rb.AddForce(maxHoverThrust * (1f - (closestHit.distance / targetHeight)) * Vector3.up, ForceMode.Acceleration);
    }

    private void Stabalize(Vector3 groundNormal)
    {
        var cross = Vector3.Cross(transform.up, groundNormal);

        var turnDir = transform.InverseTransformDirection(cross);

        rb.AddTorque(transform.forward * turnDir.z * stabalizeForce);
    }
}
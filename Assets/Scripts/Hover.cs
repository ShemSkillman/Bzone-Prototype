using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DebugHelper;

public class Hover : MonoBehaviour
{
    [SerializeField] float maxHoverThrust = 10f;
    [SerializeField] float targetHeight = 2f;
    [SerializeField] float stabalizeForce = 100f;

    [SerializeField] Transform[] points;

    GizmoHelper gizmoHelper;
    Rigidbody rb;
    MeshCollider collider;

    float lowestHoverPointY = Mathf.Infinity;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        collider = GetComponent<MeshCollider>();

        gizmoHelper = FindObjectOfType<GizmoHelper>();

        rb.centerOfMass = new Vector3(0, 0, 0);

        foreach (Transform point in points)
        {
            if (point.position.y < lowestHoverPointY)
            {
                lowestHoverPointY = point.position.y;
            }
        }
    }

    private void FixedUpdate()
    {
        ApplyHoverForce();
        //Stabalize(Vector3.up);
    }

    private void ApplyHoverForce()
    {
        foreach (Transform point in points)
        {
            float height = targetHeight + (lowestHoverPointY - point.position.y);
            bool isHit = Physics.Raycast(point.position, Vector3.down, out RaycastHit hit, height, LayerMask.GetMask("Terrain"));

            if (isHit)
            {
                gizmoHelper.Colour = Color.white;
                gizmoHelper.DrawSphere(hit.point, 1f);
                gizmoHelper.DrawLine(point.transform.position, hit.point);

                rb.AddForceAtPosition(maxHoverThrust * (1f - (hit.distance / height)) * hit.normal, point.transform.position, ForceMode.Acceleration);
            }                
        }
    }

    private void Stabalize(Vector3 groundNormal)
    {
        var cross = Vector3.Cross(transform.up, groundNormal);

        var turnDir = transform.InverseTransformDirection(cross);

        rb.AddTorque(transform.forward * turnDir.z * stabalizeForce);
    }
}
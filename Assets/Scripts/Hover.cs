using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Debug;

public class Hover : MonoBehaviour
{
    [SerializeField] float maxHoverThrust = 10f;
    [SerializeField] float targetHeight = 2f;
    [SerializeField] float stabalizeForce = 100f;

    [SerializeField] Transform[] hoverPoints;

    GizmoHelper gizmoHelper;
    MeshCollider collider;
    Rigidbody rb;
    LayerMask terrainLayer;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        collider = GetComponent<MeshCollider>();
        gizmoHelper = GetComponent<GizmoHelper>();

        rb.centerOfMass = new Vector3(0, 0, 0);

        terrainLayer = LayerMask.GetMask("Terrain");       
    }

    private void FixedUpdate()
    {
        gizmoHelper.Clear();

        float lowestPointY = collider.bounds.min.y;

        RaycastHit closestHit = new RaycastHit();
        closestHit.distance = Mathf.Infinity;

        foreach (Transform point in hoverPoints)
        {
            float pointOffsetY = point.position.y - lowestPointY;

            bool isHit = Physics.Raycast(point.position, Vector3.down, out RaycastHit hit, targetHeight + pointOffsetY, terrainLayer);

            if (!isHit) continue;

            gizmoHelper.Colour = Color.white;
            gizmoHelper.DrawLine(point.position, hit.point);
            gizmoHelper.DrawSphere(hit.point, 1f);

            hit.distance -= pointOffsetY;

            if (hit.distance < closestHit.distance)
            {
                closestHit = hit;
            }
        }

        if (closestHit.distance != Mathf.Infinity)
        {
            rb.AddForce(maxHoverThrust * (1f - (closestHit.distance / targetHeight)) * Vector3.up, ForceMode.Acceleration);

            gizmoHelper.Colour = Color.green;
            gizmoHelper.DrawSphere(closestHit.point, 1.1f);
        }            

        if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit1, targetHeight, terrainLayer))
        {
            Stabalize(hit1.normal);
        }
        else
        {
            Stabalize(Vector3.up);
        }
    }

    private void Stabalize(Vector3 groundNormal)
    {
        var cross = Vector3.Cross(transform.up, groundNormal);

        var turnDir = transform.InverseTransformDirection(cross);

        rb.AddTorque(transform.forward * turnDir.z * stabalizeForce);
    }
}
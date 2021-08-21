using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DebugHelper;

public class Hover : MonoBehaviour
{
    [SerializeField] float maxHoverThrust = 10f;
    [SerializeField] float targetHeight = 2f;
    [SerializeField] float stabalizeForce = 100f;

    [SerializeField] Transform[] hoverPoints;

    GizmoHelper gizmoHelper;
    MeshCollider meshCollider;
    Rigidbody rb;
    LayerMask terrainLayer;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        meshCollider = GetComponent<MeshCollider>();
        gizmoHelper = GetComponent<GizmoHelper>();

        rb.centerOfMass = new Vector3(0, 0, 0);

        terrainLayer = LayerMask.GetMask("Terrain");
    }

    private void FixedUpdate()
    {
        gizmoHelper.Clear();

        ApplyHoverForce();
        ApplyStabalization();
    }

    private void ApplyHoverForce()
    {
        float lowestPointY = meshCollider.bounds.min.y;

        RaycastHit closestHit = new RaycastHit();
        closestHit.distance = Mathf.Infinity;

        foreach (Transform point in hoverPoints)
        {
            Vector3 pos = new Vector3(point.position.x, lowestPointY, point.position.z);

            bool isHit = Physics.Raycast(pos, Vector3.down, out RaycastHit hit, targetHeight, terrainLayer);

            if (!isHit) continue;

            gizmoHelper.Colour = Color.white;
            gizmoHelper.DrawLine(point.position, hit.point);
            gizmoHelper.DrawSphere(hit.point, 1f);

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
    }

    private void ApplyStabalization()
    {
        if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, targetHeight, terrainLayer))
        {
            Stabalize(hit.normal);
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
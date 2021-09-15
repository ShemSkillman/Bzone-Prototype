using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HoverPoint : MonoBehaviour
{
    public Vector3 HitPos { get; set; } = Vector3.zero;

    public float DistanceFromGround { get; set; } = Mathf.Infinity;

    public Vector3 Normal { get; set; } = Vector3.up;

    public void Recalculate(float maxDistance)
    {
        bool isHit = Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, maxDistance, LayerMask.GetMask("Terrain"));

        if (!isHit)
        {
            DistanceFromGround = Mathf.Infinity;
        }
        else
        {
            DistanceFromGround = hit.distance;
            HitPos = hit.point;
            Normal = hit.normal;
        }
    }
}
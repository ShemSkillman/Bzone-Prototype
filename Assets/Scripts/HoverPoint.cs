using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HoverSystem
{
    public class HoverPoint : MonoBehaviour
    {
        public Vector3 HitPos { get; set; } = Vector3.zero;

        public float DistanceFromGround { get; private set; } = Mathf.Infinity;

        public LayerMask HoverableLayers { get; set; }

        public void Recalculate(float maxDistance)
        {
            bool isHit = Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, maxDistance, HoverableLayers.value);

            if (!isHit)
            {
                DistanceFromGround = Mathf.Infinity;
                HitPos = transform.position + (Vector3.down * maxDistance);
            }
            else
            {
                DistanceFromGround = hit.distance;
                HitPos = hit.point;
            }
        }
    }
}
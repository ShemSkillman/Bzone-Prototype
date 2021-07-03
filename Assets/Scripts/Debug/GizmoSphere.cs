using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Debug
{
    public class GizmoSphere : GizmoObject
    {
        Vector3 center;
        float radius;

        public GizmoSphere(Vector3 center, float radius, Color col) : base(col)
        {
            this.center = center;
            this.radius = radius;
        }

        protected override void DrawSub()
        {
            Gizmos.DrawSphere(center, radius);
        }
    }
}
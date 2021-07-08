using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DebugHelper
{
    public class GizmoLine : GizmoObject
    {
        Vector3 from;
        Vector3 to;

        public GizmoLine(Vector3 from, Vector3 to, Color col) : base(col)
        {
            this.from = from;
            this.to = to;
        }

        protected override void DrawSub()
        {
            Gizmos.DrawLine(from, to);
        }
    }
}
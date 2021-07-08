using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DebugHelper
{
    public abstract class GizmoObject
    {
        protected Color col;

        public GizmoObject(Color col)
        {
            this.col = col;
        }

        public void Draw()
        {
            Gizmos.color = col;
            DrawSub();
        }

        protected abstract void DrawSub();
    }
}
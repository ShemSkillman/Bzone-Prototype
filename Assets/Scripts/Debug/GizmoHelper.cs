using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DebugHelper
{
    public class GizmoHelper : MonoBehaviour
    {
        List<GizmoObject> objectsToRender = new List<GizmoObject>();
        public Color Colour { get; set; } = Color.white;

        private void OnDrawGizmos()
        {
            foreach (GizmoObject obj in objectsToRender)
            {
                obj.Draw();
            }
        }

        public void DrawSphere(Vector3 center, float radius)
        {
            objectsToRender.Add(new GizmoSphere(center, radius, Colour));
        }

        public void DrawLine(Vector3 from, Vector3 to)
        {
            objectsToRender.Add(new GizmoLine(from, to, Colour));
        }

        public void Clear()
        {
            objectsToRender.Clear();
        }
    }
}
#if UNITY_EDITOR

using System.Collections.Generic;
using UnityEngine;

namespace FishingGameTool2D.Debuging
{
    public class DebugUtilities
    {
        #region DrawPath
        public static void DrawPath(List<Vector3> pathPoints, Color lineColor, Color pointColor, Vector3 pointSize = default)
        {
            if(pathPoints.Count < 1)
            {
                Debug.LogWarning("The path must have a minimum of two points.");
                return;
            }

            for(int i = 1; i < pathPoints.Count; i++)
            {
                Debug.DrawLine(pathPoints[i], pathPoints[i - 1], lineColor);
                DrawWireBox(pathPoints[i], Quaternion.LookRotation(pathPoints[i] - pathPoints[i - 1]), pointSize != default ? pointSize : new Vector3(0.2f, 0.2f, 0.2f), pointColor);
            }
        }

        private static void DrawWireBox(Vector3 pos, Quaternion rot, Vector3 scale, Color c)
        {
            Matrix4x4 m = new Matrix4x4();
            m.SetTRS(pos, rot, scale);

            var point1 = m.MultiplyPoint(new Vector3(-0.5f, -0.5f, 0.5f));
            var point2 = m.MultiplyPoint(new Vector3(0.5f, -0.5f, 0.5f));
            var point3 = m.MultiplyPoint(new Vector3(0.5f, -0.5f, -0.5f));
            var point4 = m.MultiplyPoint(new Vector3(-0.5f, -0.5f, -0.5f));

            var point5 = m.MultiplyPoint(new Vector3(-0.5f, 0.5f, 0.5f));
            var point6 = m.MultiplyPoint(new Vector3(0.5f, 0.5f, 0.5f));
            var point7 = m.MultiplyPoint(new Vector3(0.5f, 0.5f, -0.5f));
            var point8 = m.MultiplyPoint(new Vector3(-0.5f, 0.5f, -0.5f));

            Debug.DrawLine(point1, point2, c);
            Debug.DrawLine(point2, point3, c);
            Debug.DrawLine(point3, point4, c);
            Debug.DrawLine(point4, point1, c);

            Debug.DrawLine(point5, point6, c);
            Debug.DrawLine(point6, point7, c);
            Debug.DrawLine(point7, point8, c);
            Debug.DrawLine(point8, point5, c);

            Debug.DrawLine(point1, point5, c);
            Debug.DrawLine(point2, point6, c);
            Debug.DrawLine(point3, point7, c);
            Debug.DrawLine(point4, point8, c);
        }

        #endregion
    }
}
#endif


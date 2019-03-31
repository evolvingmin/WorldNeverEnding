using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace ChallengeKit
{
    public class DrawGizmos
    {
        public static Vector3 DrawRect2D(Rect rect, Color color)
        {
            Gizmos.color = color;

            Vector3 leftTop = rect.min;
            Vector3 rightTop = leftTop;
            rightTop.x += rect.width;
            Vector3 leftBottom = leftTop;
            leftBottom.y -= rect.height;

            Vector3 rightBottom = leftBottom;
            rightBottom.x += rect.width;

            Gizmos.DrawLine(leftTop, rightTop);
            Gizmos.DrawLine(rightTop, rightBottom);
            Gizmos.DrawLine(rightBottom, leftBottom);
            Gizmos.DrawLine(leftBottom, leftTop);

            return rightTop;
        }

        public static Vector3 DrawRect2D(Vector3 leftTop, float sizeX, float sizeY, Color color)
        {
            Gizmos.color = color;
            Vector3 rightTop = leftTop;
            rightTop.x += sizeX;
            Vector3 leftBottom = leftTop;
            leftBottom.y -= sizeY;

            Vector3 rightBottom = leftBottom;
            rightBottom.x += sizeX;

            Gizmos.DrawLine(leftTop, rightTop);
            Gizmos.DrawLine(rightTop, rightBottom);
            Gizmos.DrawLine(rightBottom, leftBottom);
            Gizmos.DrawLine(leftBottom, leftTop);

            return rightTop;
        }
    }
}

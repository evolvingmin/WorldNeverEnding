using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ChallengeKit
{
    public static class MathEx
    {
        // return : degrees
        public static float SignedAngle(Vector3 from, Vector3 to, Vector3 axis)
        {
            float angle = Vector3.Angle(from, to);
            float sign = Mathf.Sign(Vector3.Dot(axis, Vector3.Cross(from, to)));

            return angle * sign;
        }
        public static float SignedAngle(Vector3 from, Vector3 to)
        {
            return SignedAngle(from, to, Vector3.up);
        }


        public static bool IsIntersectionVectorAndRectangle(Vector3 Point1, Vector3 Point2, Rect rect)
        {
            return FasterLineSegmentIntersection(Point1, Point2, rect.min, rect.min + new Vector2(rect.width, 0)) ||
                FasterLineSegmentIntersection(Point1, Point2, rect.min, rect.min + new Vector2(0, rect.height)) ||
                FasterLineSegmentIntersection(Point1, Point2, rect.max, rect.max - new Vector2(0, rect.width)) ||
                FasterLineSegmentIntersection(Point1, Point2, rect.max, rect.max - new Vector2(0, rect.height));
        }

        static bool FasterLineSegmentIntersection(Vector2 p1, Vector2 p2, Vector2 p3, Vector2 p4)
        {

            Vector2 a = p2 - p1;
            Vector2 b = p3 - p4;
            Vector2 c = p1 - p3;

            float alphaNumerator = b.y * c.x - b.x * c.y;
            float alphaDenominator = a.y * b.x - a.x * b.y;
            float betaNumerator = a.x * c.y - a.y * c.x;
            float betaDenominator = alphaDenominator; /*2013/07/05, fix by Deniz*/

            bool doIntersect = true;

            if (alphaDenominator == 0 || betaDenominator == 0)
            {
                doIntersect = false;
            }
            else
            {
                if (alphaDenominator > 0)
                {
                    if (alphaNumerator < 0 || alphaNumerator > alphaDenominator)
                    {
                        doIntersect = false;
                    }
                }
                else if (alphaNumerator > 0 || alphaNumerator < alphaDenominator)
                {
                    doIntersect = false;
                }

                if (doIntersect && betaDenominator > 0)
                {
                    if (betaNumerator < 0 || betaNumerator > betaDenominator)
                    {
                        doIntersect = false;
                    }
                }
                else if (betaNumerator > 0 || betaNumerator < betaDenominator)
                {
                    doIntersect = false;
                }
            }

            return doIntersect;
        }
    }
}
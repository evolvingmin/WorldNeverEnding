using UnityEngine;
using System.Collections;

namespace ChallengeKit
{
    public static class TransformEx
    {
        public static void Reset(this Transform transform)
        {
            transform.localPosition = Vector3.zero;
            transform.localRotation = Quaternion.identity;
            transform.localScale = Vector3.one;
        }

        // 시작점과 끝점의 2D 회전 쿼터니언을 구합니다.
        public static void Rotate2D(this Transform transform, Vector3 startPosition, Vector3 endPosition)
        {
            float angle = Vector3.SignedAngle(Vector3.up, endPosition - startPosition, Vector3.forward);
            Vector3 angleZ = new Vector3(0, 0, angle);
            transform.localRotation = Quaternion.Euler(angleZ);
        }
    }


}

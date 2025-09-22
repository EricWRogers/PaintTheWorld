using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace KinematicCharacterControler
{
    // Rail component that defines a grindable rail
    [System.Serializable]
    public class Rail : MonoBehaviour
    {
        [Header("Rail Settings")]
        public Transform[] railPoints; // Points that define the rail path
        public float railWidth = 0.5f;
        public bool isLoop = false; // Whether the rail loops back to start

        [Header("Grinding Physics")]
        public float baseGrindSpeed = 8f;
        public float gravityInfluence = 0.00f; // How much gravity affects grinding


        public Vector3 GetPointOnRail(float t)
        {
            if (railPoints == null || railPoints.Length < 2) return Vector3.zero;

            t = Mathf.Clamp01(t);
            float scaledT = t * (railPoints.Length - 1);
            int index = Mathf.FloorToInt(scaledT);
            float localT = scaledT - index;

            if (index >= railPoints.Length - 1)
                return railPoints[railPoints.Length - 1].position;

            return Vector3.Lerp(railPoints[index].position, railPoints[index + 1].position, localT);
        }

        public Vector3 GetDirectionOnRail(float t)
        {
            if (railPoints == null || railPoints.Length < 2) return Vector3.forward;

            t = Mathf.Clamp01(t);
            float scaledT = t * (railPoints.Length - 1);
            int index = Mathf.FloorToInt(scaledT);

            if (index >= railPoints.Length - 1)
                index = railPoints.Length - 2;

            return (railPoints[index + 1].position - railPoints[index].position).normalized;
        }

        public float GetRailLength()
        {
            if (railPoints == null || railPoints.Length < 2) return 0f;

            float length = 0f;
            for (int i = 0; i < railPoints.Length - 1; i++)
            {
                if (railPoints[i] != null && railPoints[i + 1] != null)
                    length += Vector3.Distance(railPoints[i].position, railPoints[i + 1].position);
            }
            return length;
        }

        void OnDrawGizmos()
        {
            if (railPoints == null || railPoints.Length < 2) return;

            Gizmos.color = Color.cyan;
            for (int i = 0; i < railPoints.Length - 1; i++)
            {
                if (railPoints[i] != null && railPoints[i + 1] != null)
                {
                    Gizmos.DrawLine(railPoints[i].position, railPoints[i + 1].position);
                    Gizmos.DrawWireSphere(railPoints[i].position, railWidth * 0.5f);
                }
            }

            if (railPoints[railPoints.Length - 1] != null)
                Gizmos.DrawWireSphere(railPoints[railPoints.Length - 1].position, railWidth * 0.5f);
        }

    }
}
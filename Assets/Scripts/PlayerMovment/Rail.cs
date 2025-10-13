using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace KinematicCharacterControler
{
    [System.Serializable]
    public class Rail : MonoBehaviour
    {
        [Header("Rail Settings")]
        public Transform[] railPoints; // Points that define the rail path
        public float railWidth = 0.5f;
        public bool isLoop = false; // Whether the rail loops back to start

        [Header("Grinding Physics")]
        public float baseGrindSpeed = 8f;
        public float speedBoostMultiplier = 1.0f; // Multiplier for speed on this rail

        [Header("Rail Features")]
        public float railBoost = 0f; // Additional speed boost when entering this rail
        
        // Cached calculations for performance
        private float[] segmentLengths;
        private float totalLength;
        private bool cacheValid = false;




        public Vector3 GetPointOnRail(float t)
        {
            if (railPoints == null || railPoints.Length < 2) return Vector3.zero;

            // Handle looping
            if (isLoop)
            {
                t = t % 1f;
                if (t < 0) t += 1f;
            }
            else
            {
                t = Mathf.Clamp01(t);
            }

            float scaledT = t * (railPoints.Length - 1);
            int index = Mathf.FloorToInt(scaledT);
            float localT = scaledT - index;

            if (index >= railPoints.Length - 1)
            {
                if (isLoop && railPoints.Length > 0)
                {
                    // Loop back to start
                    return Vector3.Lerp(railPoints[railPoints.Length - 1].position, railPoints[0].position, localT);
                }
                return railPoints[railPoints.Length - 1].position;
            }

            return Vector3.Lerp(railPoints[index].position, railPoints[index + 1].position, localT);
        }

        public Vector3 GetDirectionOnRail(float t)
        {
            if (railPoints == null || railPoints.Length < 2) return Vector3.forward;

            // Handle looping
            if (isLoop)
            {
                t = t % 1f;
                if (t < 0) t += 1f;
            }
            else
            {
                t = Mathf.Clamp01(t);
            }

            float scaledT = t * (railPoints.Length - 1);
            int index = Mathf.FloorToInt(scaledT);

            if (index >= railPoints.Length - 1)
            {
                if (isLoop && railPoints.Length > 0)
                {
                    // Direction from last point to first
                    return (railPoints[0].position - railPoints[railPoints.Length - 1].position).normalized;
                }
                index = railPoints.Length - 2;
            }

            return (railPoints[index + 1].position - railPoints[index].position).normalized;
        }

        public float GetRailLength()
        {
            float length = 0f;
            for (int i = 0; i < railPoints.Length - 1; i++)
            {
                if (railPoints[i] != null && railPoints[i + 1] != null)
                    length += Vector3.Distance(railPoints[i].position, railPoints[i + 1].position);
            }
            return length;
        }

        // Get the curvature at a point for physics calculations
        public float GetCurvatureAtPoint(float t)
        {
            if (railPoints == null || railPoints.Length < 3) return 0f;

            const float delta = 0.01f;
            Vector3 dir1 = GetDirectionOnRail(Mathf.Max(0, t - delta));
            Vector3 dir2 = GetDirectionOnRail(Mathf.Min(1, t + delta));
            
            float angle = Vector3.Angle(dir1, dir2);
            return angle / (2f * delta);
        }


        void OnDrawGizmos()
        {
            if (railPoints == null || railPoints.Length < 2) return;

            // Draw rail path
            for (int i = 0; i < railPoints.Length - 1; i++)
            {
                if (railPoints[i] != null && railPoints[i + 1] != null)
                {
                    // Color based on slope
                    Vector3 dir = (railPoints[i + 1].position - railPoints[i].position).normalized;
                    float slope = dir.y;
                    
                    // Green for downslope (speed boost), red for upslope (speed loss)
                    Gizmos.color = Color.Lerp(Color.green, Color.red, (slope + 1f) * 0.5f);
                    Gizmos.DrawLine(railPoints[i].position, railPoints[i + 1].position);
                    
                    // Draw rail width
                    Gizmos.color = new Color(0, 1, 1, 0.3f);
                    Gizmos.DrawWireSphere(railPoints[i].position, railWidth * 0.5f);
                }
            }

            if (railPoints[railPoints.Length - 1] != null)
            {
                Gizmos.color = new Color(0, 1, 1, 0.3f);
                Gizmos.DrawWireSphere(railPoints[railPoints.Length - 1].position, railWidth * 0.5f);
            }

            // Draw loop connection
            if (isLoop && railPoints.Length > 1 && railPoints[0] != null && railPoints[railPoints.Length - 1] != null)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawLine(railPoints[railPoints.Length - 1].position, railPoints[0].position);
            }
        }
    }
}
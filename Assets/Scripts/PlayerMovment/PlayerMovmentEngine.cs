using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using System.Linq;
using System.Collections.Generic;
using System;


namespace KinematicCharacterControler
{
    
    [RequireComponent(typeof(CapsuleCollider))]
    [RequireComponent(typeof(Rigidbody))]
    public class PlayerMovmentEngine : MonoBehaviour
    {
        public CapsuleCollider capsule;
        [Tooltip("What layers the Player should collide with")]
        public LayerMask collisionLayers;

        [Header("Collision & Slope")]
        public float skinWidth = 0.015f;
        public int maxBounces = 5;

        [Tooltip("Max Slope you can walk up")]
        public float maxSlopeAngle = 55f;
        public float downSlopeMult = 1.01f;
        public float upSlopeMult = 0.99f;

        private float m_anglePower = 0.5f;

        [Header("Ground Checks")]
        [Tooltip("")]
        public float defaultGroundCheck = 0.25f;
        public float defaultGroundedDistance = 0.05f;
        public float snapDownDistance = 0.45f; 
        public bool shouldSnapDown = true; // Should snap to ground

        [Header("Gravity")]
        [Tooltip("Direction Gravity pulls the player towards:")]
        public Vector3 gravity = new Vector3(0f, -9.81f, 0f);

        [SerializeField, ReadOnly] protected GroundedState groundedState;

        public PhysicsMaterial DefaultPhysicsMat;
        private PhysicsMaterial currPhysicsMat;

        protected Vector3 m_velocity;

        private Bounds bounds;


        void Awake()
        {
            capsule = gameObject.GetComponent<CapsuleCollider>(); 
        }
 
        public Vector3 MovePlayer(Vector3 movement)
        {
            Vector3 position = transform.position;
            Quaternion rotation = transform.rotation;

            Vector3 remaining = movement;
            int bounces = 0;

            float maxMove = 20f;
            if (remaining.magnitude > maxMove)
                remaining = remaining.normalized * maxMove;

            while (bounces < maxBounces && remaining.sqrMagnitude > 0.000001f)
            {
                float distance = remaining.magnitude;

                if (!CastSelf(position, rotation, remaining.normalized, distance, out RaycastHit hit))
                {
                    position += remaining;
                    break;
                }

                
                float slopeAngle = Vector3.Angle(Vector3.up, hit.normal);
                bool isGround = hit.normal.y > 0.01f && slopeAngle <= maxSlopeAngle;

                /* //Wall Collision Stuff ot stop moemntum
                bool isWallLike = hit.normal.y < 0.1f || slopeAngle > maxSlopeAngle;
                float intoWall = Vector3.Dot(m_velocity, hit.normal);
                if (isWallLike && intoWall < -0.5f)
                {
                    Vector3 wallwardComponent = hit.normal * intoWall;
                    m_velocity -= wallwardComponent;

                    float headOnRatio = Mathf.Abs(intoWall) / (m_velocity.magnitude + 0.01f);
                    float tangentialDamping = Mathf.Lerp(1.0f, 0.4f, headOnRatio);
                    m_velocity *= tangentialDamping;
                }
                */
                // Handle overlaps / penetration
                if (hit.distance <= skinWidth * 0.5f)
                {
                    position += hit.normal * (skinWidth);
                    float into = Vector3.Dot(remaining, hit.normal);
                    if (into < 0f)
                        remaining -= hit.normal * into;

                    bounces++;
                    continue;
                }

                // Move to just before the impact
                float safeDist = hit.distance - skinWidth;
                if (safeDist > 0f)
                    position += remaining.normalized * safeDist;

                float usedFraction = Mathf.Clamp01(hit.distance / distance);
                remaining *= (1f - usedFraction);

                position += hit.normal * skinWidth;

                // Basic slide: remove normal component
                Vector3 n = hit.normal;
                float vn = Vector3.Dot(remaining, n);
                if (vn < 0f)
                    remaining -= n * vn;

                if (remaining.sqrMagnitude < 0.000001f)
                    break;

                // -------- SLOPE / MOMENTUM LOGIC (only for ground) --------
                Vector3 projected = Vector3.ProjectOnPlane(remaining, n);
                if (projected.sqrMagnitude > 0.000001f)
                {
                    if (isGround)
                    {
                        float slopeT = Mathf.InverseLerp(0f, maxSlopeAngle, slopeAngle);
                        Vector3 downhill = Vector3.ProjectOnPlane(Vector3.down, n);
                        if (downhill.sqrMagnitude > 0.000001f)
                        {
                            downhill.Normalize();
                            float alongDownhill = Vector3.Dot(projected.normalized, downhill);
                            float target = (alongDownhill > 0f) ? downSlopeMult : upSlopeMult;
                            float mult = Mathf.Lerp(1f, target, slopeT);

                            float weight = Mathf.Clamp01(remaining.magnitude / (movement.magnitude + 0.0001f));
                            m_velocity *= Mathf.Pow(mult, weight);
                        }
                    }

                    remaining = projected;
                }
                else
                {
                    remaining = Vector3.ProjectOnPlane(remaining, Vector3.up);
                }

                bounces++;
            }

            return position;
        }


 

        public Vector3 CollideAndSlide(Vector3 _vel, Vector3 _pos, int _depth, bool _gravityPass, Vector3 _velInit)
        {
            if (_depth > maxBounces) return Vector3.zero;

            float dist = _vel.magnitude + skinWidth;
            bounds = capsule.bounds;
            bounds.Expand(-2 * skinWidth);

            RaycastHit hit;


            if (CastSelf(_pos, transform.rotation, _vel.normalized, dist, out hit))
            {
                Vector3 snapToSurface = _vel.normalized * (hit.distance - skinWidth);
                Vector3 leftOver = _vel - snapToSurface;
                float angle = Vector3.Angle(Vector3.up, hit.normal);

                if (snapToSurface.magnitude <= skinWidth) snapToSurface = Vector3.zero;

                // SLope
                if (angle <= maxSlopeAngle)
                {
                    if (_gravityPass)
                    {
                        return snapToSurface;
                    }
                    leftOver = ProjectAndScale(leftOver, hit.normal);
                }
                else  // Wall or steep slope
                {
                    float scale = 1 - Vector3.Dot(
                        new Vector3(hit.normal.x, 0, hit.normal.z).normalized,
                        -new Vector3(_velInit.x, 0, _velInit.z).normalized

                    );

                    if (groundedState.isGrounded && !_gravityPass)
                    {
                        leftOver = ProjectAndScale(
                            new Vector3(leftOver.x, 0, leftOver.z),
                            new Vector3(hit.normal.x, 0, hit.normal.z)
                        ).normalized;
                        leftOver *= scale;
                    }
                    else
                    {
                        leftOver = ProjectAndScale(leftOver, hit.normal) * scale;
                    }
                }

                return snapToSurface + CollideAndSlide(leftOver, _pos + snapToSurface, _depth + 1, _gravityPass, _velInit);
            }


            return _vel;
        }

        private Vector3 ProjectAndScale(Vector3 _vec, Vector3 _normal)
        {
            float mag = _vec.magnitude;
            _vec = Vector3.ProjectOnPlane(_vec, _normal).normalized;
            _vec *= mag;
            return _vec;
        }

        public bool CheckIfGrounded(out RaycastHit _hit)
        {

            
            if (CastSelf(transform.position, transform.rotation, Vector3.down, defaultGroundCheck, out _hit))
            {
                float angle = Vector3.Angle(_hit.normal, Vector3.up);
                bool isGrounded = _hit.distance <= (capsule.height / 2f) + defaultGroundedDistance;

                groundedState = new GroundedState(_hit.distance, isGrounded, angle, _hit.normal, _hit.point);
                return isGrounded;
                
            }
            else
            {
                groundedState = new GroundedState(defaultGroundCheck, false, 0f, Vector3.up, Vector3.zero);
                return false;
            }
         }



        public bool CastSelf(Vector3 pos, Quaternion rot, Vector3 dir, float dist, out RaycastHit hit)
        {
           
            Vector3 center = rot * capsule.center + pos;
            float radius = capsule.radius;
            float height = capsule.height;

            // Get top and bottom points of collider
            Vector3 bottom = center + rot * Vector3.down * (height / 2 - radius);
            Vector3 top = center + rot * Vector3.up * (height / 2 - radius);

            IEnumerable<RaycastHit> hits = Physics.CapsuleCastAll( top, bottom, radius, dir, dist, collisionLayers, QueryTriggerInteraction.Ignore);
            bool didHit = hits.Count() > 0;

            // Find the closest objects hit
            float closestDist = didHit ? Enumerable.Min(hits.Select(hit => hit.distance)) : 0;
            IEnumerable<RaycastHit> closestHit = hits.Where(hit => hit.distance == closestDist);

            hit = closestHit.FirstOrDefault();

            // Return if any objects were hit
            return didHit;
        }

        public void SnapPlayerDown()
        {
            bool closeToGround = CastSelf(
               transform.position,
               transform.rotation,
               Vector3.down,
               snapDownDistance,
               out RaycastHit groundHit);

            // If within the threshold distance of the ground
            if (closeToGround && groundHit.distance > 0)
            {
                // Snap the player down the distance they are from the ground
                transform.position += Vector3.down * (groundHit.distance - 0.001f);
            }
        }

    }
        [Serializable]
        public struct GroundedState
        {
            public float distToGround;
            public bool isGrounded;
            public float angle;
            public Vector3 groundNormal;
            public Vector3 groundHitPosition;

            public GroundedState(float distToGround, bool isGrounded, float angle, Vector3 groundNormal, Vector3 groundHitPosition)
            {
                this.distToGround = distToGround;
                this.isGrounded = isGrounded;
                this.angle = angle;
                this.groundNormal = groundNormal;
                this.groundHitPosition = groundHitPosition;
            }

        }
}

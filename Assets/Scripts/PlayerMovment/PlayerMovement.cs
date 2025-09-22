using System.ComponentModel.Design.Serialization;
using JetBrains.Annotations;
using UnityEngine;

namespace KinematicCharacterControler
{
    public class PlayerMovement : MonoBehaviour
    {
        [Header("Movement")]
        public float speed = 5f;
        public float rotationSpeed = 5f;
        public float maxWalkAngle = 60f;
        public GameObject player;

        private Transform m_orientation;
        public Transform cam;
        public PlayerMovmentEngine engine;

        [Header("Physics")]
        public Vector3 gravity = new Vector3(0, -9, 0);
        private float m_elapsedFalling;
        private Vector3 m_velocity;
        public bool lockCursor = true;

        [Header("Jump Settings")]
        public float jumpVelocity = 5.0f;
        public float maxJumpAngle = 80f;
        public float jumpCooldown = 0.25f;
        public float jumpInputElapsed = Mathf.Infinity;
        private float m_timeSinceLastJump = 0.0f;
        public bool m_jumpInputPressed = false;
        private float m_jumpBufferTime = 0.25f;

        [Header("Rail Grinding")]
        public LayerMask railLayer;
        public float railDetectionRadius = 1.5f;
        public float railSnapDistance = 2f;
        public float minGrindSpeed = 3f;
        public float grindExitForce = 8f;
        
        [Header("Grinding Input")]
        public KeyCode grindKey = KeyCode.LeftControl;
        
        // Grinding state
        public bool isGrinding { get; private set; }
        public Rail currentRail;
        public float railProgress;
        public float grindSpeed;
        public Vector3 grindVelocity;
        public bool grindInputHeld;

        void Start()
        {
            player = GameObject.Find("Player");
            m_orientation = cam;
        }

        void Update()
        {
            HandleCursor();
            UpdateGrindInput();
            HandleInput();
            
            if (!isGrinding)
            {
                HandleRegularMovement();
                TryStartGrinding();
            }
            else
            {
                HandleGrindMovement();
            }
        }

        void HandleCursor()
        {
            if (lockCursor)
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
            else
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = false;
            }
        }

        void UpdateGrindInput()
        {
            grindInputHeld = Input.GetKey(grindKey);
        }

        void HandleInput()
        {
            if (Input.GetKey(KeyCode.Space))
                m_jumpInputPressed = true;
            else
                m_jumpInputPressed = false;

            if (m_jumpInputPressed)
                jumpInputElapsed = 0.0f;
            else
                jumpInputElapsed += Time.deltaTime;
        }

        void HandleRegularMovement()
        {
            float horizontal = Input.GetAxisRaw("Horizontal");
            float vertical = Input.GetAxisRaw("Vertical");

            Vector3 viewDir = transform.position - new Vector3(cam.position.x, transform.position.y, cam.position.z);
            m_orientation.forward = viewDir.normalized;

            Vector3 inputDir = m_orientation.forward * vertical + m_orientation.right * horizontal;

            // Rotate player
            if (inputDir != Vector3.zero)
            {
                player.transform.forward = Vector3.Slerp(player.transform.forward, inputDir.normalized, Time.deltaTime * rotationSpeed);
            }

            bool onGround = engine.CheckIfGrounded(out RaycastHit groundHit);
            bool falling = !(onGround && maxWalkAngle >= Vector3.Angle(Vector3.up, groundHit.normal));

            // Handle gravity and falling
            if (falling)
            {
                m_velocity += gravity * Time.deltaTime;
                m_elapsedFalling += Time.deltaTime;
            }
            else
            {
                m_velocity = Vector3.zero;
                m_elapsedFalling = 0;
            }

            // Handle jumping
            bool canJump = onGround && engine.groundedState.angle <= maxJumpAngle && m_timeSinceLastJump >= jumpCooldown;
            bool attemptingJump = jumpInputElapsed <= m_jumpBufferTime;

            if (canJump && attemptingJump)
            {
                m_velocity = Vector3.up * jumpVelocity;
                m_timeSinceLastJump = 0.0f;
                jumpInputElapsed = Mathf.Infinity;
            }
            else
            {
                m_timeSinceLastJump += Time.deltaTime;
            }

            // Apply movement
            transform.position = engine.MovePlayer(inputDir * speed * Time.deltaTime);
            transform.position = engine.MovePlayer(m_velocity * Time.deltaTime);

            if (onGround && !attemptingJump)
                engine.SnapPlayerDown();
        }

        void HandleGrindMovement()
        {
            ContinueGrinding();
        }

        // RAIL GRINDING SYSTEM
        void TryStartGrinding()
        {

            if (!grindInputHeld) return;
            
            // Check for nearby rails
            Collider[] railColliders = Physics.OverlapSphere(transform.position, railDetectionRadius, railLayer);
            
            Rail closestRail = null;
            float closestDistance = float.MaxValue;
            float bestProgress = 0f;
            
            foreach (var collider in railColliders)
            {
                Rail rail = collider.GetComponent<Rail>();
                if (rail == null) continue;
                
                // Find closest point on this rail
                float progress;
                Vector3 closestPoint = GetClosestPointOnRail(rail, transform.position, out progress);
                float distance = Vector3.Distance(transform.position, closestPoint);
                
                if (distance < closestDistance && distance <= railSnapDistance)
                {
                    closestDistance = distance;
                    closestRail = rail;
                    bestProgress = progress;
                }
            }
            
            if (closestRail != null)
            {
                StartGrinding(closestRail, bestProgress);
            }
        }
        
        Vector3 GetClosestPointOnRail(Rail rail, Vector3 position, out float progress)
        {
            progress = 0f;
            if (rail.railPoints == null || rail.railPoints.Length < 2) return Vector3.zero;
            
            Vector3 closestPoint = rail.railPoints[0].position;
            float closestDistance = Vector3.Distance(position, closestPoint);
            
            for (int i = 0; i < rail.railPoints.Length - 1; i++)
            {
                Vector3 lineStart = rail.railPoints[i].position;
                Vector3 lineEnd = rail.railPoints[i + 1].position;
                
                Vector3 pointOnLine = GetClosestPointOnLine(lineStart, lineEnd, position);
                float distance = Vector3.Distance(position, pointOnLine);
                
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestPoint = pointOnLine;
                    
                    float segmentLength = Vector3.Distance(lineStart, lineEnd);
                    float distanceAlongSegment = Vector3.Distance(lineStart, pointOnLine);
                    float localProgress = segmentLength > 0 ? distanceAlongSegment / segmentLength : 0f;
                    
                    progress = (i + localProgress) / (rail.railPoints.Length - 1);
                }
            }
            
            return closestPoint;
        }
        
        Vector3 GetClosestPointOnLine(Vector3 lineStart, Vector3 lineEnd, Vector3 point)
        {
            Vector3 lineDirection = lineEnd - lineStart;
            float lineLength = lineDirection.magnitude;
            lineDirection.Normalize();
            
            Vector3 toPoint = point - lineStart;
            float projectedDistance = Vector3.Dot(toPoint, lineDirection);
            projectedDistance = Mathf.Clamp(projectedDistance, 0f, lineLength);
            
            return lineStart + lineDirection * projectedDistance;
        }
        
        void StartGrinding(Rail rail, float progress)
        {
            isGrinding = true;
            currentRail = rail;
            railProgress = progress;

            Vector3 railDirection = rail.GetDirectionOnRail(progress);
            grindSpeed = speed;
            
            Vector3 railPosition = rail.GetPointOnRail(progress);
            transform.position = railPosition;
            
            m_velocity = Vector3.zero;
        }
        
        void ContinueGrinding()
        {
            if (currentRail == null)
            {
                ExitGrinding();
                return;
            }
            
            
            // Exit if at end of rail
            if (railProgress >= 1f)
            {
                ExitGrinding();
                return;
            }
            
            // Calculate movement along rail
            Vector3 railDirection = currentRail.GetDirectionOnRail(railProgress);
            
            // Apply gravity influence (makes grinding faster downhill, slower uphill)
            float gravityEffect = Vector3.Dot(gravity.normalized, railDirection) * currentRail.gravityInfluence;
            grindSpeed += gravityEffect * Time.deltaTime;
            
            // Apply speed curve
            float speedMultiplier = currentRail.speedCurve.Evaluate(railProgress);
            float effectiveSpeed = grindSpeed * speedMultiplier;
            
            // Clamp minimum speed
            effectiveSpeed = Mathf.Max(minGrindSpeed, effectiveSpeed);
            
            // Calculate desired movement along rail
            Vector3 railMovement = railDirection * effectiveSpeed * Time.deltaTime;
            
            // Use your movement engine for the rail movement
            Vector3 currentPos = transform.position;
            Vector3 newPosition = engine.MovePlayer(railMovement);
            transform.position = newPosition;
            
            // Update rail progress based on actual movement achieved along rail direction
            Vector3 actualMovement = transform.position - currentPos;
            float actualDistance = Vector3.Dot(actualMovement, railDirection);
            
            float railLength = currentRail.GetRailLength();
            if (railLength > 0)
            {
                float progressDelta = actualDistance / railLength;
                railProgress += progressDelta;
                
                // Clamp progress to ensure it doesn't go beyond the rail
                railProgress = Mathf.Clamp01(railProgress);
            }
            
            // Additional check: if we're very close to the end, snap to end
            if (railProgress > 0.95f)
            {
                Vector3 railEnd = currentRail.GetPointOnRail(1f);
                float distanceToEnd = Vector3.Distance(transform.position, railEnd);
                
                // If very close to end, consider it reached
                if (distanceToEnd < 1f)
                {
                    railProgress = 1f;
                }
            }
            
            // Keep player aligned to rail (prevent drift)
            Vector3 idealRailPosition = currentRail.GetPointOnRail(railProgress);
            Vector3 currentRailPosition = transform.position;
            
            // Only correct position if we've drifted too far from the rail
            float driftDistance = Vector3.Distance(currentRailPosition, idealRailPosition);
            if (driftDistance > 0.5f) // Allow some tolerance
            {
                // Gradually pull back to rail instead of snapping
                Vector3 correctionDirection = (idealRailPosition - currentRailPosition).normalized;
                Vector3 correction = correctionDirection * Mathf.Min(driftDistance, 2f * Time.deltaTime);
                transform.position += correction;
            }
            
            // Store grind velocity for potential exit
            grindVelocity = railDirection * effectiveSpeed;
        }
        
        void ExitGrinding()
        {
            if (!isGrinding) return;
            
            isGrinding = false;
            
            // Give player exit velocity
            if (currentRail != null)
            {
                Vector3 railDirection = currentRail.GetDirectionOnRail(railProgress);
                m_velocity = railDirection * grindSpeed;
                
                // Add slight upward velocity for smooth transition
                m_velocity.y = grindExitForce;
            }
            
            currentRail = null;
            railProgress = 0f;
            grindSpeed = 0f;
        }
        
        // Visualization
        void OnDrawGizmos()
        {
            if (isGrinding && currentRail != null)
            {
                Gizmos.color = Color.yellow;
                Vector3 railPos = currentRail.GetPointOnRail(railProgress);
                Gizmos.DrawWireSphere(railPos, 0.5f);
                
                Vector3 railDir = currentRail.GetDirectionOnRail(railProgress);
                Gizmos.DrawRay(railPos, railDir * 2f);
            }
            
            // Show detection radius when not grinding
            if (!isGrinding)
            {
                Gizmos.color = Color.blue;
                Gizmos.DrawWireSphere(transform.position, railDetectionRadius);
            }
        }
    }
}
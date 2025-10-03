using UnityEngine;


namespace KinematicCharacterControler
{
    public class PlayerMovement : PlayerMovmentEngine
    {
        [Header("Movement")]
        public float speed = 5f;
        public float rotationSpeed = 5f;
        public float maxWalkAngle = 60f;
        public GameObject player;
        private float currSpeed;

        private Transform m_orientation;
        public Transform cam;
        public float movementColorMult = 2f;
        private float m_currColorMult = 1f;
        private float m_shopMoveMult => PlayerManager.instance.stats.skills[3].currentMult;
        public GetPaintColor standPaintColor;
        private PaintColors colors;

        [Header("Momentum")]
        public float acceleration = 10f;
        public float deceleration = 8f;
        public float maxSpeed = 20;

        private Vector3 m_momentum = Vector3.zero;

        [Header("Physics")]
        private float m_elapsedFalling;
        public bool lockCursor = true;

        [Header("Jump Settings")]
        public float jumpForce = 5.0f;
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
        public float m_railDir = 1f;


        [SerializeField] private Transform m_railDetectionPoint;
        void Start()
        {
            player = GameObject.Find("Player");
            m_orientation = cam;
            colors = PaintManager.instance.GetComponent<PaintColors>();
        }

        void Update()
        {
            HandleCursor();
            UpdateGrindInput();
            HandleInput();
            HandlePaintColor();

            if (!isGrinding)
            {
                HandleRegularMovement();
                TryStartGrinding();
            }
            else
            {
                ContinueGrinding();
            }
        }
        
        public void HandlePaintColor()
        {
            m_currColorMult = standPaintColor.standingColor == colors.movementPaint ? m_currColorMult = movementColorMult : m_currColorMult = 1f;
            //Debug.Log(m_currColorMult);
         }

        public void HandleCursor()
        {
            if (lockCursor)
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
            else
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
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
            currSpeed = speed * movementColorMult * m_shopMoveMult;
            float horizontal = Input.GetAxisRaw("Horizontal");
            float vertical = Input.GetAxisRaw("Vertical");


            Vector3 viewDir = transform.position - new Vector3(cam.position.x, transform.position.y, cam.position.z);
            m_orientation.forward = viewDir.normalized;

            Vector3 inputDir = m_orientation.forward * vertical + m_orientation.right * horizontal;
            inputDir = inputDir.normalized;

            // target velocity
            Vector3 targetVelocity = inputDir * currSpeed;

            if (inputDir.magnitude > 0.1f)
            {
                m_momentum = Vector3.MoveTowards(m_momentum, targetVelocity, currSpeed * Time.deltaTime);
            }
            else
            {
                m_momentum = Vector3.MoveTowards(m_momentum, Vector3.zero, currSpeed * Time.deltaTime);
            }
            
            if (m_momentum.magnitude > maxSpeed)
            {
                m_momentum = m_momentum.normalized * maxSpeed;
            }
            



            // Rotate player
            if (inputDir != Vector3.zero)
            {
                player.transform.forward = Vector3.Slerp(player.transform.forward, m_momentum, Time.deltaTime * rotationSpeed);
                m_velocity.x = 0f;
                m_velocity.z = 0f;
            }
            
            if (Input.GetMouseButton(0))
                player.transform.forward = Vector3.Slerp(player.transform.forward, m_orientation.forward, Time.deltaTime * rotationSpeed);

            bool onGround = CheckIfGrounded(out RaycastHit groundHit) && m_velocity.y <= 0.0f;
            bool falling = !(onGround && maxWalkAngle >= Vector3.Angle(Vector3.up, groundHit.normal));

            // Handle gravity and falling
            if (falling)
            {
                m_velocity += gravity * Time.deltaTime;
                m_elapsedFalling += Time.deltaTime;
            }
            else if(onGround)
            {
                m_velocity = Vector3.zero;
                m_elapsedFalling = 0;
            }

            // Handle jumping
            bool canJump = onGround && groundedState.angle <= maxJumpAngle && m_timeSinceLastJump >= jumpCooldown;
            bool attemptingJump = jumpInputElapsed <= m_jumpBufferTime;

            if (canJump && attemptingJump)
            {
                m_velocity = Vector3.up * jumpForce;
                m_timeSinceLastJump = 0.0f;
                jumpInputElapsed = Mathf.Infinity;
            }
            else
            {
                m_timeSinceLastJump += Time.deltaTime;
            }

            // Apply movement
            transform.position = MovePlayer(m_momentum * Time.deltaTime);
            transform.position = MovePlayer(m_velocity * Time.deltaTime);

            if (onGround && !attemptingJump)
                SnapPlayerDown();
        }


        // RAIL GRINDING SYSTEM
        void TryStartGrinding()
        {

            
            // Check for nearby rails
            Collider[] railColliders = Physics.OverlapSphere(m_railDetectionPoint.position, railDetectionRadius, railLayer);
            
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

            
            grindSpeed = speed;
            
            // Get rail direction at this point
            Vector3 railDir = rail.GetDirectionOnRail(progress);
            
            // Determine which direction along the rail matches player's movement better
            float forwardDot = Vector3.Dot(transform.forward, railDir);
            float backwardDot = Vector3.Dot(transform.forward, -railDir);
            
             m_railDir = forwardDot > backwardDot ? 1f : -1f;
            
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

            if (m_jumpInputPressed)
            {
                ExitGrinding();
                return;
            }

            if (!currentRail.isLoop)
            {
                // Exit if at end of rail
                if (railProgress >= 1f || railProgress <= 0f)
                {
                    ExitGrinding();
                    return;
                }
            }

            // Calculate movement along rail
            Vector3 railDirection = currentRail.GetDirectionOnRail(railProgress) * m_railDir;


            Vector3 railMovement = railDirection * grindSpeed * Time.deltaTime;


            Vector3 currentPos = transform.position;
            Vector3 newPosition = MovePlayer(railMovement);
            transform.position = newPosition;

            // Update rail progress based on actual movement achieved along rail direction
            Vector3 actualMovement = transform.position - currentPos;
            float actualDistance = Vector3.Dot(actualMovement, railDirection);

            float railLength = currentRail.GetRailLength();
            if (railLength > 0)
            {
                float progressDelta = (actualDistance / railLength) * m_railDir;
                railProgress += progressDelta;
            }



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
            grindVelocity = railDirection * grindSpeed;
            SnapPlayerDown();
        }
        
        void ExitGrinding()
        {
            if (!isGrinding) return;
            
            isGrinding = false;
            
            // Give player exit velocity
            if (currentRail != null)
            {
                Vector3 railDirection = currentRail.GetDirectionOnRail(railProgress) * m_railDir;
                m_velocity = railDirection * grindSpeed;
                
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

                Vector3 railDir = currentRail.GetDirectionOnRail(railProgress) * m_railDir;
                Gizmos.DrawRay(railPos, railDir * 2f);
            }

            Gizmos.color = Color.blue;

            Gizmos.DrawWireSphere(m_railDetectionPoint.position, railDetectionRadius);
            
        }
    }
}
using NUnit.Framework;
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
        private Vector2 moveInput;
        public bool lockCursor = true;

        [Header("Momentum")]
        public float acceleration = 10f;
        public float deceleration = 8f;
        public float maxSpeed = 20;

        private Vector3 m_momentum = Vector3.zero;

        [Header("Dashing")]
        public AnimationCurve dashSpeedCurve;
        public float dashSpeed = 20f;
        public KeyCode dashKey = KeyCode.LeftShift;
        public bool isDashing = false;
        public float dashDuration = 0.5f;
        public float dashCooldown = 2f;
        private float m_dashTime = 0f;
        private float m_timeSinceLastDash = 0f;
        public bool m_dashInputPressed = false;

        [Header("Jump Settings")]
        public float jumpForce = 5.0f;
        public float maxJumpAngle = 80f;
        public float jumpCooldown = 0.25f;
        public float jumpInputElapsed = Mathf.Infinity;
        private float m_timeSinceLastJump = 0.0f;
        public bool m_jumpInputPressed = false;
        private float m_jumpBufferTime = 0.25f;
        private float m_elapsedFalling;

        [Header("Wall Riding")]
        public bool isWallRiding = false;
        public float wallCheckDist = 1;

        private float m_wallTimer = 0f;
        public LayerMask wallLayers;
        private bool leftWall;
        private bool rightWall;
        private RaycastHit leftWallHit;
        private RaycastHit rightWallHit;
        
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
            HandleInput();
            HandlePaintColor();
            HandleFOV();

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


        public void HandleFOV()
        {
            Camera cam = Camera.main;
            if (cam == null) return;

            float targetFOV = isDashing ? 80f : 60f;
            cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, targetFOV, Time.deltaTime * 8f);
        }
        
        public void HandlePaintColor()
        {
            m_currColorMult = standPaintColor.standingColor == colors.movementPaint ? m_currColorMult = movementColorMult : m_currColorMult = 1f;
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

        void HandleInput()
        {
            moveInput = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
            
            if (Input.GetKey(KeyCode.Space))
                m_jumpInputPressed = true;
            else
                m_jumpInputPressed = false;

            if (m_jumpInputPressed)
                jumpInputElapsed = 0.0f;
            else
                jumpInputElapsed += Time.deltaTime;

            if (Input.GetKeyDown(dashKey))
            {
                m_dashInputPressed = true;
                isDashing = true;
            }
                

        }

        void HandleRegularMovement()
        {
            
            m_timeSinceLastDash += Time.deltaTime;
        
            currSpeed = speed * movementColorMult * m_shopMoveMult;


            Vector3 viewDir = transform.position - new Vector3(cam.position.x, transform.position.y, cam.position.z);
            m_orientation.forward = viewDir.normalized;

            Vector3 inputDir = m_orientation.forward * moveInput.y + m_orientation.right * moveInput.x;
            inputDir = inputDir.normalized;

            // target velocity
            Vector3 targetVelocity = inputDir * currSpeed;

            if (isDashing)
            {
                HandleDashing();
            }

            if (inputDir.magnitude > 0.1f && !isDashing)
            {
                m_momentum = Vector3.MoveTowards(m_momentum, targetVelocity, currSpeed * Time.deltaTime);
            }
            else
            {
                m_momentum *= (1 - DefaultPhysicsMat.dynamicFriction);
            }
            
            if (m_momentum.magnitude > maxSpeed && !isDashing)
            {
                m_momentum = m_momentum.normalized * maxSpeed;
            }
            



            // Rotate player
            if (inputDir != Vector3.zero &&  !Input.GetMouseButton(0))
            {
                player.transform.forward = Vector3.Slerp(player.transform.forward, m_momentum.normalized, Time.deltaTime * rotationSpeed);
                m_velocity.x = 0f;
                m_velocity.z = 0f;
            }
            
            if (Input.GetMouseButton(0))
                player.transform.forward = Vector3.Slerp(player.transform.forward, m_orientation.forward, Time.deltaTime * rotationSpeed);

            bool onGround = CheckIfGrounded(out RaycastHit groundHit) && m_velocity.y <= 0.0f;
            bool falling = !(onGround && maxWalkAngle >= Vector3.Angle(Vector3.up, groundHit.normal));

            // Handle gravity and falling
            if (falling && !isWallRiding)
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

            if (isWallRiding)
            {
                m_wallTimer += Time.deltaTime;
                if (CheckForWall())
                {
                    RaycastHit wallHit = leftWall ? leftWallHit : rightWallHit;
                    HandleWallRunning(wallHit);
                }
                else
                {
                    ExitWallRide();
                }
                return;
            }


            if (m_jumpInputPressed && falling && CheckForWall())
            {
                if (leftWall)
                {
                    HandleWallRunning(leftWallHit);
                    m_wallTimer = 0f;
                    isWallRiding = true;
                    return;
                }
                if (rightWall)
                {
                    HandleWallRunning(rightWallHit);
                    m_wallTimer = 0f;
                    isWallRiding = true;
                    return;
                }
                isWallRiding = false;
            }
            else
                isWallRiding = false;
             

            // Apply movement (combine momentum and velocity so one doesn't overwrite the other)
            Vector3 totalMovement = m_momentum + m_velocity;
            transform.position = MovePlayer(totalMovement * Time.deltaTime);

            if (onGround && !attemptingJump)
                SnapPlayerDown();
        }

        void HandleDashing()
        {
            

            bool canDash = m_timeSinceLastDash >= dashCooldown;
            if (m_dashInputPressed && canDash)
            {
                m_dashTime = 0f;
                m_timeSinceLastDash = 0f;
                m_dashInputPressed = false;
            }

            if (m_dashTime < dashDuration)
            {
                float dashProgress = m_dashTime / dashDuration;
                float currentDashSpeed = dashSpeed * dashSpeedCurve.Evaluate(dashProgress);

                Vector3 dashDirection = (m_orientation.forward * moveInput.y + m_orientation.right * moveInput.x).normalized;
                if (dashDirection == Vector3.zero)
                    dashDirection = m_orientation.forward;

                Vector3 dashVelocity = dashDirection * currentDashSpeed;

                m_momentum = Vector3.MoveTowards(m_momentum, dashVelocity, currentDashSpeed * Time.deltaTime);

                m_dashTime += Time.deltaTime;
            }
            else
            {
                isDashing = false;
            }
        }


        void HandleWallRunning(RaycastHit _hit)
        {
            Vector3 wallNormal = _hit.normal;

            if (m_jumpInputPressed  && m_wallTimer > 0.2f)
                {
                    RaycastHit currentWallHit = leftWall ? leftWallHit : rightWallHit;

                    m_velocity = (wallNormal + Vector3.up).normalized * jumpForce;
                    m_momentum = wallNormal * jumpForce * 0.5f; // Push away from wall
                    jumpInputElapsed = Mathf.Infinity; // Consume the jump input
                    ExitWallRide();
                    return;
                } 


            if (!isWallRiding)
            {
                float wallOffset = 0.5f;
                transform.position = _hit.point + wallNormal * wallOffset;
            }

            isWallRiding = true;

            Vector3 wallDirection = Vector3.Cross(wallNormal, Vector3.up).normalized;
            if (Vector3.Dot(wallDirection, transform.forward) < 0)
                wallDirection *= -1;

            m_momentum = wallDirection * Mathf.Max(m_momentum.magnitude, speed);


            Vector3 totalMovement = m_momentum + m_velocity;
            transform.position = MovePlayer(totalMovement * Time.deltaTime);

            m_elapsedFalling = 0f;
        }

        void ExitWallRide()
        {

            isWallRiding = false;
        }

        bool CheckForWall()
        {
            // Cast rays to each side and store the proper hit information.
            bool hitLeft = Physics.Raycast(transform.position, -transform.right, out RaycastHit leftHitTemp, wallCheckDist, collisionLayers);
            bool hitRight = Physics.Raycast(transform.position, transform.right, out RaycastHit rightHitTemp, wallCheckDist, collisionLayers);

            leftWall = hitLeft;
            rightWall = hitRight;
            leftWallHit = leftHitTemp;
            rightWallHit = rightHitTemp;

            return leftWall || rightWall;
        }

        #region Rail grinding
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

            
            grindSpeed = m_momentum.magnitude;
            
            // Get rail direction at this point
            Vector3 railDir = rail.GetDirectionOnRail(progress);
            
            // Determine which direction along the rail matches player's movement better
            float forwardDot = Vector3.Dot(transform.forward, railDir);
            float backwardDot = Vector3.Dot(transform.forward, -railDir);
            
             m_railDir = forwardDot > backwardDot ? 1f : -1f;
            
            Vector3 railPosition = rail.GetPointOnRail(progress);
            transform.position = railPosition;
            railDir *= m_railDir;
            m_momentum = m_momentum.magnitude * railDir;
            
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


            m_momentum += railDirection * grindSpeed;
            if(m_momentum.magnitude > grindSpeed)
            {
                m_momentum = m_momentum.normalized * grindSpeed;
            }



            Vector3 currentPos = transform.position;

            transform.position = MovePlayer(m_momentum * Time.deltaTime);

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
                m_momentum += railDirection * grindSpeed;
                m_momentum.y = grindExitForce;

                //m_velocity.y = grindExitForce;
            }
            
            currentRail = null;
            railProgress = 0f;
            grindSpeed = 0f;
        }
        #endregion
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

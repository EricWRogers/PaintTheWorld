using UnityEngine;
using KinematicCharacterControler;
using System.ComponentModel;

public class PlayerMovement : PlayerMovmentEngine
{
    private PlayerInputActions.PlayerActions m_inputActions;

    [Header("Movement")]
    public float speed = 5f;
    public float maxSpeed = 20f; 
    public float rotationSpeed = 5f;
    public float maxWalkAngle = 60f;
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
    public float groundAccelMult = 1f;
    public float airAccelMult = 0.8f;
    public float airDrag = 0.05f;
    public float groundDrag = 0.1f;

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

    [Header("Wall Riding")]
    [SerializeField] private bool m_isWallRiding = false;
    public float wallCheckDist = 1f;
    public float climbMult = 0.5f;
    public LayerMask wallLayers;
    public bool leftWall;
    public bool rightWall;
    private Vector3 m_wallNormal;
    private Vector3 m_wallRunDir;
    public float wallCheckDistance = 1f;

    [Header("Rail Grinding")]
    public LayerMask railLayer;
    public float railDetectionRadius = 1.5f;
    public float railSnapDistance = 2f;
    public float minGrindSpeed = 3f;
    public float grindExitForce = 8f;
    public bool isGrinding { get; private set; }
    public Rail currentRail;
    public float railProgress;
    public float grindSpeed;
    public float m_railDir = 1f;
    [SerializeField] private Transform m_railDetectionPoint;

    void Start()
    {
        m_orientation = cam;
        colors = PaintManager.instance.GetComponent<PaintColors>();
        m_inputActions = new PlayerInputActions().Player;
        m_inputActions.Enable();
    }

    void Update()
    {
        HandleCursor();
        HandleInput();
        HandlePaintColor();
        HandleFOV();
        m_timeSinceLastDash += Time.deltaTime;
    }

    void FixedUpdate()
    {
        HandleRotation();

       
        if (TryStartGrinding() || isGrinding)
        {
            ContinueGrinding();
        }
        else if (WallRun())
        {
            transform.position = MovePlayer(m_velocity * Time.deltaTime);
        }
        else if (HandleDashing())
        {
            // Dashing handles its own momentum
            transform.position = MovePlayer(m_velocity * Time.deltaTime);
        }
        else
        {
            HandleRegularMovement();
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
        m_currColorMult = standPaintColor.standingColor == colors.movementPaint ? movementColorMult : 1f;
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
        moveInput = m_inputActions.Move.ReadValue<Vector2>();

        m_jumpInputPressed = m_inputActions.Jump.IsPressed();
        if (m_jumpInputPressed)
            jumpInputElapsed = 0.0f;
        else
            jumpInputElapsed += Time.deltaTime;
        
        if (m_inputActions.Dash.WasPressedThisFrame())
            m_dashInputPressed = true;
    }

    void HandleRegularMovement()
    {
        currSpeed = speed * movementColorMult * m_shopMoveMult;

        Vector3 inputDir = m_orientation.forward * moveInput.y + m_orientation.right * moveInput.x;
        inputDir.Normalize();

        // Check ground state
        bool onGround = CheckIfGrounded(out RaycastHit groundHit);
        bool canWalk = onGround && maxWalkAngle >= Vector3.Angle(Vector3.up, groundHit.normal);

        // Apply horizontal input acceleration
        if (inputDir.magnitude > 0.1f && !isDashing)
        {
            Vector3 targetVelocity = inputDir * currSpeed;
            float accelMult = canWalk ? groundAccelMult : airAccelMult;
            
            // Only accelerate horizontal components, preserve vertical momentum
            Vector3 horizontalMomentum = new Vector3(m_velocity.x, 0, m_velocity.z);
            Vector3 newHorizontal = Vector3.MoveTowards(horizontalMomentum, targetVelocity * accelMult, currSpeed * Time.deltaTime);
            m_velocity.x = newHorizontal.x;
            m_velocity.z = newHorizontal.z;
        }
        else
        {
            // Apply drag
            float drag = canWalk ? groundDrag : airDrag;
            m_velocity.x *= (1f - drag);
            m_velocity.z *= (1f - drag);
        }        

        // Clamp horizontal speed
        Vector3 horizontalSpeed = new Vector3(m_velocity.x, 0, m_velocity.z);
        if (horizontalSpeed.magnitude > maxSpeed && !isDashing)
        {
            horizontalSpeed = horizontalSpeed.normalized * maxSpeed;
            m_velocity.x = horizontalSpeed.x;
            m_velocity.z = horizontalSpeed.z;
        }

        // Apply gravity
        if (!canWalk)
        {
            m_velocity += gravity * Time.deltaTime;
        }
        else
        {
            // On ground  zero out vertical momentum
            m_velocity.y = 0f;
        }

        // Handle jumping
        bool canJump = onGround && groundedState.angle <= maxJumpAngle && m_timeSinceLastJump >= jumpCooldown;
        bool attemptingJump = jumpInputElapsed <= m_jumpBufferTime;
        
        if (canJump && attemptingJump)
        {
            m_velocity += jumpForce * Vector3.up;
            m_timeSinceLastJump = 0.0f;
            jumpInputElapsed = Mathf.Infinity;
        }
        else
        {
            m_timeSinceLastJump += Time.deltaTime;
        }

        // Apply movement
        transform.position = MovePlayer(m_velocity * Time.deltaTime);
        
        if (onGround && !attemptingJump)
            SnapPlayerDown();
    }

    bool HandleDashing()
    {
        bool canDash = m_timeSinceLastDash >= dashCooldown;
        
        if (m_dashInputPressed && canDash)
        {
            m_dashTime = 0f;
            m_timeSinceLastDash = 0f;
            m_dashInputPressed = false;
            isDashing = true;
        }
        
        if (isDashing && m_dashTime < dashDuration)
        {
            float dashProgress = m_dashTime / dashDuration;
            float currentDashSpeed = dashSpeed * dashSpeedCurve.Evaluate(dashProgress) * m_shopMoveMult;

            Vector3 dashDirection = (m_orientation.forward * moveInput.y + m_orientation.right * moveInput.x).normalized;
            if (dashDirection == Vector3.zero)
                dashDirection = m_orientation.forward;

            // Preserve vertical momentum during dash, only override horizontal
            Vector3 dashVelocity = dashDirection * currentDashSpeed;
            dashVelocity.y = m_velocity.y;

            m_velocity = Vector3.MoveTowards(m_velocity, dashVelocity, currentDashSpeed * Time.deltaTime);
            
            // Still apply gravity during dash
            m_velocity += gravity * Time.deltaTime;
            
            m_dashTime += Time.deltaTime;
            return true;
        }
        else if (isDashing)
        {
            isDashing = false;
        }
        
        return isDashing;
    }

    void HandleRotation()
    {
        Vector3 viewDir = transform.position - new Vector3(cam.position.x, transform.position.y, cam.position.z);
        m_orientation.forward = viewDir.normalized;

        Vector3 inputDir = m_orientation.forward * moveInput.y + m_orientation.right * moveInput.x;
        inputDir.Normalize();

        if (inputDir != Vector3.zero && !m_inputActions.Attack.IsPressed())
        {
            Vector3 forwardNoY = m_velocity;
            forwardNoY.y = 0;
            if (forwardNoY.sqrMagnitude > 0.01f)
                transform.forward = Vector3.Slerp(transform.forward, forwardNoY.normalized, Time.deltaTime * rotationSpeed);
        }
        
        if (m_inputActions.Attack.IsPressed())
            transform.forward = Vector3.Slerp(transform.forward, m_orientation.forward, Time.deltaTime * rotationSpeed);
    }

    bool WallRun()
    {
        // Exit wall run with jump
        if (m_isWallRiding && m_jumpInputPressed)
        {
            m_velocity = m_wallNormal * jumpForce + Vector3.up * jumpForce;
            m_isWallRiding = false;
            return false;
        }

        Vector3 inputDir = new Vector3(moveInput.x, 0, moveInput.y);

        // Try to start wall riding
        if (inputDir.z > 0 && !m_isWallRiding && !groundedState.isGrounded)
        {
            RaycastHit leftHit, rightHit;
            bool left = Physics.Raycast(transform.position, -transform.right, out leftHit, wallCheckDistance, wallLayers);
            bool right = Physics.Raycast(transform.position, transform.right, out rightHit, wallCheckDistance, wallLayers);
        
            leftWall = left;
            rightWall = right;

            if (left || right)
            {
                m_isWallRiding = true;
                if (left && right)
                {
                    // choose closer hit if both sides hit
                    float distL = Vector3.Distance(transform.position, leftHit.point);
                    float distR = Vector3.Distance(transform.position, rightHit.point);
                    m_wallNormal = distL <= distR ? leftHit.normal : rightHit.normal;
                }
                else if (left) m_wallNormal = leftHit.normal;
                else m_wallNormal = rightHit.normal;
            }
        
        }

        // Continue wall riding
        if (m_isWallRiding)
        {
            RaycastHit hit;
            if (Physics.Raycast(transform.position, -m_wallNormal, out hit, wallCheckDistance, wallLayers))
            {
                m_wallNormal = hit.normal;
                Vector3 wallNormalNoY = new Vector3(m_wallNormal.x, 0, m_wallNormal.z);
                m_wallRunDir = Vector3.Cross(wallNormalNoY, Vector3.up).normalized;

                if (Vector3.Dot(m_wallRunDir, transform.forward) < 0)
                    m_wallRunDir *= -1;

                // Set momentum to run along wall with upward climb
                m_velocity = m_wallRunDir * currSpeed + Vector3.up * climbMult;
                
                return true;
            }
            else
            {
                // Lost contact with wall - push off
                m_isWallRiding = false;
                m_velocity = m_wallNormal * jumpForce  + Vector3.up * jumpForce;
                return false;
            }
        }

        m_isWallRiding = false;
        return false;
    }

    #region Rail Grinding
    bool TryStartGrinding()
    {
        if (isGrinding) return true;

        Collider[] railColliders = Physics.OverlapSphere(m_railDetectionPoint.position, railDetectionRadius, railLayer);

        Rail closestRail = null;
        float closestDistance = float.MaxValue;
        float bestProgress = 0f;

        foreach (var collider in railColliders)
        {
            Rail rail = collider.GetComponent<Rail>();
            if (rail == null) continue;

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
            return true;
        }
        return false;
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

        // Use horizontal momentum magnitude for grind speed
        Vector3 horizontalMomentum = new Vector3(m_velocity.x, 0, m_velocity.z);
        grindSpeed = Mathf.Max(horizontalMomentum.magnitude, speed);

        // Determine grind direction based on forward facing
        Vector3 railDir = rail.GetDirectionOnRail(progress);
        float forwardDot = Vector3.Dot(transform.forward, railDir);
        float backwardDot = Vector3.Dot(transform.forward, -railDir);
        m_railDir = forwardDot > backwardDot ? 1f : -1f;

        // Snap to rail
        Vector3 railPosition = rail.GetPointOnRail(progress);
        transform.position = railPosition;

        // Set momentum along rail (preserve speed, change direction)
        railDir *= m_railDir;
        m_velocity = railDir * grindSpeed;
    }

    void ContinueGrinding()
    {
        if (currentRail == null)
        {
            ExitGrinding();
            return;
        }

        // Exit on jump input
        if (m_jumpInputPressed)
        {
            ExitGrinding();
            return;
        }

        // Exit if at end of non-looping rail
        if (!currentRail.isLoop && (railProgress >= 1f || railProgress <= 0f))
        {
            ExitGrinding();
            return;
        }

        // Calculate movement along rail
        Vector3 railDirection = currentRail.GetDirectionOnRail(railProgress) * m_railDir;
        m_velocity = railDirection * grindSpeed;

        // Clamp speed
        if (m_velocity.magnitude > grindSpeed)
        {
            m_velocity = m_velocity.normalized * grindSpeed;
        }

        // Move player
        transform.position = MovePlayer(m_velocity * Time.deltaTime);

        // Update progress along rail
        float railLength = currentRail.GetRailLength();
        if (railLength > 0)
        {
            float intendedDistance = grindSpeed * Time.deltaTime;
            float progressDelta = (intendedDistance / railLength) * m_railDir;
            railProgress += progressDelta;
        }

        // Correct drift from ideal rail path
        Vector3 idealRailPosition = currentRail.GetPointOnRail(railProgress);
        float driftDistance = Vector3.Distance(transform.position, idealRailPosition);
        float maxAllowedDrift = 0.5f;

        if (driftDistance > maxAllowedDrift)
        {
            // Snap back if drifted too far
            float correctedProgress;
            Vector3 closestPoint = GetClosestPointOnRail(currentRail, transform.position, out correctedProgress);
            railProgress = correctedProgress;
            idealRailPosition = closestPoint;
            driftDistance = Vector3.Distance(transform.position, idealRailPosition);
        }

        if (driftDistance > 0.1f)
        {
            // Gradually correct position
            Vector3 correctionDirection = (idealRailPosition - transform.position).normalized;
            float maxCorrectionThisFrame = 2f * Time.deltaTime;
            float correctionAmount = Mathf.Min(driftDistance, maxCorrectionThisFrame);
            transform.position += correctionDirection * correctionAmount;
        }

        SnapPlayerDown();
    }

    void ExitGrinding()
    {
        if (!isGrinding) return;

        isGrinding = false;

        // Give exit velocity
        if (currentRail != null)
        {
            Vector3 railDirection = currentRail.GetDirectionOnRail(railProgress) * m_railDir;
            
            // Preserve horizontal speed, add upward boost
            Vector3 horizontalVelocity = railDirection * Mathf.Min(grindSpeed, maxSpeed);
            m_velocity = new Vector3(horizontalVelocity.x, grindExitForce, horizontalVelocity.z);
        }

        currentRail = null;
        railProgress = 0f;
        grindSpeed = 0f;
    }
    #endregion

    void OnDestroy()
    {
        m_inputActions.Disable();
    }

    void OnDrawGizmos()
    {
        if (isGrinding && currentRail != null)
        {
            Gizmos.color = Color.yellow;
            Vector3 railPos = currentRail.GetPointOnRail(railProgress);
            Gizmos.DrawWireSphere(railPos, 0.5f);
            Vector3 railDir = currentRail.GetDirectionOnRail(railProgress) * m_railDir;
            Gizmos.DrawRay(railPos, railDir * 2f);

            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(transform.position, railPos);
            Gizmos.DrawSphere(transform.position, 0.08f);

            Gizmos.color = Color.magenta;
            float closestProgress;
            Vector3 closestPoint = GetClosestPointOnRail(currentRail, transform.position, out closestProgress);
            Gizmos.DrawWireSphere(closestPoint, 0.2f);
            Gizmos.DrawLine(transform.position, closestPoint);
        }
        
        if (m_railDetectionPoint != null)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(m_railDetectionPoint.position, railDetectionRadius);
        }
    }
}
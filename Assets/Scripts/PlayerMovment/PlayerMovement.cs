using NUnit.Framework;
using UnityEngine;
using KinematicCharacterControler;
using UnityEngine.Experimental.GlobalIllumination;



public class PlayerMovement : PlayerMovmentEngine
{
    private PlayerInputActions.PlayerActions m_inputActions;

    [Header("Movement")]
    public float speed = 5f;
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
    public bool m_isWallRiding = false;
    public float wallCheckDist = 1;
    private float m_wallTimer = 0f;
    public LayerMask wallLayers;
    public bool leftWall;
    public bool rightWall;
    private RaycastHit leftWallHit;
    private RaycastHit rightWallHit;
    private Vector3 m_wallNormal;
    private Vector3 m_wallRunDir;
    public float wallCheckDistance = 1f;

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
        m_orientation = cam;
        colors = PaintManager.instance.GetComponent<PaintColors>();
        m_inputActions = new PlayerInputActions().Player;
        m_inputActions.Enable();
    }
    void Update()
    {
        
        //HandleCursor();
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
            return;
        }
        else if (WallRun())
        {
        
        }
        else if (HandleDashing())
        {
            
        }
        else
        {
            HandleRegularMovement();
            return;
        }

        transform.position = MovePlayer(m_momentum * Time.deltaTime);   
        
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
        // if (lockCursor)
        // {
        //     Cursor.lockState = CursorLockMode.Locked;
        //     Cursor.visible = false;
        // }
        // else
        // {
        //     Cursor.lockState = CursorLockMode.None;
        //     Cursor.visible = true;
        // }
    }
    void HandleInput()
    {
        moveInput = new Vector2(m_inputActions.Move.ReadValue<Vector2>().x, m_inputActions.Move.ReadValue<Vector2>().y);

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
        inputDir = inputDir.normalized;
        // target velocity
        Vector3 targetVelocity = inputDir * currSpeed;

        if (inputDir.magnitude > 0.1f && !isDashing)
        {
            if(groundedState.isGrounded)
                m_momentum = Vector3.MoveTowards(m_momentum, targetVelocity* groundAccelMult,  currSpeed * Time.deltaTime);
            else
                m_momentum = Vector3.MoveTowards(m_momentum, targetVelocity * airAccelMult,  currSpeed * Time.deltaTime);
           
        }
        else{
            m_momentum *= 1f - (groundedState.isGrounded ? groundDrag : airDrag);
        }


        if (m_momentum.magnitude > maxSpeed && !isDashing)
        {
            m_momentum = m_momentum.normalized * maxSpeed;
        }

        // Rotate player
      
        bool onGround = CheckIfGrounded(out RaycastHit groundHit) && m_velocity.y <= 0.0f;
        bool falling = !(onGround && maxWalkAngle >= Vector3.Angle(Vector3.up, groundHit.normal));
        // Handle gravity and falling
        if (falling && !m_isWallRiding)
        {
            m_velocity += gravity * Time.deltaTime;
            m_elapsedFalling += Time.deltaTime;
        }
        else if (onGround)
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
        

        // Apply movement (combine momentum and velocity so one doesn't overwrite the other)
        Vector3 totalMovement = m_momentum + m_velocity;
        transform.position = MovePlayer(totalMovement * Time.deltaTime);
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

            Vector3 dashVelocity = dashDirection * currentDashSpeed;

            m_momentum = Vector3.MoveTowards(m_momentum, dashVelocity, currentDashSpeed * Time.deltaTime);
            m_dashTime += Time.deltaTime;
            return true;
        }
        else if (isDashing)
        {
            isDashing = false;
            return false;
        }
        return false;
    }

    void HandleRotation()
    {
        Vector3 viewDir = transform.position - new Vector3(cam.position.x, transform.position.y, cam.position.z);
        m_orientation.forward = viewDir.normalized;

        Vector3 inputDir = m_orientation.forward * moveInput.y + m_orientation.right * moveInput.x;
        inputDir = inputDir.normalized;

        if (inputDir != Vector3.zero && !m_inputActions.Attack.IsPressed())
        {
            Vector3 forwardNoY = m_momentum;
            forwardNoY.y = 0;
            transform.forward = Vector3.Slerp(transform.forward, forwardNoY.normalized, Time.deltaTime * rotationSpeed);
        }
        if (m_inputActions.Attack.IsPressed())
            transform.forward = Vector3.Slerp(transform.forward, m_orientation.forward, Time.deltaTime * rotationSpeed);
    }
    bool WallRun()
    {
       
        if (m_isWallRiding && m_jumpInputPressed)
        {
            m_momentum = m_wallNormal * jumpForce + Vector3.up * jumpForce;
            m_isWallRiding = false;
            return true;
        }

        Vector3 inputDir = new Vector3(moveInput.x, 0, moveInput.y);

        if (inputDir.z > 0)
        {
            if (!m_isWallRiding  && !groundedState.isGrounded)
            {
                RaycastHit hit;
                if (Physics.Raycast(transform.position, transform.right, out hit, wallCheckDistance, wallLayers) ||
                     Physics.Raycast(transform.position, -transform.right, out hit, wallCheckDistance, wallLayers))
                {
                    m_isWallRiding = true;
                    m_wallNormal = hit.normal;
                }
            }
        }

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
    
                m_momentum = m_wallRunDir * currSpeed;
                
                return true;
            }
            else
            {
                m_isWallRiding = false;
                m_momentum = m_wallNormal * jumpForce + Vector3.up * jumpForce;
            }
              
                
                
        }

        m_isWallRiding = false;
        return false;
    }

    #region Rail grinding
    // RAIL GRINDING SYSTEM
    bool TryStartGrinding()
    {
        if (isGrinding) return false;
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

        grindSpeed = m_momentum.magnitude < 5 ? speed : m_momentum.magnitude;

        Vector3 railDir = rail.GetDirectionOnRail(progress);

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
        m_momentum = railDirection * grindSpeed;
        if (m_momentum.magnitude > grindSpeed)
        {
            m_momentum = m_momentum.normalized * grindSpeed;
        }
        Vector3 currentPos = transform.position;
        transform.position = MovePlayer(m_momentum * Time.deltaTime);

        float railLength = currentRail.GetRailLength();
        if (railLength > 0)
        {
            float intendedDistance = grindSpeed * Time.deltaTime; 
            float progressDelta = (intendedDistance / railLength) * m_railDir;
            railProgress += progressDelta;
        }
        Vector3 idealRailPosition = currentRail.GetPointOnRail(railProgress);
        Vector3 currentRailPosition = transform.position;

        
        float driftDistance = Vector3.Distance(currentRailPosition, idealRailPosition);
        float maxAllowedDrift = 0.5f; // meters allowed between actual position and ideal rail point before clamping
        if (driftDistance > maxAllowedDrift)
        {
            // Find the progress on the rail closest to the current player position and snap progress to it.
            float correctedProgress = 0f;
            Vector3 closestPoint = GetClosestPointOnRail(currentRail, currentRailPosition, out correctedProgress);
            railProgress = correctedProgress;
            idealRailPosition = closestPoint;
            driftDistance = Vector3.Distance(currentRailPosition, idealRailPosition);
        }

        if (driftDistance > 0.1f) // small tolerance before correcting position
        {
            // Gradually pull back to rail instead of snapping; cap correction per-frame so collisions still apply.
            Vector3 correctionDirection = (idealRailPosition - currentRailPosition).normalized;
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


    void OnDestroy()
    {
          m_inputActions.Disable();   
    }
    // Visualization
    void OnDrawGizmos()
    {
        if (isGrinding && currentRail != null)
        {
            // Ideal point on the rail at current progress
            Gizmos.color = Color.yellow;
            Vector3 railPos = currentRail.GetPointOnRail(railProgress);
            Gizmos.DrawWireSphere(railPos, 0.5f);
            Vector3 railDir = currentRail.GetDirectionOnRail(railProgress) * m_railDir;
            Gizmos.DrawRay(railPos, railDir * 2f);

            // Draw line from player to ideal rail point
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(transform.position, railPos);
            Gizmos.DrawSphere(transform.position, 0.08f);

            // Draw the closest point on the rail to the player for clamping/debug
            Gizmos.color = Color.magenta;
            float closestProgress;
            Vector3 closestPoint = GetClosestPointOnRail(currentRail, transform.position, out closestProgress);
            Gizmos.DrawWireSphere(closestPoint, 0.2f);
            Gizmos.DrawLine(transform.position, closestPoint);
        }
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(m_railDetectionPoint.position, railDetectionRadius);

    }
}
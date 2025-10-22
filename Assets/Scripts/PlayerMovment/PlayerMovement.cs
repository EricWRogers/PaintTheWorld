using UnityEngine;
using KinematicCharacterControler;
using UnityEngine.Splines;

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
    private float m_maxSpeed;
    private Vector2 moveInput;
    public bool lockCursor = true;

    [Header("Momentum")]
    public float groundAccelMult = 1f;
    public float airAccelMult = 0.8f;
    public float airDrag = 0.2f;
    public float groundDrag = 0.4f;

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
    public int currJumpCount = 1;
    public int maxJumpCount => PlayerManager.instance.maxJumpCount;
    public float jumpInputElapsed = Mathf.Infinity;
    private float m_timeSinceLastJump = 0.0f;
    public bool m_jumpInputPressed = false;
    private float m_jumpBufferTime = 0.25f;
    private bool wasGrounded = false;

    [Header("Wall Riding")]
    [SerializeField] private bool m_isWallRiding = false;
    public float wallCheckDist = 1f;
    public float climbMult = 0.5f;
    public float wallGravity = 1f;
    public LayerMask wallLayers;
    public bool leftWall;
    public bool rightWall;
    private bool m_wallPaint = false;
    private Vector3 m_wallNormal;
    private Vector3 m_wallRunDir;
    public float wallCheckDistance = 1f;


    [Header("Rail Grinding")]
    public LayerMask railLayer;
    public SplineContainer splineContainer;
    private float m_timer;
    private float betweenRailTime = 0.25f;
    public float railDetectionRadius = 1.5f;
    public float railSnapDistance = 2f;
    public float minGrindSpeed = 3f;
    public float grindExitForce = 8f;
    public bool isGrinding { get; private set; }
    public Rail currentRail;
    private bool wasGrinding = false;
    public float railProgress;
    public float grindSpeed;
    public float m_railDir = 1f;
    [SerializeField] private Transform m_railDetectionPoint;
    [Header("Paint Things")]
    public GetPaintColor standPaintColor;
    private PaintColors colors;
    private float paintRotation;
    [SerializeField] private Transform paintPoint;

    void Start()
    {
        m_orientation = cam;
        colors = PaintManager.instance.GetComponent<PaintColors>();
        m_inputActions = new PlayerInputActions().Player;
        m_inputActions.Enable();

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        
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


        if ((TryStartGrinding() || isGrinding) && m_timer >= betweenRailTime)
        {
            ContinueGrinding();
        }
        else if (WallRun())
        {
            Debug.Log("handle Wall RUn IF");
            transform.position = MovePlayer(m_velocity * Time.deltaTime);
        }
        else if (HandleDashing())
        {
            Debug.Log("Handle Dashing If");
            // Dashing handles its own momentum
            transform.position = MovePlayer(m_velocity * Time.deltaTime);
        }
        else
        {
            Debug.Log("Handle Movment IF");
            HandleRegularMovement();
        }
        m_timer += Time.deltaTime;
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
        if(groundedState.isGrounded)
        {
            m_maxSpeed = standPaintColor.standingColor == colors.movementPaint ? maxSpeed : maxSpeed * 1.5f;
        }
        m_wallPaint = standPaintColor.standingColor == colors.jumpPaint;
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
        
        currSpeed = speed * m_currColorMult * m_shopMoveMult;

        Vector3 inputDir = (m_orientation.forward * moveInput.y + m_orientation.right * moveInput.x).normalized;

        bool onGround = CheckIfGrounded(out RaycastHit groundHit);
        bool canWalk = onGround && maxWalkAngle >= Vector3.Angle(Vector3.up, groundHit.normal);

        currJumpCount = onGround ? maxJumpCount : currJumpCount;

        Vector3 horizontalVel = new Vector3(m_velocity.x, 0, m_velocity.z);

        if (inputDir.sqrMagnitude > 0.01f && !isDashing)
        {
            float accelMult = canWalk ? groundAccelMult : airAccelMult;
            Vector3 targetVel = inputDir * currSpeed;

            horizontalVel = Vector3.MoveTowards(horizontalVel, targetVel, currSpeed * accelMult * Time.deltaTime);
        }
        else
        {
            float baseDrag = canWalk ? groundDrag : airDrag;
            float dragFactor = Mathf.Exp(-baseDrag * Time.deltaTime);
            horizontalVel *= dragFactor;

            
            if (canWalk)
            {
                float frictionBoost = Mathf.InverseLerp(0, currSpeed, horizontalVel.magnitude);
                horizontalVel = Vector3.Lerp(horizontalVel, Vector3.zero, (1f - frictionBoost) * baseDrag * Time.deltaTime);
            }
        }

        // Clamp horizontal speed
        if (horizontalVel.magnitude > m_maxSpeed && !isDashing)
            horizontalVel = horizontalVel.normalized * m_maxSpeed;

        m_velocity.x = horizontalVel.x;
        m_velocity.z = horizontalVel.z;

        if (!canWalk)
        {
            wasGrounded = false;
            m_velocity += gravity * Time.deltaTime;
        }
        else
        {
            if (!wasGrounded)
            {
                m_velocity.y = 0f;
                wasGrounded = true;
            }
        }        

        // === Jump handling ===
        bool canJump = ((onGround && groundedState.angle <= maxJumpAngle) || currJumpCount >0) && m_timeSinceLastJump >= jumpCooldown;
        bool attemptingJump = jumpInputElapsed <= m_jumpBufferTime;

        if (canJump && m_jumpInputPressed)
        {
            m_velocity.y = jumpForce;
            m_timeSinceLastJump = 0.0f;
            jumpInputElapsed = Mathf.Infinity;
            currJumpCount--;
        }
        else
        {
            m_timeSinceLastJump += Time.deltaTime;
        }

        transform.position = MovePlayer(m_velocity * Time.deltaTime);

        if (onGround && !attemptingJump && groundedState.angle < 10)
            SnapPlayerDown();

        Debug.Log(groundedState.angle);
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

        if ((inputDir != Vector3.zero && !m_inputActions.Attack.IsPressed()) || isGrinding)
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
            paintPoint.Rotate(0, 0, -paintRotation);
            paintRotation = 0f;
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
                    float distL = Vector3.Distance(transform.position, leftHit.point);
                    float distR = Vector3.Distance(transform.position, rightHit.point);
                    m_wallNormal = distL <= distR ? leftHit.normal : rightHit.normal;
                }
                else if (left)
                {
                    m_wallNormal = leftHit.normal;
                    paintRotation -= 90;
                }
                else
                {
                    m_wallNormal = rightHit.normal;
                    paintRotation += 90f;
                   
                }
                paintPoint.Rotate(0, 0f, paintRotation);
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

                if (m_wallPaint)
                {
                    m_velocity = m_wallRunDir * currSpeed + Vector3.up * climbMult;
                }
                else
                {
                    m_velocity =  m_wallRunDir * currSpeed + -Vector3.up * wallGravity;   
                }                    

                
                
                return true;
            }
            else
            {
                // Lost contact with wall - push off
                m_isWallRiding = false;
                m_velocity = m_wallNormal * jumpForce + Vector3.up * jumpForce;
                paintPoint.Rotate(0, 0, -paintRotation);
                paintRotation = 0f;                
                return false;
            }
        }

        m_isWallRiding = false;
        return false;
    }

    #region Rail Grinding
    bool TryStartGrinding()
    {
        // Already grinding? Continue.
        if (isGrinding) return true;
        if(wasGrinding)
        {
            wasGrinding = false;
            return false;
        }
        // Detect rails nearby
        Collider[] railColliders = Physics.OverlapSphere(m_railDetectionPoint.position, railDetectionRadius, railLayer);
        if (railColliders.Length == 0) return false;

    
        // Search for the closest spline point
        foreach (var collider in railColliders)
        {
            splineContainer = collider.GetComponent<SplineContainer>();
            if (splineContainer == null || splineContainer.Splines.Count == 0) continue;


        }
    
        // If a valid spline was found within snap distance, start grinding
        if (splineContainer != null)
        {
           
            StartGrinding();
            return true;

        }
    
        return false;
    }

    void StartGrinding()
    {
        isGrinding = true;

        Vector3 position = FindClosestPointOnSpline(out float progress);

        railProgress = progress;

        // Use horizontal velocity magnitude as grind speed
        Vector3 horizontalVelocity = new Vector3(m_velocity.x, 0, m_velocity.z);
        grindSpeed = Mathf.Max(horizontalVelocity.magnitude, minGrindSpeed);


        Vector3 pos = splineContainer.Splines[0].EvaluatePosition(progress);
        transform.position = position + splineContainer.transform.position + Vector3.up * 1.4f;

    }

     void ContinueGrinding()
    {
        if (splineContainer == null || splineContainer.Splines.Count == 0)
        {
            
            ExitGrinding();
            return;
        }

        if (m_jumpInputPressed)
        {
            ExitGrinding();
            return;
        }

        var spline = splineContainer.Splines[0];
        railProgress += (m_velocity.magnitude * Time.deltaTime) / spline.GetLength();

  
        Vector3 position = (Vector3)spline.EvaluatePosition(railProgress);
        

        if (m_velocity.magnitude < 5) m_velocity = m_velocity.normalized * grindSpeed;
        Vector3 dir = (transform.position - position).normalized;
        transform.position = position + splineContainer.transform.position+ Vector3.up *1.4f;


        float splineLength = splineContainer.Splines[0].GetLength();

        float distanceDelta = grindSpeed * Time.deltaTime;
        railProgress += (distanceDelta / splineLength);
        if (railProgress >= 0.999)   ExitGrinding();
        else SnapPlayerDown();
    }

    void ExitGrinding()
    {
        Debug.Log("Exited Grinding");
        if (!isGrinding) return;
        isGrinding = false;
        wasGrinding = true;
        // Give exit velocity
        if (splineContainer != null)
        {
            m_velocity = Vector3.up * jumpForce;

        }
        transform.position = MovePlayer(m_velocity * Time.deltaTime);
        m_timer = 0f;
        splineContainer = null;
        currentRail = null;
        railProgress = 0f;
        grindSpeed = 0f;
    }
    

    Vector3 FindClosestPointOnSpline(out float bestT)
    {
        int samples = 100;
        float minDist = float.MaxValue;
        bestT = 0f;
        Vector3 bestPoint = Vector3.zero;

        for (int i = 0; i <= samples; i++)
        {
            float t = (float)i / samples;
            Vector3 p = splineContainer.EvaluatePosition(t);
            float d = (transform.position - p).sqrMagnitude;

            if (d < minDist)
            {
                minDist = d;
                bestT = t;
                bestPoint = p;
            }
        }

        return bestPoint;
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

        }
        
        if (m_railDetectionPoint != null)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(m_railDetectionPoint.position, railDetectionRadius);
        }
    }
}
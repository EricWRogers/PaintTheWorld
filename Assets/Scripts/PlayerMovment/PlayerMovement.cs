using UnityEngine;
using KinematicCharacterControler;
using UnityEngine.Splines;

#region Custom Edtior for Unity
#if UNITY_EDITOR
using UnityEditor;

[CustomEditor(typeof(PlayerMovement))]
public class PlayerMOvmentEditor : Editor
{
    private bool showMovement = true;
    private bool showMomentum = false;
    private bool showDashing = false;
    private bool showJump = false;
    private bool showWall = false;
    private bool showRail = false;
    private bool showPaint = false;
    private bool showEngine = false;

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        
        showEngine = EditorGUILayout.Foldout(showEngine, "Engine Stuff/ Gravity/ Collsion");
        if(showEngine)
        {
            EditorGUILayout.PropertyField(serializedObject.FindProperty("capsule"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("collisionLayers"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("skinWidth"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("maxBounces"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("maxSlopeAngle")); 
            EditorGUILayout.PropertyField(serializedObject.FindProperty("downSlopeMult")); 
            EditorGUILayout.PropertyField(serializedObject.FindProperty("upSlopeMult")); 
            EditorGUILayout.PropertyField(serializedObject.FindProperty("defaultGroundCheck"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("defaultGroundedDistance"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("snapDownDistance"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("shouldSnapDown"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("gravity"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("DefaultPhysicsMat"));
            EditorGUILayout.Space();
        }

        // Movement
        showMovement = EditorGUILayout.Foldout(showMovement, "Movement");
        if (showMovement)
        {
            EditorGUILayout.PropertyField(serializedObject.FindProperty("cam"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("speed"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("maxSpeed"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("rotationSpeed"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("movementColorMult"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("lockCursor"));
        }

        // Momentum
        showMomentum = EditorGUILayout.Foldout(showMomentum, "Momentum");
        if (showMomentum)
        {
            EditorGUILayout.PropertyField(serializedObject.FindProperty("groundAccelMult"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("airAccelMult"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("airDrag"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("groundDrag"));
        }

        // Dashing
        showDashing = EditorGUILayout.Foldout(showDashing, "Dashing");
        if (showDashing)
        {

            EditorGUILayout.PropertyField(serializedObject.FindProperty("dashSpeedCurve"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("dashSpeed"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("dashDuration"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("dashCooldown"));
        }

        // Jump
        showJump = EditorGUILayout.Foldout(showJump, "Jump");
        if (showJump)
        {
            EditorGUILayout.PropertyField(serializedObject.FindProperty("jumpForce"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("maxJumpAngle"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("jumpCooldown"));
        }

        // Wall Riding
        showWall = EditorGUILayout.Foldout(showWall, "Wall Riding");
        if (showWall)
        {
            EditorGUILayout.PropertyField(serializedObject.FindProperty("wallCheckDist"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("wallCheckDistance"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("climbMult"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("wallGravity"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("wallLayers"));
        }

        // Rail Grinding
        showRail = EditorGUILayout.Foldout(showRail, "Rail Grinding");
        if (showRail)
        {
            EditorGUILayout.PropertyField(serializedObject.FindProperty("railLayer"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("splineContainer"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("railDetectionRadius"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("railSnapDistance"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("minGrindSpeed"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("grindExitForce"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_railDetectionPoint"));
        }

        // Paint
        showPaint = EditorGUILayout.Foldout(showPaint, "Paint");
        if (showPaint)
        {
            EditorGUILayout.PropertyField(serializedObject.FindProperty("standPaintColor"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("paintPoint"));
        }

        // Apply changes
        serializedObject.ApplyModifiedProperties();
    }
}
#endif
#endregion

public class PlayerMovement : PlayerMovmentEngine
{
    private PlayerInputActions.PlayerActions m_inputActions;

    [Header("Movement")]
    [Tooltip("Base Movment Speed")]
    public float speed = 5f;

    [Tooltip("Max Movment Speed if not on Movment Paint")]
    public float maxSpeed = 20f; 

    [Tooltip("How fast the Player Rotates direction")]
    public float rotationSpeed = 5f;

    [Tooltip("How much Speed Paint effects Movment Speed: This is a multiplier")]
    public float movementColorMult = 2f;
    private float currSpeed;
    private Transform m_orientation;

    public Transform cam;
    
    private float m_currColorMult = 1f;
    private float m_shopMoveMult => PlayerManager.instance.stats.skills[3].currentMult;
    private float m_maxSpeed;
    private Vector2 moveInput;
    public bool lockCursor = true;

    [Header("Momentum")]
    [Tooltip("How fast you can Aceelerate towards a direction on the ground")]
    public float groundAccelMult = 1f;

    [Tooltip("How fast you can Aceelerate towards a direction in The Air")]
    public float airAccelMult = 0.8f;

    [Tooltip("Friction while in the air")]
    public float airDrag = 0.2f;

    [Tooltip("Friction while on the Ground")]
    public float groundDrag = 0.4f;

    [Header("Dashing")]
    [Tooltip("Shows the rate of acerleration over the time of the dash")]
    public AnimationCurve dashSpeedCurve;
    public float dashSpeed = 20f;

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
    [ReadOnly] public bool m_isWallRiding = false;
    [ReadOnly] public bool leftWall;
    [ReadOnly] public bool rightWall;

    public float wallCheckDist = 1f;
    public float climbMult = 0.5f;
    public float wallGravity = 1f;
    public LayerMask wallLayers;

    private bool m_wallPaint = false;
    private Vector3 m_wallNormal;
    private Vector3 m_wallRunDir;
    public float wallCheckDistance = 1f;


    [Header("Rail Grinding")]
    public LayerMask railLayer;
    public SplineContainer splineContainer;
    private float m_timer;
    private float betweenRailTime = 0.5f;
    public float railDetectionRadius = 1.5f;
    public float railSnapDistance = 2f;
    public float minGrindSpeed = 3f;
    public float grindExitForce = 8f;
    [ReadOnly] public bool isGrinding;
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
        CheckIfGrounded(out RaycastHit _); 
        HandleRotation();


        if ((TryStartGrinding() || isGrinding) && m_timer > betweenRailTime)
        {
            ContinueGrinding();
        }
        else if (WallRun())
        {
            splineContainer = null;
            transform.position = MovePlayer(m_velocity * Time.deltaTime);
        }
        else if (HandleDashing())
        {
            splineContainer = null;
            // Dashing handles its own momentum
            transform.position = MovePlayer(m_velocity * Time.deltaTime);
        }
        else
        {
            splineContainer = null;
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
            m_maxSpeed = standPaintColor.standingColor == colors.movementPaint ? maxSpeed : maxSpeed * 4;
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
        bool canWalk = onGround && maxSlopeAngle >= Vector3.Angle(Vector3.up, groundHit.normal);

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
        if (horizontalVel.magnitude > m_maxSpeed && !isDashing )
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
    
    #region Wall Running
    bool WallRun()
    {
        // Exit wall run with jump
        if (m_isWallRiding && (m_jumpInputPressed || Physics.Raycast(transform.position, m_velocity.normalized, wallCheckDist, collisionLayers)))
        {
            m_velocity = m_wallNormal * jumpForce + Vector3.up * jumpForce;
            m_isWallRiding = false;
            paintPoint.Rotate(0, 0, -paintRotation);
            paintRotation = 0f;
            return false;
        }
        if(groundedState.isGrounded && m_isWallRiding){
            m_velocity += m_wallNormal * 2;
            m_isWallRiding = false;
            paintPoint.Rotate(0, 0, -paintRotation);
            paintRotation = 0f;
            return false;
        }

        Vector3 inputDir = new Vector3(moveInput.x, 0, moveInput.y);

        // Try to start wall riding
        if (!m_isWallRiding && !groundedState.isGrounded)
        {

                if(WallCheck(out RaycastHit hit))
                {
                    m_isWallRiding = true;
                    m_wallNormal = hit.normal;
                    if (leftWall)
                    {
                        paintRotation -= 90;
                    }
                    else if(rightWall)
                    {
                        paintRotation += 90;
                    }
                }
                paintPoint.Rotate(0, 0f, paintRotation);
            

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
                    m_velocity = m_wallRunDir * currSpeed + -Vector3.up * wallGravity;
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

    public bool WallCheck(out RaycastHit hit)
    {
        hit = new RaycastHit();

        for (int i = -60; i <= 60; i += 5)
        {
            if (Physics.Raycast(transform.position, -transform.right, out hit, wallCheckDistance, wallLayers))
            {
                leftWall = true;
                return true;
            }
        }
        for(int i = -20; i <= 20; i+=5)
        {
            if(Physics.Raycast(transform.position, transform.right, out hit, wallCheckDistance, wallLayers))
            {
                rightWall = true;
                return true;
            }
        }
        

        return false;
    }
    #endregion

    #region Rail Grinding
    bool TryStartGrinding()
    {
        if (isGrinding) return true;
        // Already grinding? Continue.

        if (wasGrinding)
        {
            wasGrinding = false;
            return false;
        }
        splineContainer = null;
        // Detect rails nearby
        Collider[] railColliders = Physics.OverlapSphere(m_railDetectionPoint.position, railDetectionRadius, railLayer);
        if (railColliders.Length == 0) return false;


        // Search for the closest spline point
        foreach (var collider in railColliders)
        {
            splineContainer = collider.GetComponent<SplineContainer>();
            if (splineContainer == null || splineContainer.Splines.Count == 0)   continue;
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
        var splineRef = splineContainer.Splines[0];

        Vector3 closest = FindClosestPointOnSpline(out float progress);
        railProgress = progress;

        grindSpeed = Mathf.Max(m_velocity.magnitude, minGrindSpeed);


        Vector3 tangent = GetSplineTangentAt(splineRef, railProgress);
        if (tangent.sqrMagnitude < 0.0001f) tangent = Vector3.forward;


        float dot = Vector3.Dot(m_velocity.normalized, tangent.normalized);
        m_railDir = dot >= 0f ? 1f : -1f;

        
        Vector3 splinePos = (Vector3)splineRef.EvaluatePosition(railProgress);
        Vector3 worldSplinePos = splineContainer.transform.position + splinePos + Vector3.up * 1.4f;

        Vector3 snapDelta = worldSplinePos - transform.position;
        transform.position = MovePlayer(snapDelta); // MovePlayer returns new pos usually; keep consistent usage
        if (m_velocity.magnitude < minGrindSpeed)
        {
            m_velocity = m_velocity.normalized * minGrindSpeed;
        }
        m_velocity = tangent.normalized * grindSpeed * m_railDir;


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
            var splineRef = splineContainer.Splines[0];
            Vector3 tangent = GetSplineTangentAt(splineRef, railProgress).normalized * m_railDir;
            m_velocity = tangent * grindExitForce + Vector3.up * jumpForce;
            ExitGrinding();
            return;
        }

        var splineRef2 = splineContainer.Splines[0];

        
        Vector3 tangentHere = GetSplineTangentAt(splineRef2, railProgress).normalized * m_railDir;


        float speedAlongTangent = Vector3.Dot(m_velocity, tangentHere);

        if (speedAlongTangent < 0.01f)
            speedAlongTangent = grindSpeed;


        float splineLen = splineRef2.GetLength();
        if (splineLen <= 0.001f) splineLen = 1f;
        railProgress += (speedAlongTangent * Time.deltaTime) / splineLen * m_railDir;
        railProgress = Mathf.Clamp01(railProgress);

        // Move using controller's MovePlayer so collisions/contacts are handled consistently
        m_velocity = tangentHere * speedAlongTangent;
        Vector3 move = m_velocity * Time.deltaTime;
        transform.position = MovePlayer(move);

        // If near end, exit
        if (railProgress >= 0.999f || railProgress <= 0.001f)
        {
            ExitGrinding();
            return;
        }

        // Keep the player snapped close to the spline laterally to avoid drift:
        Vector3 splinePos = (Vector3)splineRef2.EvaluatePosition(railProgress);
        Vector3 worldSplinePos = splineContainer.transform.position + splinePos + Vector3.up * 1.4f;
        // Only correct lateral offset (project delta onto plane perpendicular to tangent)
        Vector3 delta = worldSplinePos - transform.position;
        Vector3 lateral = delta - Vector3.Project(delta, tangentHere);
        // Apply a small correction to pull player onto spline smoothly (not teleport)
        float correctionStrength = Mathf.Clamp01(5f * Time.deltaTime); // tweak if needed
        transform.position = MovePlayer(lateral * correctionStrength);

        SnapPlayerDown();
    }

    void ExitGrinding()
    {

        if (!isGrinding || wasGrinding) return;

        isGrinding = false;
        wasGrinding = true;

        
        // Give exit velocity
        if (splineContainer != null)
        {
            m_velocity += Vector3.up * jumpForce;

        }
        //transform.position = MovePlayer(m_velocity * Time.deltaTime);
        m_timer = 0f;
        splineContainer = null;
        railProgress = 0f;
        grindSpeed = 0f;
        wasGrinding = false;


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
        Vector3 GetSplineTangentAt(Spline spline, float t, float deltaT = 0.001f)
    {
        // Numerical tangent: sample a small step forward (clamp) to approximate derivative.
        float t2 = Mathf.Clamp01(t + deltaT);
        Vector3 p1 = (Vector3)spline.EvaluatePosition(t);
        Vector3 p2 = (Vector3)spline.EvaluatePosition(t2);

        Vector3 tangent = (p2 - p1).normalized;
        if (tangent.sqrMagnitude < 0.0001f)
        {
            // Fallback: try backward sample
            float t0 = Mathf.Clamp01(t - deltaT);
            p2 = (Vector3)spline.EvaluatePosition(t0);
            tangent = (p1 - p2).normalized;
        }
        return tangent;
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
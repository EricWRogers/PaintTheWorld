using UnityEngine;
using KinematicCharacterControler;
using UnityEngine.Splines;
using Unity.VisualScripting;
using UnityEditor.EditorTools;






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
    private bool showAnimations = false;

    // hi there sorry for touching your script without asking, was trying to tie our animations to the info in here. 
    // feel free to delete everything to do with the animations in here

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
            EditorGUILayout.PropertyField(serializedObject.FindProperty("jumpHoldMult"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("maxJumpHoldTime"));
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

        showAnimations = EditorGUILayout.Foldout(showAnimations, "Animations");
        if (showAnimations)
        {
            EditorGUILayout.PropertyField(serializedObject.FindProperty("animator"));
        }
        serializedObject.ApplyModifiedProperties();
    }
}
#endif
#endregion

public class PlayerMovement : PlayerMovmentEngine
{
    // animator variables
    [SerializeField] private Animator animator;
    //private bool Grounded;
   // private bool Moving;

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
    private Vector3 m_lasPos;

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
    private bool m_dashBack = false;
    private float m_dashStartSpeed;
    private Vector3 dashDir;

    [Header("Jump Settings")]
    [Tooltip("Default Upward force applied when jumping")]
    public float jumpForce = 5.0f;
    public float maxJumpAngle = 80f;
    public float jumpCooldown = 0.25f;
    public int currJumpCount = 1;
    public int maxJumpCount => PlayerManager.instance.maxJumpCount;
    public float jumpInputElapsed = Mathf.Infinity;
    private float m_timeSinceLastJump = 0.0f;
    public bool m_jumpInputPressed = false;
    // True only on the frame jump was pressed (latched until consumed).
    private bool m_jumpPressedThisFrame = false;

    private float m_jumpBufferTime = 0.25f;

    private bool wasGrounded = false;


    [Tooltip("The max mult to jumping when Holding for jump")]
    public float jumpHoldMult = 1.5f;

    [Tooltip("Time till you get the full Jump Hold Mult to the jump")]
    public float maxJumpHoldTime = 2f;

    private bool m_jumpHoldActive = false;
    private float m_jumpHoldTimer = 0f;




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

    private enum MoveState
    {
        Grounded,
        Air,
        Dash,
        WallRide,
        Grind
    };

    [SerializeField] private MoveState state;
    private MoveState prevState;

    void Start()
    {
        //to find animator
       // animator = gameObject.GetComponent<Animator>();
       // Moving = false;
       // Grounded = true;

        m_orientation = cam;
        colors = PaintManager.instance.GetComponent<PaintColors>();
        m_inputActions = new PlayerInputActions().Player;
        m_inputActions.Enable();

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        m_lasPos = transform.position;
        
    }

    void Update()
        
    {
      //  HandleAnimations();
        HandleInput();
        HandlePaintColor();
        HandleFOV();
        m_timeSinceLastDash += Time.deltaTime;
        if (PlayerInputLock.Locked) return;  //this is for locking player movement on the kiosk camera for the shop

    }


    void FixedUpdate()
    {
        if (PlayerInputLock.Locked) return;  //this is for locking player movement on the kiosk camera for the shop
   
        bool onGround = CheckIfGrounded(out RaycastHit _);
        HandleRotation();

        // Decide state ONCE (and only call WallRun/HandleDashing/TryStartGrinding once)
        EvaluateTransitions(onGround);
        HandleAnimations();

        // Tick current state
        switch (state)
        {
            case MoveState.Grind:
                // grinding handles its own movement inside ContinueGrinding()
                ContinueGrinding();
                break;

            case MoveState.WallRide:
                splineContainer = null;
                transform.position = MovePlayer(m_velocity * Time.deltaTime);
                break;

            case MoveState.Dash:
                splineContainer = null;
                // Dashing handles its own momentum
                transform.position = MovePlayer(m_velocity * Time.deltaTime);
                break;

            case MoveState.Grounded:
            case MoveState.Air:
            default:
                splineContainer = null;
                HandleRegularMovement();
                break;
        }

        m_timer += Time.deltaTime;
    }
        // Grind > WallRide > Dash > Regular (Grounded/Air)
    private void EvaluateTransitions(bool onGround)
    {
        // Grind
        if ((TryStartGrinding() || isGrinding) && m_timer > betweenRailTime)
        {
            SetState(MoveState.Grind);
            return;
        }

        // Wall ride
        if (WallRun())
        {
            SetState(MoveState.WallRide);
            return;
        }

        // Dash
        if (HandleDashing())
        {
            SetState(MoveState.Dash);
            return;
        }

        // Regular base locomotion
        SetState(onGround ? MoveState.Grounded : MoveState.Air);
    }

    private void SetState(MoveState next)
    {
        if (state == next) return;
        prevState = state;
        state = next;
    }

    // programing animations -- by cleo
    public void HandleAnimations()
   {
        Vector2 horzVelocity = new Vector2(m_velocity.x, m_velocity.z);
       if (horzVelocity.magnitude > 2f)
           animator.SetBool("Moving", true);

        else
            animator.SetBool("Moving", false);

        if (groundedState.isGrounded)
            animator.SetBool("Grounded", true);
    
        else
            animator.SetBool("Grounded", false);

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

    void HandleInput()
    {
        moveInput = m_inputActions.Move.ReadValue<Vector2>();

        m_jumpInputPressed = m_inputActions.Jump.IsPressed();
        // Buffer based on a *press*, not on being held (prevents auto-jumping / supports variable jump height cleanly)
        if (m_inputActions.Jump.WasPressedThisFrame())
            jumpInputElapsed = 0.0f;
        else
            jumpInputElapsed += Time.deltaTime;
        
        m_dashInputPressed = m_inputActions.Dash.WasPressedThisFrame();
    }

    void HandleRegularMovement()
    {
        currSpeed = speed * m_currColorMult * m_shopMoveMult;

        // Raw input direction (we’ll re-project it onto the slope when grounded)
        Vector3 inputDir = (m_orientation.forward * moveInput.y + m_orientation.right * moveInput.x).normalized;

        bool onGround = CheckIfGrounded(out RaycastHit groundHit);
        bool canWalk = onGround && maxSlopeAngle >= Vector3.Angle(Vector3.up, groundHit.normal);

        currJumpCount = onGround ? maxJumpCount : currJumpCount;

      
        // banking/slope math (might not work)
        Vector3 groundNormal = canWalk ? groundHit.normal : Vector3.up;

        if (canWalk)
        {
            Vector3 slopeForward = Vector3.ProjectOnPlane(m_orientation.forward, groundNormal);
            Vector3 slopeRight   = Vector3.ProjectOnPlane(m_orientation.right, groundNormal);


            if (slopeForward.sqrMagnitude > 0.0001f) slopeForward.Normalize();
            if (slopeRight.sqrMagnitude > 0.0001f) slopeRight.Normalize();

            Vector3 slopeInputDir = slopeForward * moveInput.y + slopeRight * moveInput.x;
            if (slopeInputDir.sqrMagnitude > 0.0001f) slopeInputDir.Normalize();

            inputDir = slopeInputDir;
        }

        // Instead of flattening to XZ, keep it tangent to the current ground plane
        Vector3 rawVel = m_velocity;
        Vector3 horizontalVel = canWalk
            ? Vector3.ProjectOnPlane(rawVel, groundNormal)
            : new Vector3(m_velocity.x, 0, m_velocity.z);

        if (canWalk && rawVel.sqrMagnitude > 0.0001f && horizontalVel.sqrMagnitude > 0.0001f)
        {
            float bankAngle = Vector3.Angle(rawVel, horizontalVel);     
            float t = Mathf.Clamp01(bankAngle / 45f);                   
            float bankMult = Mathf.Lerp(1f, 0.85f, t);                  
            horizontalVel *= bankMult;
        }

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

        //// Clamp “surface speed” (this now clamps along the ramp, not just flat XZ)
        //if (horizontalVel.magnitude > m_maxSpeed && !isDashing)
        //    horizontalVel = horizontalVel.normalized * m_maxSpeed;

        // On walkable ground, velocity should live on the surface plane (includes vertical on ramps).
        // In air, keep your original pattern (preserve Y separately).
        if (canWalk)
        {
            m_velocity = horizontalVel;
        }
        else
        {
            m_velocity.x = horizontalVel.x;
            m_velocity.z = horizontalVel.z;
        }

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

        // Jump handling
        bool canJump = ((onGround && groundedState.angle <= maxJumpAngle) || currJumpCount > 0) && m_timeSinceLastJump >= jumpCooldown;
        bool attemptingJump = jumpInputElapsed <= m_jumpBufferTime;

        if (canJump && attemptingJump)
        {
            if(m_jumpHoldActive == true)
                return;
            //m_velocity.y = jumpForce;
            m_timeSinceLastJump = 0.0f;
            jumpInputElapsed = Mathf.Infinity;
            currJumpCount--;

            // Start variable jump hold window
            m_jumpHoldActive = true;
            m_jumpHoldTimer = 0f;
        }
        else
        {
            m_timeSinceLastJump += Time.deltaTime;
        }

        // Variable jump height: while holding jump shortly after jump start, add extra lift (capped)
        if (m_jumpHoldActive)
        {
            if (!m_jumpInputPressed || m_jumpHoldTimer >= maxJumpHoldTime )
            {
                float finalJumpForce = Mathf.Lerp(jumpForce, jumpForce * jumpHoldMult, m_jumpHoldTimer / maxJumpHoldTime);
                m_velocity.y = finalJumpForce;
                m_jumpHoldActive = false;
            }
            else
            {
                m_jumpHoldTimer += Time.deltaTime;
            }
        }

        transform.position = MovePlayer(m_velocity * Time.deltaTime);

        if (onGround && !attemptingJump && groundedState.angle > 10 && transform.position.y - m_lasPos.y < 0.001f)
            SnapPlayerDown();

        m_velocity = (transform.position - m_lasPos).normalized * m_velocity.magnitude;
        m_lasPos = transform.position;
    }
    #region Dashing/Dodge
    bool HandleDashing()
    {
        bool canDash = m_timeSinceLastDash >= dashCooldown;
        

        if(canDash && m_dashInputPressed)
        {
           if(moveInput.sqrMagnitude > 0.01f)
            {
                dashDir = (m_orientation.forward * moveInput.y + m_orientation.right * moveInput.x).normalized;
            }

            
            isDashing = false;
            m_dashTime = 0f;
            m_timeSinceLastDash = 0f;
            m_dashStartSpeed = m_velocity.magnitude;

            m_velocity = dashDir * (dashSpeed + m_dashStartSpeed);

            
            
        }
      
        isDashing = false;
        return isDashing;
    }


    #endregion
    

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

        for (int i = -40; i <= 40; i += 5)
        {
            if (Physics.Raycast(transform.position, -transform.right, out hit, wallCheckDistance, wallLayers))
            {
                leftWall = true;
                return true;
            }
        }
        for(int i = -40; i <= 40; i+=5)
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

        if (m_railDetectionPoint == null) return false;

        // Detect rails nearby
        Collider[] railColliders = Physics.OverlapSphere(m_railDetectionPoint.position, railDetectionRadius, railLayer);
        if (railColliders.Length == 0) return false;

        // Pick the closest rail (prevents snapping to a random rail when multiple overlap).
        SplineContainer best = null;
        float bestDistSqr = float.MaxValue;

        foreach (var col in railColliders)
        {
            var sc = col.GetComponent<SplineContainer>();
            if (sc == null || sc.Splines == null || sc.Splines.Count == 0) continue;

            FindClosestPointOnSpline(sc, transform.position, out float t, out float distSqr);
            if (distSqr < bestDistSqr)
            {
                bestDistSqr = distSqr;
                best = sc;
            }
        }

        if (best == null) return false;

       
        

        splineContainer = best;
        StartGrinding();
        return true;
    }

    void StartGrinding()
    {
        isGrinding = true;

        if (splineContainer == null || splineContainer.Splines == null || splineContainer.Splines.Count == 0)
        {
            ExitGrinding();
            return;
        }

        var splineRef = splineContainer.Splines[0];

        // Find closest point on spline in WORLD space (handles rotated rails).
        FindClosestPointOnSpline(splineContainer, transform.position, out float progress, out _);
        railProgress = progress;

        grindSpeed = Mathf.Max(m_velocity.magnitude, minGrindSpeed);

        // Tangent from Spline.EvaluatePosition is in spline LOCAL space -> convert to world.
        Vector3 localTangent = GetSplineTangentAt(splineRef, railProgress);
        if (localTangent.sqrMagnitude < 0.0001f) localTangent = Vector3.forward;

        Vector3 worldTangent = splineContainer.transform.TransformDirection(localTangent);
        if (worldTangent.sqrMagnitude < 0.0001f) worldTangent = transform.forward;

        // Choose rail direction using horizontal approach (prevents vertical velocity from flipping direction).
        Vector3 approach = m_velocity;
        approach.y = 0f;
        Vector3 tH = worldTangent;
        tH.y = 0f;

        if (approach.sqrMagnitude < 0.0001f) approach = transform.forward;
        if (tH.sqrMagnitude < 0.0001f) tH = transform.forward;

        float dot = Vector3.Dot(approach.normalized, tH.normalized);
        m_railDir = dot >= 0f ? 1f : -1f;

        // Snap player to spline in WORLD space (TransformPoint handles rail rotation).
        Vector3 localSplinePos = (Vector3)splineRef.EvaluatePosition(railProgress);
        Vector3 worldSplinePos = splineContainer.transform.TransformPoint(localSplinePos) + Vector3.up * 1.4f;

        Vector3 snapDelta = worldSplinePos - transform.position;
        transform.position = MovePlayer(snapDelta);

        // Set velocity along world tangent
        m_velocity = worldTangent.normalized * grindSpeed * m_railDir;
    }

    void ContinueGrinding()
    {
        if (splineContainer == null || splineContainer.Splines == null || splineContainer.Splines.Count == 0)
        {
            ExitGrinding();
            return;
        }

        // Jump off rail: use a one-shot press (not "held") so it fires reliably once.
        if (m_jumpPressedThisFrame)
        {
            m_jumpPressedThisFrame = false;

            var splineRef = splineContainer.Splines[0];

            Vector3 localTangent = GetSplineTangentAt(splineRef, railProgress);
            if (localTangent.sqrMagnitude < 0.0001f) localTangent = Vector3.forward;

            Vector3 worldTangent = splineContainer.transform.TransformDirection(localTangent).normalized * m_railDir;

            // Keep your original jump math (tangent * grindExitForce + up * jumpForce)
            m_velocity = worldTangent * grindExitForce + Vector3.up * jumpForce;

            ExitGrinding();
            return;
        }

        var splineRef2 = splineContainer.Splines[0];

        // Full 3D world tangent at current progress (respecting rail slope + container rotation)
        Vector3 localTangentHere = GetSplineTangentAt(splineRef2, railProgress);
        if (localTangentHere.sqrMagnitude < 0.0001f) localTangentHere = Vector3.forward;

        Vector3 tangentHere = splineContainer.transform.TransformDirection(localTangentHere).normalized * m_railDir;

        // Use full velocity to get how fast we're moving along tangent
        float speedAlongTangent = Vector3.Dot(m_velocity, tangentHere);

        // If speed is tiny or reversed, fall back to stored grindSpeed to keep motion consistent
        if (speedAlongTangent < 0.01f)
            speedAlongTangent = grindSpeed;

        // Advance progress based on actual distance along spline (convert world speed to t-space)
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
        Vector3 localSplinePos2 = (Vector3)splineRef2.EvaluatePosition(railProgress);
        Vector3 worldSplinePos = splineContainer.transform.TransformPoint(localSplinePos2) + Vector3.up * 1.4f;

        Vector3 delta = worldSplinePos - transform.position;
        Vector3 lateral = delta - Vector3.Project(delta, tangentHere);

        // Apply lateral correction gently (not teleport)
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
            m_velocity += Vector3.up * grindExitForce;
        }
        transform.position = MovePlayer(m_velocity * Time.deltaTime);

        m_timer = 0f;
        splineContainer = null;
        railProgress = 0f;
        grindSpeed = 0f;

        // Note: cooldown gating is handled elsewhere; clearing here prevents the flag from blocking indefinitely.
        wasGrinding = false;
    }

    // Finds the closest point on a spline container in WORLD space (handles container rotation).
    void FindClosestPointOnSpline(SplineContainer container, Vector3 worldPos, out float bestT, out float bestDistSqr)
    {
        int samples = 100;
        bestDistSqr = float.MaxValue;
        bestT = 0f;

        if (container == null || container.Splines == null || container.Splines.Count == 0)
            return;

        var spline = container.Splines[0];

        for (int i = 0; i <= samples; i++)
        {
            float t = (float)i / samples;

            Vector3 localP = (Vector3)spline.EvaluatePosition(t);
            Vector3 p = container.transform.TransformPoint(localP);

            float d = (worldPos - p).sqrMagnitude;

            if (d < bestDistSqr)
            {
                bestDistSqr = d;
                bestT = t;
            }
        }
    }

    // Legacy call site compatibility (kept).
    Vector3 FindClosestPointOnSpline(out float bestT)
    {
        FindClosestPointOnSpline(splineContainer, transform.position, out bestT, out _);
        var spline = splineContainer.Splines[0];
        Vector3 localP = (Vector3)spline.EvaluatePosition(bestT);
        return splineContainer.transform.TransformPoint(localP);
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

using UnityEngine;
using KinematicCharacterControler;
using UnityEngine.Splines;
using Unity.VisualScripting;



#region Custom Editor for Unity
#if UNITY_EDITOR
using UnityEditor.EditorTools;
using UnityEditor;

[CustomEditor(typeof(PlayerMovement))]
public class PlayerMovmentEditor : Editor
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

        showMomentum = EditorGUILayout.Foldout(showMomentum, "Momentum");
        if (showMomentum)
        {
            EditorGUILayout.PropertyField(serializedObject.FindProperty("groundAccelMult"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("airAccelMult"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("airDrag"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("groundDrag"));
        }

        showDashing = EditorGUILayout.Foldout(showDashing, "Dashing");
        if (showDashing)
        {
            EditorGUILayout.PropertyField(serializedObject.FindProperty("dashSpeedCurve"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("dashSpeed"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("dashDuration"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("dashCooldown"));
        }

        showJump = EditorGUILayout.Foldout(showJump, "Jump");
        if (showJump)
        {
            EditorGUILayout.PropertyField(serializedObject.FindProperty("jumpForce"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("maxJumpAngle"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("jumpCooldown"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("jumpHoldMult"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("maxJumpHoldTime"));
        }

        showWall = EditorGUILayout.Foldout(showWall, "Wall Riding");
        if (showWall)
        {
            EditorGUILayout.PropertyField(serializedObject.FindProperty("wallCheckDist"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("wallCheckDistance"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("climbMult"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("wallGravity"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("wallLayers"));
        }

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

        showPaint = EditorGUILayout.Foldout(showPaint, "Paint");
        if (showPaint)
        {
            EditorGUILayout.PropertyField(serializedObject.FindProperty("standPaintColor"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("paintPoint"));
        }

        showAnimations = EditorGUILayout.Foldout(showAnimations, "Animations");
        if (showAnimations)
        {
            EditorGUILayout.PropertyField(serializedObject.FindProperty("animator"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("playerModel"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("modelRotationSpeed"));
        }

        serializedObject.ApplyModifiedProperties();
    }
}
#endif
#endregion


public class PlayerMovement : PlayerMovmentEngine
{
    // -------------------------------------------------------------------------
    #region Fields & Variables

    // Animator
    [SerializeField] private Animator animator;

    // Input
    private PlayerInputActions.PlayerActions m_inputActions;
    private Vector2 moveInput;
    private bool m_jumpInputPressed = false;
    private bool m_dashInputPressed = false;

    // Movement
    [Header("Movement")]
    [Tooltip("Base Movement Speed")]
    public float speed = 5f;

    [Tooltip("Max Movement Speed if not on Movement Paint")]
    public float maxSpeed = 20f;

    [Tooltip("How fast the Player Rotates direction")]
    public float rotationSpeed = 5f;

    [Tooltip("How much Speed Paint effects Movement Speed: This is a multiplier")]
    public float movementColorMult = 2f;

    public Transform cam;
    public bool lockCursor = true;

    private float currSpeed;
    private Transform m_orientation;
    private float m_currColorMult = 1f;
    private float m_shopMoveMult => PlayerManager.instance.stats.skills[3].currentMult;
    private float m_maxSpeed;
    private Vector3 m_lasPos;

    // Momentum
    [Header("Momentum")]
    [Tooltip("How fast you can Accelerate towards a direction on the ground")]
    public float groundAccelMult = 1f;

    [Tooltip("How fast you can Accelerate towards a direction in The Air")]
    public float airAccelMult = 0.8f;

    [Tooltip("Friction while in the air")]
    public float airDrag = 0.2f;

    [Tooltip("Friction while on the Ground")]
    public float groundDrag = 0.4f;

    // Runtime Bonuses
    [Header("Runtime Bonuses")]
    public float bonusMoveSpeed = 0f;
    public float bonusMaxSpeed = 0f;

    // Dashing
    [Header("Dashing")]
    [Tooltip("Shows the rate of acceleration over the time of the dash")]
    public AnimationCurve dashSpeedCurve;
    public float dashSpeed = 20f;
    public float dashDuration = 0.5f;
    public float dashCooldown = 1f;

    public bool isDashing = false;
    public bool m_dashInputPressed_field = false;

    private float m_dashTime = 0f;
    private float m_timeSinceLastDash = 0f;
    private bool m_dashBack = false;
    private float m_dashStartSpeed;
    private Vector3 dashDir;

    // Jump
    [Header("Jump Settings")]
    [Tooltip("Default Upward force applied when jumping")]
    public float jumpForce = 5.0f;
    public float maxJumpAngle = 80f;
    public float jumpCooldown = 0.25f;
    public float jumpHoldMult = 1.5f;
    public float maxJumpHoldTime = 2f;

    public int currJumpCount = 1;
    public int maxJumpCount => PlayerManager.instance.maxJumpCount;
    public float jumpInputElapsed = Mathf.Infinity;

    private float m_timeSinceLastJump = 0.0f;
    private float m_jumpBufferTime = 0.25f;
    private bool m_jumpHoldActive = false;
    private float m_jumpHoldTimer = 0f;
    private bool wasGrounded = false;
    private bool _wasGroundedLastFrame = true;

    // Wall Riding
    [Header("Wall Riding")]
    [ReadOnly] public bool m_isWallRiding = false;
    [ReadOnly] public bool leftWall;
    [ReadOnly] public bool rightWall;

    public float wallCheckDist = 1f;
    public float wallCheckDistance = 1f;
    public float climbMult = 0.5f;
    public float wallGravity = 1f;
    public LayerMask wallLayers;

    private bool m_wallPaint = false;
    private Vector3 m_wallNormal;
    private Vector3 m_wallRunDir;

    // Rail Grinding
    [Header("Rail Grinding")]
    public LayerMask railLayer;
    public SplineContainer splineContainer;
    public float railDetectionRadius = 1.5f;
    public float railSnapDistance = 2f;
    public float minGrindSpeed = 3f;
    public float grindExitForce = 8f;
    public float railProgress;
    public float grindSpeed;

    [ReadOnly] public bool isGrinding;
    public Rail currentRail;

    public float m_railDir = 1f;
    [SerializeField] private Transform m_railDetectionPoint;

    private bool wasGrinding = false;
    private float m_timer;
    private float betweenRailTime = 0.5f;
    private float m_grindExitCooldown = 0f;
    private const float k_grindExitCooldownDuration = 0.4f;

    // Stun
    [Header("Stun State")]
    [ReadOnly] public bool isStunned = false;
    public float stunDuration = 1f;
    private float m_stunTimer = 0f;

    // Paint
    [Header("Paint Things")]
    public PlayerPaint standPaintColor;
    [SerializeField] private Transform paintPoint;

    private PaintColors colors;
    private float paintRotation;

    // Model / Visuals
    public GameObject playerModel;
    public float modelRotationSpeed = 5f;
    private Vector3 smoothedNormal;
    private Vector3 m_targetUp;

    // State Machine
    private enum MoveState
    {
        Grounded,
        Air,
        Dash,
        WallRide,
        Grind,
        Stun,
    };

    [SerializeField] private MoveState state;
    private MoveState prevState;

    #endregion



    #region Unity Lifecycle (Start / Update / FixedUpdate)

    void Start()
    {
        m_orientation = cam;
        colors = PaintManager.instance.GetComponent<PaintColors>();
        m_inputActions = new PlayerInputActions().Player;
        m_inputActions.Enable();
        m_lasPos = transform.position;
    }

    void Update()
    {
        HandleInput();
        HandlePaintColor();
        HandleFOV();
        HandleAnimations();
        if (PlayerInputLock.Locked) return;
        RotatePlayerModel();
    }

    void FixedUpdate()
    {
        if (PlayerInputLock.Locked) return;

        bool onGround = CheckIfGrounded(out RaycastHit _);
        HandleRotation();
        EvaluateTransitions(onGround);

        if (state == MoveState.Stun)
        {
            m_stunTimer += Time.deltaTime;
            HandleRegularMovement();
            if (m_stunTimer >= stunDuration)
            {
                m_stunTimer = 0f;
                isStunned = false;
                SetState(MoveState.Grounded);
            }
            return;
        }

        switch (state)
        {
            case MoveState.Grind:
                ContinueGrinding();
                break;

            case MoveState.WallRide:
                splineContainer = null;
                transform.position = MovePlayer(m_velocity * Time.deltaTime);
                break;

            case MoveState.Dash:
                splineContainer = null;
                transform.position = MovePlayer(m_velocity * Time.deltaTime);
                break;

            case MoveState.Grounded:
            case MoveState.Air:
            case MoveState.Stun:
                HandleRegularMovement();
                break;

            default:
                splineContainer = null;
                HandleRegularMovement();
                break;
        }

        m_timer += Time.deltaTime;
        CheckLandingEvent();
    }

    void OnDestroy()
    {
        m_inputActions.Disable();
    }

    #endregion

    #region State Machine


    private void EvaluateTransitions(bool onGround)
    {
        if (isStunned)
        {
            SetState(MoveState.Stun);
            return;
        }

        if ((TryStartGrinding() || isGrinding) && m_timer > betweenRailTime)
        {
            SetState(MoveState.Grind);
            return;
        }

        if (WallRun())
        {
            SetState(MoveState.WallRide);
            return;
        }

        if (HandleDashing())
        {
        }

        SetState(onGround ? MoveState.Grounded : MoveState.Air);
    }

    private void SetState(MoveState next)
    {
        if (state == next) return;
        prevState = state;
        state = next;
    }

    #endregion


    #region Input

    void HandleInput()
    {
        if (isStunned)
        {
            moveInput = Vector2.zero;
            m_jumpInputPressed = false;
            m_dashInputPressed = false;
            return;
        }

        moveInput = m_inputActions.Move.ReadValue<Vector2>();

        m_jumpInputPressed = m_inputActions.Jump.IsPressed();

        if (m_inputActions.Jump.WasPressedThisFrame())
            jumpInputElapsed = 0.0f;
        else
            jumpInputElapsed += Time.deltaTime;

        m_dashInputPressed = m_inputActions.Dash.WasPressedThisFrame();
    }

    #endregion

    #region Regular Movement & Jump

    void HandleRegularMovement()
    {
        currSpeed = (speed + bonusMoveSpeed) * m_currColorMult;

        Vector3 inputDir = (m_orientation.forward * moveInput.y + m_orientation.right * moveInput.x).normalized;

        bool onGround = CheckIfGrounded(out RaycastHit groundHit);
        bool canWalk = onGround && maxSlopeAngle >= Vector3.Angle(Vector3.up, groundHit.normal);

        currJumpCount = onGround ? maxJumpCount : currJumpCount;

        // Slope math
        Vector3 groundNormal = canWalk ? groundHit.normal : Vector3.up;

        if (canWalk)
        {
            Vector3 slopeForward = Vector3.ProjectOnPlane(m_orientation.forward, groundNormal);
            Vector3 slopeRight   = Vector3.ProjectOnPlane(m_orientation.right, groundNormal);

            if (slopeForward.sqrMagnitude > 0.0001f) slopeForward.Normalize();
            if (slopeRight.sqrMagnitude   > 0.0001f) slopeRight.Normalize();

            Vector3 slopeInputDir = slopeForward * moveInput.y + slopeRight * moveInput.x;
            if (slopeInputDir.sqrMagnitude > 0.0001f) slopeInputDir.Normalize();

            inputDir = slopeInputDir;
        }

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

        if (inputDir.sqrMagnitude > 0.01f && !isDashing && moveInput.sqrMagnitude > 0.01f)
        {
            float accelMult = canWalk ? groundAccelMult : airAccelMult;
            Vector3 targetVel = inputDir * currSpeed;
            horizontalVel = Vector3.MoveTowards(horizontalVel, targetVel, currSpeed * accelMult * Time.deltaTime);
        }
        else
        {
            float baseDrag = canWalk ? groundDrag : airDrag;

            if (horizontalVel.magnitude > currSpeed)
                baseDrag *= 0.3f;

            float dragFactor = Mathf.Exp(-baseDrag * Time.deltaTime);
            horizontalVel *= dragFactor;

            if (canWalk)
            {
                float frictionBoost = Mathf.InverseLerp(0, currSpeed, horizontalVel.magnitude);
                horizontalVel = Vector3.Lerp(horizontalVel, Vector3.zero, (1f - frictionBoost) * baseDrag * Time.deltaTime);
            }
        }

        if (horizontalVel.magnitude > m_maxSpeed)
            horizontalVel = horizontalVel.normalized * m_maxSpeed;

        if (canWalk)
        {
            if (m_velocity.y > 8f)
                m_velocity = horizontalVel + new Vector3(0, m_velocity.y, 0);
            else
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
            if (!wasGrounded && m_velocity.y <= 0f)
            {
                m_velocity.y = 0f;
                wasGrounded = true;
            }
        }

        // Jump
        bool isWallNearby = !groundedState.isGrounded && WallCheck(out _);
        bool canJump = ((onGround && groundedState.angle <= maxJumpAngle) || currJumpCount > 0) && m_timeSinceLastJump >= jumpCooldown && !isWallNearby;
        bool attemptingJump = jumpInputElapsed <= m_jumpBufferTime;

        if (canJump && attemptingJump)
        {
            if (m_jumpHoldActive == true)
                return;

            m_timeSinceLastJump = 0.0f;
            jumpInputElapsed = Mathf.Infinity;
            currJumpCount--;

            m_velocity.y = jumpForce;

            m_jumpHoldActive = true;
            m_jumpHoldTimer = 0f;
        }
        else
        {
            m_timeSinceLastJump += Time.deltaTime;
        }

        // Variable jump hold
        if (m_jumpHoldActive)
        {
            if (!m_jumpInputPressed || m_jumpHoldTimer >= maxJumpHoldTime)
            {
                m_jumpHoldActive = false;
            }
            else
            {
                float holdBoost = Mathf.Lerp(0, jumpForce * jumpHoldMult, m_jumpHoldTimer / maxJumpHoldTime);
                m_velocity.y += holdBoost * Time.deltaTime;
                m_jumpHoldTimer += Time.deltaTime;
            }
        }

        transform.position = MovePlayer(m_velocity * Time.deltaTime);

        if (onGround && !attemptingJump && groundedState.angle > 7 && transform.position.y - m_lasPos.y < 0.001f)
            SnapPlayerDown();

        m_lasPos = transform.position;
    }

    public void AddForce(Vector3 force)
    {
        m_velocity += force;
        if (force.y > 0 && groundedState.isGrounded)
            wasGrounded = false;
    }

    public Vector3 Velocity => m_velocity;

    public bool GetGrounded() => groundedState.isGrounded;

    #endregion


    #region Dashing

    bool HandleDashing()
    {
        if (isDashing)
        {
            m_dashTime += Time.deltaTime;
            if (m_dashTime >= dashDuration)
                isDashing = false;

            m_timeSinceLastDash += Time.deltaTime;
            return isDashing;
        }

        bool canDash = m_timeSinceLastDash >= dashCooldown;

        if (canDash && m_dashInputPressed && moveInput.sqrMagnitude > 0.01f)
        {
            dashDir = (m_orientation.forward * moveInput.y + m_orientation.right * moveInput.x).normalized;

            isDashing = true;
            m_dashTime = 0f;
            m_timeSinceLastDash = 0f;

            float startSpeed = m_velocity.magnitude;
            m_velocity = dashDir * (dashSpeed + startSpeed);

            GameEvents.PlayerDodged?.Invoke();

            return true;
        }

        m_timeSinceLastDash += Time.deltaTime;
        return false;
    }

    #endregion


    #region Rotation

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

    void RotatePlayerModel()
    {
        if (isGrinding) return;

        Vector3 targetUp = groundedState.isGrounded ? groundedState.groundNormal : Vector3.up;
        smoothedNormal = Vector3.Slerp(smoothedNormal, targetUp, Time.deltaTime * 12f);

        float maxTilt = 45f;
        float angle = Vector3.Angle(Vector3.up, smoothedNormal);
        if (angle > maxTilt)
            smoothedNormal = Vector3.Slerp(Vector3.up, smoothedNormal, maxTilt / angle);

        Vector3 modelUp = transform.forward;
        Vector3 slopeForward = Vector3.ProjectOnPlane(modelUp, smoothedNormal).normalized;

        if (slopeForward.sqrMagnitude < 0.001f)
            slopeForward = Vector3.ProjectOnPlane(transform.right, smoothedNormal).normalized;

        Quaternion worldTargetRot = Quaternion.LookRotation(slopeForward, smoothedNormal);
        Quaternion localTargetRot = Quaternion.Inverse(transform.rotation) * worldTargetRot;

        playerModel.transform.localRotation = Quaternion.Slerp(
            playerModel.transform.localRotation,
            localTargetRot,
            Time.deltaTime * 10f
        );
    }

    #endregion

    #region Wall Riding

    bool WallRun()
    {
        if (m_isWallRiding && (!m_jumpInputPressed || Physics.Raycast(transform.position, m_velocity.normalized, wallCheckDist, collisionLayers)))
        {
            m_velocity = m_wallNormal * jumpForce + Vector3.up * jumpForce;
            m_isWallRiding = false;
            paintPoint.Rotate(0, 0, -paintRotation);
            paintRotation = 0f;
            return false;
        }

        if (groundedState.isGrounded && m_isWallRiding)
        {
            m_velocity += m_wallNormal * 2;
            m_isWallRiding = false;
            paintPoint.Rotate(0, 0, -paintRotation);
            paintRotation = 0f;
            return false;
        }

        if (!m_isWallRiding && !groundedState.isGrounded && m_jumpInputPressed)
        {
            if (WallCheck(out RaycastHit hit))
            {
                m_isWallRiding = true;
                m_wallNormal = hit.normal;
                if (leftWall)
                    paintRotation -= 90;
                else if (rightWall)
                    paintRotation += 90;
            }
            paintPoint.Rotate(0, 0f, paintRotation);
        }

        if (m_isWallRiding)
        {
            if (Physics.Raycast(transform.position, -m_wallNormal, out RaycastHit hit, wallCheckDistance, wallLayers))
            {
                m_wallNormal = hit.normal;
                Vector3 wallNormalNoY = new Vector3(m_wallNormal.x, 0, m_wallNormal.z);
                m_wallRunDir = Vector3.Cross(wallNormalNoY, Vector3.up).normalized;

                if (Vector3.Dot(m_wallRunDir, transform.forward) < 0)
                    m_wallRunDir *= -1;

                if (m_wallPaint)
                    m_velocity = m_wallRunDir * currSpeed + Vector3.up * climbMult;
                else
                    m_velocity = m_wallRunDir * currSpeed + -Vector3.up * wallGravity;

                return true;
            }
            else
            {
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

        for (int i = -20; i <= 20; i += 5)
        {
            if (Physics.Raycast(transform.position, -transform.right, out hit, wallCheckDistance, wallLayers))
            {
                leftWall = true;
                return true;
            }
        }
        for (int i = -20; i <= 20; i += 5)
        {
            if (Physics.Raycast(transform.position, transform.right, out hit, wallCheckDistance, wallLayers))
            {
                rightWall = true;
                return true;
            }
        }

        return false;
    }

    public Vector3 GetWallNormal() => m_wallNormal;

    #endregion


    #region Rail Grinding

    bool TryStartGrinding()
    {
        if (isGrinding) return true;

        if (m_grindExitCooldown > 0f)
        {
            m_grindExitCooldown -= Time.deltaTime;
            return false;
        }

        splineContainer = null;

        Collider[] railColliders = Physics.OverlapSphere(m_railDetectionPoint.position, railDetectionRadius, railLayer);
        if (railColliders.Length == 0) return false;

        foreach (var collider in railColliders)
        {
            splineContainer = collider.GetComponent<SplineContainer>();
            if (splineContainer == null || splineContainer.Splines.Count == 0) continue;
        }

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

        grindSpeed = Mathf.Max(m_velocity.magnitude + grindExitForce, minGrindSpeed + grindExitForce);

        Vector3 tangent = GetSplineTangentAt(splineRef, railProgress);
        if (tangent.sqrMagnitude < 0.0001f) tangent = Vector3.forward;

        float dot = Vector3.Dot(m_velocity.normalized, tangent.normalized);
        m_railDir = dot >= 0f ? 1f : -1f;

        Vector3 splinePos = (Vector3)splineRef.EvaluatePosition(railProgress);
        Vector3 worldSplinePos = splineContainer.transform.position + splinePos + Vector3.up;

        Vector3 snapDelta = worldSplinePos - transform.position;
        transform.position = MovePlayer(snapDelta);

        m_velocity = tangent.normalized * grindSpeed * m_railDir;

        GameEvents.PlayerStartedGrinding?.Invoke();
    }

    void ContinueGrinding()
    {
        if (splineContainer == null || splineContainer.Splines.Count == 0)
        {
            ExitGrinding();
            return;
        }

        bool jumpBuffered = jumpInputElapsed <= m_jumpBufferTime;
        if (jumpBuffered)
        {
            jumpInputElapsed = Mathf.Infinity;
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

        m_velocity = tangentHere * speedAlongTangent;
        Vector3 move = m_velocity * Time.deltaTime;
        transform.position = MovePlayer(move);

        if (railProgress >= 0.999f || railProgress <= 0.001f)
        {
            ExitGrinding();
            return;
        }

        Vector3 splinePos = (Vector3)splineRef2.EvaluatePosition(railProgress);
        Vector3 worldSplinePos = splineContainer.transform.position + splinePos + Vector3.up * 1.4f;
        Vector3 delta = worldSplinePos - transform.position;
        Vector3 lateral = delta - Vector3.Project(delta, tangentHere);
        float correctionStrength = Mathf.Clamp01(5f * Time.deltaTime);
        transform.position = MovePlayer(lateral * correctionStrength);

        SnapPlayerDown();
    }

    void ExitGrinding()
    {
        if (!isGrinding) return;

        isGrinding = false;
        m_grindExitCooldown = k_grindExitCooldownDuration;

        if (splineContainer != null)
            m_velocity += Vector3.up * grindExitForce * 0.5f;

        transform.position = MovePlayer(m_velocity * Time.deltaTime);

        GameEvents.PlayerEndedGrinding?.Invoke();

        m_timer = 0f;
        splineContainer = null;
        railProgress = 0f;
        grindSpeed = 0f;
        wasGrinding = false;
    }

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

    Vector3 FindClosestPointOnSpline(out float bestT)
    {
        FindClosestPointOnSpline(splineContainer, transform.position, out bestT, out _);
        var spline = splineContainer.Splines[0];
        Vector3 localP = (Vector3)spline.EvaluatePosition(bestT);
        return splineContainer.transform.TransformPoint(localP);
    }

    Vector3 GetSplineTangentAt(Spline spline, float t, float deltaT = 0.001f)
    {
        float t2 = Mathf.Clamp01(t + deltaT);
        Vector3 p1 = (Vector3)spline.EvaluatePosition(t);
        Vector3 p2 = (Vector3)spline.EvaluatePosition(t2);

        Vector3 tangent = (p2 - p1).normalized;
        if (tangent.sqrMagnitude < 0.0001f)
        {
            float t0 = Mathf.Clamp01(t - deltaT);
            p2 = (Vector3)spline.EvaluatePosition(t0);
            tangent = (p1 - p2).normalized;
        }
        return tangent;
    }

    #endregion


    #region Stun

    public void Stunned()
    {
        isStunned = true;
        m_stunTimer = 0f;
    }

    #endregion


    #region Paint & Color

    public void HandlePaintColor()
    {
        m_currColorMult = standPaintColor.standingColor == colors.movementPaint ? movementColorMult : 1f;
        if (groundedState.isGrounded)
        {
            float effectiveMaxSpeed = maxSpeed + bonusMaxSpeed;
            m_maxSpeed = standPaintColor.standingColor == colors.movementPaint ? effectiveMaxSpeed : effectiveMaxSpeed * 4;
        }
        m_wallPaint = standPaintColor.standingColor == colors.jumpPaint;
    }

    #endregion


    #region Speed Bonuses

    public void SetMoveSpeedBonus(float moveBonus, float maxBonus)
    {
        bonusMoveSpeed = Mathf.Max(0f, moveBonus);
        bonusMaxSpeed  = Mathf.Max(0f, maxBonus);
    }

    public void ClearMoveSpeedBonus()
    {
        bonusMoveSpeed = 0f;
        bonusMaxSpeed  = 0f;
    }

    #endregion


    #region Animations & FOV

    public void HandleAnimations()
    {
        animator.SetBool("Grinding", isGrinding);
        animator.SetBool("Dashing", isDashing);

        Vector2 horzVelocity = new Vector2(m_velocity.x, m_velocity.z);
        animator.SetBool("Moving", horzVelocity.magnitude > 2f);
        animator.SetBool("Grounded", groundedState.isGrounded);
    }

    public void HandleFOV()
    {
        Camera cam = Camera.main;
        if (cam == null) return;
        float targetFOV = isDashing ? 80f : 60f;
        cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, targetFOV, Time.deltaTime * 8f);
    }

    #endregion


    #region Landing Event

    void CheckLandingEvent()
    {
        bool groundedNow = groundedState.isGrounded;

        if (!_wasGroundedLastFrame && groundedNow)
        {
            GameEvents.PlayerLanded?.Invoke();
            Debug.Log("[Landing] PlayerLanded invoked");
        }

        _wasGroundedLastFrame = groundedNow;
    }

    #endregion


    #region Gizmos

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

    #endregion
}
using System.ComponentModel.Design.Serialization;
using JetBrains.Annotations;
using UnityEngine;

namespace KinematicCharacterControler
{
    public class PlayerMovement : MonoBehaviour
    {
        public float speed = 5f;
        public float rotationSpeed = 5f;
        public float maxWalkAngle = 60f;
        public GameObject player;


        private Transform m_orientation;
        public Transform cam;

        public  PlayerMovmentEngine engine;

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

        

        

        void Start()
        {
            player = GameObject.Find("Player");
            m_orientation = cam;
        }

        // Update is called once per frame
        void Update()
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

            if (Input.GetKey(KeyCode.Space))
                m_jumpInputPressed = true;
            else
                m_jumpInputPressed = false;
            float horizontal = Input.GetAxisRaw("Horizontal");
            float vertical = Input.GetAxisRaw("Vertical");

            Vector3 viewDir = transform.position - new Vector3(cam.position.x, transform.position.y, cam.position.z);
            m_orientation.forward = viewDir.normalized;

            Vector3 inputDir = m_orientation.forward * vertical + m_orientation.right * horizontal;

            // rotate player
            if (inputDir != Vector3.zero)
            {
                player.transform.forward = Vector3.Slerp(player.transform.forward, inputDir.normalized, Time.deltaTime * rotationSpeed);
            }

            bool onGround = engine.CheckIfGrounded(out RaycastHit groundHit);


            bool falling = !(onGround && maxWalkAngle >= Vector3.Angle(Vector3.up, groundHit.normal));
            print(falling);

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


            if (m_jumpInputPressed)
            {
                jumpInputElapsed = 0.0f;
            }

            // Player can jump if they are (1) on the ground, (2) within the ground jump angle, (3), after the jump cooldown
            bool canJump = onGround && engine.groundedState.angle <= maxJumpAngle && m_timeSinceLastJump >= jumpCooldown;
            bool attemptingJump = jumpInputElapsed <= m_jumpBufferTime;

            // Have player jump if they can jump and are attempting to jump
            if (canJump && attemptingJump)
            {
                m_velocity = Vector3.up * jumpVelocity;
                m_timeSinceLastJump = 0.0f;
                jumpInputElapsed = Mathf.Infinity;

            }
            else
            {
                m_timeSinceLastJump += Time.deltaTime;
                jumpInputElapsed += Time.deltaTime;
            }

            transform.position = engine.MovePlayer(inputDir * speed * Time.deltaTime);
            transform.position = engine.MovePlayer(m_velocity * Time.deltaTime);

            if(onGround && !attemptingJump)
                engine.SnapPlayerDown();

        }
    }
}

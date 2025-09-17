using UnityEditor.Experimental.GraphView; // you actually don’t need this for movement
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class ThirdPersonMovment : MonoBehaviour
{
    public CharacterController controller;
    public GameObject player;
    public Transform cam;


    [Header("Movement Settings")]
    public float speed = 6f;
    public float rotationSpeed = 5f;

    [Header("Jump / Gravity")]
    public float gravity = -9.81f;   
    public float jumpHeight = 2f;    // how high you want to jump

    private Vector3 velocity;        // holds vertical velocity

    private Transform m_orientation = null;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        player = this.gameObject;
        m_orientation = cam.transform;


        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        // check if grounded
        bool isGrounded = controller.isGrounded;

        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");

        // camera-relative movement
        Vector3 viewDir = transform.position - new Vector3(cam.position.x, transform.position.y, cam.position.z);
        m_orientation.forward = viewDir.normalized;

        Vector3 inputDir = m_orientation.forward * vertical + m_orientation.right * horizontal;

        // rotate player
        if (inputDir != Vector3.zero)
        {
            player.transform.forward = Vector3.Slerp(player.transform.forward, inputDir.normalized, Time.deltaTime * rotationSpeed);
        }

        // horizontal movement
        controller.Move(inputDir.normalized * speed * Time.deltaTime);

        // jump input
        if (isGrounded && Input.GetKeyDown(KeyCode.Space)) 
        {
            velocity.y = 0f;
            velocity.y = jumpHeight;
        }

        // apply gravity
        if(!isGrounded)
        {
            velocity.y += -gravity * Time.deltaTime;
        }

        

        // move vertically
        controller.Move(velocity * Time.deltaTime);
    }
}

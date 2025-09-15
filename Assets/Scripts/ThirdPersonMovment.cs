using UnityEngine;

public class ThirdPersonMovment : MonoBehaviour
{
    public CharacterController controller;
    public GameObject player;
    public Transform cam;
    public float speed = 6f;
    private bool isGrounded;
    public float gravity = 5.81f;

    public float mouseSensitivity = 100f;
    private Vector3 moveDir;
    float turnSmoothVelocity;
    void Start()
    {
        controller = GetComponent<CharacterController>();
        player = this.gameObject;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }



    // Update is called once per frame
    void Update()
    {

        if(!controller.isGrounded)
        {
            controller.Move(Vector3.down * gravity * Time.deltaTime);
        }

        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");
        
        Vector3 direction = new Vector3(horizontal, 0f, vertical);

        if (direction.magnitude >= 0.1f)
        {
            float targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg + cam.eulerAngles.y;
            //float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnSmoothVelocity, 0.1f);
            //transform.rotation = Quaternion.Euler(0f, angle, 0f);

            moveDir = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;
            controller.Move(speed * Time.deltaTime * moveDir.normalized);
        }

    


    



        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
            transform.Rotate(Vector3.up * mouseX);

     
    
    }
}

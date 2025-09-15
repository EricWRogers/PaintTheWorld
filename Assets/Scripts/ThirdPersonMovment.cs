using UnityEngine;

public class ThirdPersonMovment : MonoBehaviour
{
    public CharacterController controller;
    public Transform cam;
    public float speed = 6f;
    private bool isGrounded;
    public float gravity = 5.81f;



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
        

     
            controller.Move(direction * speed * Time.deltaTime);
    
    }
}

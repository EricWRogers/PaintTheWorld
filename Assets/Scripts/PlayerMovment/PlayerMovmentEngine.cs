using UnityEngine;

public class PlayerMovmentEngine : MonoBehaviour
{

    public float defaultGroundCheck = 0.25f;
    public float defaultGroundedDistance = 0.05f;
    public Vector3 previousPosition = Vector3.zero;

    public GroundedState groundedState { get;  protected set; }


    void Awake()
    {
        previousPosition = this.transform.position;
    }


    public void Move()
    {

    }


    public void CheckCollisions()
    {
        

    }

    public bool CheckIfGrounded()
    {
        RaycastHit hit;
        if (Physics.Raycast(transform.position, Vector3.down, out hit, defaultGroundCheck))
        {
            float angle = Vector3.Angle(hit.normal, Vector3.up);
            bool isGrounded = hit.distance <= defaultGroundedDistance;
            groundedState = new GroundedState(hit.distance, isGrounded, angle, hit.normal, hit.point);
            return isGrounded;
        }
        else
        {
            groundedState = new GroundedState(defaultGroundCheck, false, 0f, Vector3.up, Vector3.zero);
            return false;
        }
    }


    public struct GroundedState
    { 
        public float distToGround;
        public bool isGrounded;
        public float angle;
        public Vector3 groundNormal;
        public Vector3 groundHitPosition;
   
 
     
        public GroundedState(float distToGround, bool isGrounded, float angle, Vector3 groundNormal, Vector3 groundHitPosition)
        {
            this.distToGround = distToGround;
            this.isGrounded = isGrounded;
            this.angle = angle;
            this.groundNormal = groundNormal;
            this.groundHitPosition = groundHitPosition;
        }

    }
}

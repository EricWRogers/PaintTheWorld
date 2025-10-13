using UnityEngine;

[RequireComponent(typeof(PlayerMovement))]
public class ExtraJumpController : MonoBehaviour
{
    public int extraJumpsGranted = 0; // modified by items
    private PlayerMovement pm;
    private int jumpsUsed;

    void Awake(){ pm = GetComponent<PlayerMovement>(); }
    void OnEnable(){ jumpsUsed = 0; }

    void Update()
    {
        // Reset when grounded
        if (pm.groundedState.isGrounded) jumpsUsed = 0;

        // Detect a jump press that occurs while airborne and allow if we have extra jumps
        if (!pm.groundedState.isGrounded && pm.m_jumpInputPressed && jumpsUsed < extraJumpsGranted)
        {
            // Consume one extra jump
            jumpsUsed++;
            // apply an immediate vertical like first jump
            pm.GetType().GetMethod("HandleRegularMovement", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            pm.GetComponent<Rigidbody>()?.AddForce(Vector3.up * pm.jumpForce, ForceMode.VelocityChange);
        }
    }
}

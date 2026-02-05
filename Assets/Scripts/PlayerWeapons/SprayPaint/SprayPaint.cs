using UnityEngine;
using System.Collections;

public class SprayPaint : MonoBehaviour
{
    [Header("Ammo Settings")]
    public int currentAmmo = 100;
    public int maxAmmo = 200;
    public int ammoPerAttack = 5; 

    [Header("Combo Settings")]
    public Animator playerAnimator;
    public float comboResetTime = 3.0f;
    private int comboStep = 0; // 0 = Ready, 1 = Right Done, 2 = Left Done
    private float lastClickTime;

    [Header("Effects")]
    public ParticleSystem rightParticles;
    public ParticleSystem leftParticles;

    private void Update()
    {
        // Reset combo if the player waits too long
        if (Time.time - lastClickTime > comboResetTime)
        {
            comboStep = 0;
        }

        if (Input.GetMouseButtonDown(0))
        {
            TryAttack();
        }
    }

    private void TryAttack()
    {
        if (currentAmmo <= 0)
        {
            Debug.Log("Out of ammo!");
            return;
        }

        lastClickTime = Time.time;

        if (comboStep == 0)
        {
            // Attack 1: Right Hand
            PerformAttack("RightAttack", rightParticles);
            comboStep = 1;
        }
        else if (comboStep == 1)
        {
            // Attack 2: Left Hand
            PerformAttack("LeftAttack", leftParticles);
            comboStep = 0; // Reset to start of combo
        }
    }

    private void PerformAttack(string triggerName, ParticleSystem particles)
    {
        // 1. Play Animation
        if (playerAnimator != null)
        {
            playerAnimator.SetTrigger(triggerName);
            
            // 2. Determine spray duration based on animation length
            float animLength = GetAnimationLength(triggerName);
            
            // 3. Fire particles and consume ammo
            StartCoroutine(SprayRoutine(particles, animLength));
        }
    }

    private System.Collections.IEnumerator SprayRoutine(ParticleSystem particles, float duration)
    {
        if (particles != null)
        {
            particles.Play();
            
            // Calculate ammo: If current ammo is less than cost, we only spray for a fraction of the time
            float actualDuration = (currentAmmo < ammoPerAttack) ? 
                                   (duration * ((float)currentAmmo / ammoPerAttack)) : duration;

            ConsumeAmmo(ammoPerAttack);

            yield return new WaitForSeconds(actualDuration);
            particles.Stop();
        }
    }

    private float GetAnimationLength(string triggerName)
    {
        // This looks at the animator's current state to find how long the clip is
        // Ensure your Animation Clip names in the Animator match your triggers
        if (playerAnimator != null)
        {
            return playerAnimator.GetCurrentAnimatorStateInfo(0).length;
        }
        return 0.5f; // Default fallback
    }

    public void AddAmmo(int amount)
    {
        currentAmmo = Mathf.Min(currentAmmo + amount, maxAmmo);
        Debug.Log($"Refilled! Ammo: {currentAmmo}");
    }

    private void ConsumeAmmo(int amount)
    {
        currentAmmo = Mathf.Max(0, currentAmmo - amount);
    }
}

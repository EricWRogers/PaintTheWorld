using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using SuperPupSystems.Helper;
using UnityEngine.Events;

public class ParticlePainter2 : MonoBehaviour
{
    public Color selectedPaint;  
    public int colorKey = 0;
    public float minRadius = 0.05f;
    public float maxRadius = 0.2f;
    public float strength = 1;
    public float hardness = 1;
    public int particleDamage;
    public UnityEvent hitEvent;
    public string tagToHit;
    [Space]
    ParticleSystem part;
    List<ParticleCollisionEvent> collisionEvents;

    void Start()
    {
        part = GetComponent<ParticleSystem>() ?? GetComponentInChildren<ParticleSystem>();
        collisionEvents = new List<ParticleCollisionEvent>();
    }

    void OnParticleCollision(GameObject other)
    {
        int numCollisionEvents = part.GetCollisionEvents(other, collisionEvents);

        Paintable p = other.GetComponent<Paintable>();
        for (int i = 0; i < numCollisionEvents; i++)
        {
            if(p != null)
            {
                Vector3 pos = collisionEvents[i].intersection;
                float radius = Random.Range(minRadius, maxRadius);
                PaintManager.instance.paint(p, pos, radius, hardness, strength, selectedPaint);
            }
            
        }
    }
}

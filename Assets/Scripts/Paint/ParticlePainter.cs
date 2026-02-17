using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using SuperPupSystems.Helper;
using UnityEngine.Events;

public class ParticlePainter : MonoBehaviour
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
        part = GetComponent<ParticleSystem>();
        collisionEvents = new List<ParticleCollisionEvent>();
        selectedPaint = PaintManager.instance.GetComponent<PaintColors>().colorDict[colorKey];
    }

    void OnParticleCollision(GameObject other)
    {
        int numCollisionEvents = part.GetCollisionEvents(other, collisionEvents);

        Paintable p = other.GetComponent<Paintable>();
        for (int i = 0; i < numCollisionEvents; i++)
        {
            Debug.Log("particle hit: " + other.name + " " + other.tag);
            if (other.CompareTag("Enemy"))
            {
                Debug.Log("damaged enemy");
                other.GetComponent<Health>().Damage(particleDamage);
            }
            if(p != null)
            {
                Vector3 pos = collisionEvents[i].intersection;
                float radius = Random.Range(minRadius, maxRadius);
                PaintManager.instance.paint(p, pos, radius, hardness, strength, selectedPaint);
            }
            
        }
    }
    
       void Update()
    {
        
        if (Input.GetAxis("Mouse ScrollWheel") > 0f)
        {
            colorKey++;
            if (colorKey > 2)
            {
                colorKey = 0;
            }
            selectedPaint = PaintManager.instance.GetComponent<PaintColors>().colorDict[colorKey];
        }

        if (Input.GetAxis("Mouse ScrollWheel") < 0f)
        {
            colorKey--;
            if (colorKey < 0)
            {
                colorKey = 2;
            }
            selectedPaint = PaintManager.instance.GetComponent<PaintColors>().colorDict[colorKey];
        }

        
    }
}

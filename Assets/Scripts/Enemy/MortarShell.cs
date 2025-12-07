using SuperPupSystems.Helper;
using UnityEngine;

public class MortarShell : MonoBehaviour
{
    public Vector3 startPos;
    public Vector3 endPos;
    public float flightTime = 2f;
    public float arcHeight = 3f;
    public int damage;
    public float radius;
    public LayerMask layerMask;
    [HideInInspector] public Mortar mortar;
    private float timer = 0f;
    private bool isFlying = false;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip fireSound;
    [Range(0f, 1f)]
    public float fireSoundVolume = 0.9f;
    [Range(0.95f, 1.05f)]
    public float randomPitchRange = 1.02f;

    public void Launch(Vector3 _start, Vector3 _end, float _time, float _height, float _radius)
    {
        startPos = _start;
        endPos = _end;
        flightTime = _time;
        arcHeight = _height;
        timer = 0f;
        isFlying = true;
        transform.position = startPos;
        radius = _radius;
    }


    void Update()
    {
        if (!isFlying) return;

        timer += Time.deltaTime;
        float t = Mathf.Clamp01(timer / flightTime);

        Vector3 horizontalPos = Vector3.Lerp(startPos, endPos, t);
        float heightOffset = arcHeight * 4f * t * (1 - t);
        transform.position = horizontalPos + Vector3.up * heightOffset;

        if (t >= 1f)
        {
            isFlying = false;
            OnImpact();
        }
    }

    private void OnImpact()
    {
        
        if (fireSound != null)
        {
            float delta = randomPitchRange - 1f;
            float pitch = Random.Range(1f - delta, 1f + delta);

            // create temporary audio source so sound persists after shell is destroyed
            GameObject sgo = new GameObject("MortarImpactSound");
            sgo.transform.position = transform.position;
            var src = sgo.AddComponent<AudioSource>();
            src.spatialBlend = 1f;
            src.playOnAwake = false;
            src.clip = fireSound;
            src.volume = fireSoundVolume;
            src.pitch = pitch;
            src.Play();
            Destroy(sgo, fireSound.length / Mathf.Max(0.01f, Mathf.Abs(src.pitch)));
        }
        
        var hits = Physics.OverlapSphere(transform.position, radius, layerMask);
        if (hits.Length > 0)
        {
            foreach (Collider hit in hits)
            {
                Paintable tempPaintable = hit.transform.gameObject.GetComponent<Paintable>();
                if (tempPaintable != null)
                {
                    PaintManager.instance.paint(tempPaintable, hit.ClosestPoint(transform.position), radius, 1f, 1f, Color.clear);
                }
                if (hit.transform.gameObject.tag == "Player")
                {
                    Health health = PlayerManager.instance.health;
                    if (health != null)
                    {
                        health.Damage(damage);
                        mortar.hasTarget = false;
                        Destroy(gameObject);
                    }
                }
            }
        }
        mortar.hasTarget = false;
        Destroy(gameObject);
    }
    
    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, radius);
    }
}
// using System;
// using SuperPupSystems.Helper;
// using UnityEngine;

// public class PowerPainter : RayCastPainter
// {
//     public int damage;
//     private LineRenderer m_lineRender;
//     private GameObject m_player;
//     private float damageAccumulator = 0f;
//     public LayerMask ignoreLayers;

//     void Start()
//     {
//         m_player = PlayerManager.instance.player;
//         m_lineRender = GetComponent<LineRenderer>();
//     }

//     void Update()
//     {
//         paintColor = m_player.GetComponent<PlayerPaint>().selectedPaint;
//         damage = m_player.GetComponent<PlayerWeapon>().damage;
//         radius = m_player.GetComponent<PlayerWeapon>().paintRadius;

//         m_lineRender.startColor = paintColor;
//         m_lineRender.endColor = paintColor;

//         if (Input.GetButton("Fire1"))
//         {
//             m_lineRender.enabled = true;

//             rayCast = new Ray(rayStart.position, rayStart.forward);

//             if (Physics.Raycast(rayStart.position, rayStart.forward, out hit, rayDistance, ~ignoreLayers))
//             {
//                 m_lineRender.SetPosition(0, rayStart.position);
//                 m_lineRender.SetPosition(1, hit.point);

//                 Debug.Log("Hit: " + hit.transform.name);
//                 bool isPaintable = hit.collider.GetComponent<Paintable>() != null;
//                 Debug.Log("Paintable? " + isPaintable);

//                 // Paint if surface is paintable
//                 if (isPaintable)
//                 {
//                     TryPaint(hit);
//                 }

//                 if (hit.transform.tag == "Enemy")
//                 {
//                     damageAccumulator += damage * Time.deltaTime;
//                     if (damageAccumulator >= 1f)
//                     {
//                         int applyDamage = Mathf.FloorToInt(damageAccumulator);
//                         hit.transform.GetComponent<Health>().Damage(applyDamage);
//                         damageAccumulator -= applyDamage;
//                     }
//                 }
//             }
//             else
//             {
//                 m_lineRender.SetPosition(0, rayStart.position);
//                 m_lineRender.SetPosition(1, rayStart.position + rayStart.forward * rayDistance);
//             }
//         }
//         else
//         {
//             m_lineRender.enabled = false;
//         }
//     }
//     public void DestroyGun()
//     {
//         Destroy(gameObject);
//     }
// }

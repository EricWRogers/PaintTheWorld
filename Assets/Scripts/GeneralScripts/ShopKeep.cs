// using UnityEngine;

// public class ShopKeep : MonoBehaviour
// {
//     public float distance;
//     public GameObject ShopUI;
//     private bool m_isOpen;
//     private bool m_active;

//     // Start is called once before the first execution of Update after the MonoBehaviour is created
//     void Start()
//     {
        
//     }

//     // Update is called once per frame
//     void Update()
//     {
//         if (!GameManager.instance.inWave)
//         {
//             OpenShop();
//         }
//         else
//         {
//             CloseShop();
//         }

//         if (Vector3.Distance(PlayerManager.instance.player.transform.position, transform.position) <= distance && m_isOpen)
//         {
//             //show "e" to interact

//             if (Input.GetKeyDown(KeyCode.E))
//             {
//                 if (m_active)
//                 {
//                     DeactivateShopUI();
//                 }
//                 else
//                 {
//                     ActivateShopUI();
//                 }
                
//             }
//         }
//     }

//     public void ActivateShopUI()
//     {
//         GameManager.instance.PauseGame();
//         ShopUI.SetActive(true);
//     }
//     public void DeactivateShopUI()
//     {
//         GameManager.instance.ResumeGame();
//         ShopUI.SetActive(false);
//     }

//     public void OpenShop()
//     {
//         m_isOpen = true;
//     }
//     public void CloseShop()
//     {
//         m_isOpen = false;
//     }
//     void OnDrawGizmos()
//     {
//         Gizmos.color = Color.green;
//         Gizmos.DrawWireSphere(transform.position, distance);
//     }
// }

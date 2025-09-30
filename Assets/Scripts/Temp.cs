// using Unity.VisualScripting;
// using UnityEngine;
// using System.Collections.Generic;
// using UnityEngine.SceneManagement;

// #if UNITY_EDITOR

// using UnityEditor;
// [CustomEditor(typeof(PlayerManager))]

// public class PlayerManagerEditor : Editor
// {
//     public override void OnInspectorGUI()
//     {
//         base.OnInspectorGUI();using Unity.VisualScripting;
// using UnityEngine;
// using System.Collections.Generic;
// using UnityEngine.SceneManagement;

// #if UNITY_EDITOR

// using UnityEditor;
// [CustomEditor(typeof(pm))]

// public class PlayerManagerEditor : Editor
// {
//     public override void OnInspectorGUI()
//     {
//         base.OnInspectorGUI();
//         PlayerManager pm = (PlayerManager)target;

//         if (GUILayout.Button("Save Game"))
//         {
//             pm.SaveGame();
//         }
//         if (GUILayout.Button("Load Game"))
//         {
//             pm.LoadGame();
//         }
//         if (GUILayout.Button("Reset Save Data"))
//         {
//             pm.ResetData();
//         }
//     }
// }
// #endif

// public class pm : MonoBehaviour
// {
//     public static PlayerManager instance;
//     public GameObject player;

//     public Health health;

//     public WalletManager wallet;
//     public int levelStartingCoins = 10;



//     // Wapon Managerment
//     private WeaponManager wm;
//     [SerializeField] private string weaponsPath = "Weapons/";  // Path to weapons in Resources folder
//     public List<ScriptableObject> ownedWeapons = new List<ScriptableObject>();
//     public List<ScriptableObject> equipedWeapons = new List<ScriptableObject>();

//     //Save Vars
//     private SaveData saveData = new SaveData(10, 100, 100, new List<ScriptableObject>());
//     private const string SAVE_KEY = "PlayerSaveData";


//     void Awake()
//     {
//         if (instance == null)
//         {
//             instance = this;
//             DontDestroyOnLoad(this.gameObject);
//         }
//         else
//             Destroy(this);

//         InitializeComponents();
//         print("initialized components");
//         LoadGame();


//         print(equipedWeapons.Count);
//         equipedWeapons = ownedWeapons; //Change latter


//     }

//     private void InitializeComponents()
//     {
//         // Get required components
//         wallet = GetComponent<WalletManager>() == null ? this.AddComponent<WalletManager>() : GetComponent<WalletManager>();
//         health = GetComponent<Health>() == null ? this.AddComponent<Health>() : GetComponent<Health>();

//     }


//     public void SaveGame()
//     {
//         saveData = new SaveData(
//             health.currentHealth,
//             wallet.coins,
//             health.maxHealth,
//             ownedWeapons

//         );

//         string json = JsonUtility.ToJson(saveData, true);
//         PlayerPrefs.SetString(SAVE_KEY, json);
//         PlayerPrefs.Save();
//         Debug.Log("Game saved successfully");

//     }

//     public void LoadGame()
//     {

//         string json = PlayerPrefs.GetString(SAVE_KEY);
//         Debug.Log("Loading JSON: " + json);
//         saveData = JsonUtility.FromJson<SaveData>(json);
//         // Apply loaded data
//         Debug.Log("Loaded JSON: " + json);
//         health.SetCurrentHealth(saveData.health);
//         health.SetMaxHealth(saveData.maxHealth);
//         wallet.SetCoins(saveData.coins);
//         ownedWeapons.Clear();
//         Debug.Log("Loading owned weapons:");

//         foreach (string weaponName in saveData.ownedWeapons)
//         {
//             WeaponSO weapon = Resources.Load<WeaponSO>(weaponsPath + weaponName);
//             if (weapon != null)
//             {
//                 ownedWeapons.Add(weapon);
//             }
//             else
//             {
//                 Debug.LogWarning($"Weapon '{weaponName}' not found in Resources/{weaponsPath}");
//             }
//         }

//         Debug.Log("Game loaded successfully");

//     }

//     public void RegisterPlayer(GameObject _player)
//     {
//         player = _player;
//         print("player registered");
//         wm = player.AddComponent<WeaponManager>();

//         for (int i = 0; i < equipedWeapons.Count; i++)
//         {
//             Debug.Log("in FOR LOOP");
//             WeaponSO weapon = (WeaponSO)equipedWeapons[i];
//             Weapon weaponTemp = Instantiate(weapon.weaponPrefab, player.transform.position + Vector3.right * 0.5f, player.transform.rotation, player.transform).GetComponent<Pistol>();
//             print("weaponTemp");
//             wm.weapons.Add(weaponTemp);
//             if (i == 0)
//             {
//                 wm.currentWeapon = weaponTemp;
//                 wm.currentWeapon.gameObject.SetActive(true);
//             }
//             else
//             {
//                 weaponTemp.gameObject.SetActive(false);
//             }
//             Debug.Log("Added " + weapon.weaponName);

//         }
//     }


//     // Auto-save when application quits
//     private void OnApplicationQuit()
//     {
//         SaveGame();
//     }

//     public void OnDeath()
//     {
//         Debug.Log("Player Died");
//         SceneManager.LoadSceneAsync(0);
//         health.SetCurrentHealth(health.maxHealth);
//         wallet.SetCoins(levelStartingCoins);

//     }


//     public void ResetData()
//     {
//         List<ScriptableObject> weapon = new List<ScriptableObject>();
//         weapon.Add(Resources.Load<WeaponSO>(weaponsPath + "Pistol"));
//         weapon.Add(Resources.Load<WeaponSO>(weaponsPath + "Shotgun"));
//         SaveData tempSaveData = new SaveData(10, 100, 100, weapon);
//         PlayerPrefs.SetString(SAVE_KEY, JsonUtility.ToJson(tempSaveData, true));
//         Debug.Log("Save data reset.");
//     }


// }


// [System.Serializable]
// public class SaveData
// {
//     public int coins = 10;
//     public int health = 100;
//     public int maxHealth = 100;
//     public List<string> ownedWeapons = new List<string>();

//     public SaveData(int _coins, int _health, int _maxHealth, List<ScriptableObject> _ownedWeapons)
//     {
//         coins = _coins;
//         health = _health;
//         maxHealth = _maxHealth;

//         foreach (ScriptableObject weapon in _ownedWeapons)
//         {
//             ownedWeapons.Add(weapon.name);
//         }
    
//     }
// }
//         PlayerManager pm = (PlayerManager)target;

//         if (GUILayout.Button("Save Game"))
//         {
//             pm.SaveGame();
//         }
//         if (GUILayout.Button("Load Game"))
//         {
//             pm.LoadGame();
//         }
//         if (GUILayout.Button("Reset Save Data"))
//         {
//             pm.ResetData();
//         }
//     }
// }
// #endif

// public class pm : MonoBehaviour
// {
//     public static PlayerManager instance;
//     public GameObject player;

//     public Health health;

//     public WalletManager wallet;
//     public int levelStartingCoins = 10;



//     // Wapon Managerment
//     private WeaponManager wm;
//     [SerializeField] private string weaponsPath = "Weapons/";  // Path to weapons in Resources folder
//     public List<ScriptableObject> ownedWeapons = new List<ScriptableObject>();
//     public List<ScriptableObject> equipedWeapons = new List<ScriptableObject>();

//     //Save Vars
//     private SaveData saveData = new SaveData(10, 100, 100, new List<ScriptableObject>());
//     private const string SAVE_KEY = "PlayerSaveData";


//     void Awake()
//     {
//         if (instance == null)
//         {
//             instance = this;
//             DontDestroyOnLoad(this.gameObject);
//         }
//         else
//             Destroy(this);

//         InitializeComponents();
//         print("initialized components");
//         LoadGame();


//         print(equipedWeapons.Count);
//         equipedWeapons = ownedWeapons; //Change latter


//     }

//     private void InitializeComponents()
//     {
//         // Get required components
//         wallet = GetComponent<WalletManager>() == null ? this.AddComponent<WalletManager>() : GetComponent<WalletManager>();
//         health = GetComponent<Health>() == null ? this.AddComponent<Health>() : GetComponent<Health>();

//     }


//     public void SaveGame()
//     {
//         saveData = new SaveData(
//             health.currentHealth,
//             wallet.coins,
//             health.maxHealth,
//             ownedWeapons

//         );

//         string json = JsonUtility.ToJson(saveData, true);
//         PlayerPrefs.SetString(SAVE_KEY, json);
//         PlayerPrefs.Save();
//         Debug.Log("Game saved successfully");

//     }

//     public void LoadGame()
//     {

//         string json = PlayerPrefs.GetString(SAVE_KEY);
//         Debug.Log("Loading JSON: " + json);
//         saveData = JsonUtility.FromJson<SaveData>(json);
//         // Apply loaded data
//         Debug.Log("Loaded JSON: " + json);
//         health.SetCurrentHealth(saveData.health);
//         health.SetMaxHealth(saveData.maxHealth);
//         wallet.SetCoins(saveData.coins);
//         ownedWeapons.Clear();
//         Debug.Log("Loading owned weapons:");

//         foreach (string weaponName in saveData.ownedWeapons)
//         {
//             WeaponSO weapon = Resources.Load<WeaponSO>(weaponsPath + weaponName);
//             if (weapon != null)
//             {
//                 ownedWeapons.Add(weapon);
//             }
//             else
//             {
//                 Debug.LogWarning($"Weapon '{weaponName}' not found in Resources/{weaponsPath}");
//             }
//         }

//         Debug.Log("Game loaded successfully");

//     }

//     public void RegisterPlayer(GameObject _player)
//     {
//         player = _player;
//         print("player registered");
//         wm = player.AddComponent<WeaponManager>();

//         for (int i = 0; i < equipedWeapons.Count; i++)
//         {
//             Debug.Log("in FOR LOOP");
//             WeaponSO weapon = (WeaponSO)equipedWeapons[i];
//             Weapon weaponTemp = Instantiate(weapon.weaponPrefab, player.transform.position + Vector3.right * 0.5f, player.transform.rotation, player.transform).GetComponent<Pistol>();
//             print("weaponTemp");
//             wm.weapons.Add(weaponTemp);
//             if (i == 0)
//             {
//                 wm.currentWeapon = weaponTemp;
//                 wm.currentWeapon.gameObject.SetActive(true);
//             }
//             else
//             {
//                 weaponTemp.gameObject.SetActive(false);
//             }
//             Debug.Log("Added " + weapon.weaponName);

//         }
//     }


//     // Auto-save when application quits
//     private void OnApplicationQuit()
//     {
//         SaveGame();
//     }

//     public void OnDeath()
//     {
//         Debug.Log("Player Died");
//         SceneManager.LoadSceneAsync(0);
//         health.SetCurrentHealth(health.maxHealth);
//         wallet.SetCoins(levelStartingCoins);

//     }


//     public void ResetData()
//     {
//         List<ScriptableObject> weapon = new List<ScriptableObject>();
//         weapon.Add(Resources.Load<WeaponSO>(weaponsPath + "Pistol"));
//         weapon.Add(Resources.Load<WeaponSO>(weaponsPath + "Shotgun"));
//         SaveData tempSaveData = new SaveData(10, 100, 100, weapon);
//         PlayerPrefs.SetString(SAVE_KEY, JsonUtility.ToJson(tempSaveData, true));
//         Debug.Log("Save data reset.");
//     }


// }


// [System.Serializable]
// public class SaveData
// {
//     public int coins = 10;
//     public int health = 100;
//     public int maxHealth = 100;
//     public List<string> ownedWeapons = new List<string>();

//     public SaveData(int _coins, int _health, int _maxHealth, List<ScriptableObject> _ownedWeapons)
//     {
//         coins = _coins;
//         health = _health;
//         maxHealth = _maxHealth;

//         foreach (ScriptableObject weapon in _ownedWeapons)
//         {
//             ownedWeapons.Add(weapon.name);
//         }
    
//     }
// }
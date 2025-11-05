using System.Collections.Generic;
using KinematicCharacterControler;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DevTools : MonoBehaviour
{
    public GameObject playerPaint;
    public GameObject devToolUI;
    public List<Vector3> paintTrailPos;
    public Button paintPosButton;
    public GameObject gunOne;
    public Button button1;
    public GameObject gunTwo;
    public Button button2;
    public GameObject gunThree;
    public Button button3;
    public Slider playerMovement;
    public TextMeshProUGUI moveText;
    public Slider playerDamage;
    public TextMeshProUGUI damageText;
    public Slider paintRadius;
    public TextMeshProUGUI radiusText;
    public KeyCode debugButton;
    private bool m_toolsActive;
    private GameObject m_player;
    private int m_paintIndex = 0;

    void Start()
    {
        m_player = PlayerManager.instance.player;
        paintPosButton.onClick.AddListener(SwitchPaintPos);
        button1.onClick.AddListener(Gun1);
        button2.onClick.AddListener(Gun2);
        button3.onClick.AddListener(Gun3);
        
    }

    void Update()
    {
        if (Input.GetKeyDown(debugButton))
        {
            ToggleTools();
        }
        if (m_player == null)
            m_player = PlayerManager.instance.player;
        m_player.GetComponent<PlayerMovement>().speed = playerMovement.value;
        m_player.GetComponent<PlayerWeapon>().paintRadius = paintRadius.value;
        m_player.GetComponent<PlayerWeapon>().damage = (int)playerDamage.value;
        moveText.text = playerMovement.value.ToString();
        radiusText.text = paintRadius.value.ToString();
        damageText.text = ((int)playerDamage.value).ToString();
    }
    private void SwitchPaintPos()
    {
        if (paintTrailPos.Count == 0) return;

        playerPaint.transform.localPosition = paintTrailPos[m_paintIndex];

        m_paintIndex++;
        if (m_paintIndex >= paintTrailPos.Count)
        {
            m_paintIndex = 0;
        }
    }
    private void Gun1()
    {
        m_player.GetComponent<PlayerWeapon>().ChangeWeapon(gunOne);
    }
    private void Gun2()
    {
        m_player.GetComponent<PlayerWeapon>().ChangeWeapon(gunTwo);
    }
    private void Gun3()
    {
        m_player.GetComponent<PlayerWeapon>().ChangeWeapon(gunThree);
    }

    private void ToggleTools()
    {
        if (!m_toolsActive)
        {
            Time.timeScale = 0;
            devToolUI.SetActive(true);
            PlayerManager.instance.player.GetComponent<PlayerMovement>().lockCursor = false;
            m_toolsActive = true;
        }
        else
        {
            Time.timeScale = 1;
            devToolUI.SetActive(false);
            PlayerManager.instance.player.GetComponent<PlayerMovement>().lockCursor = true;
            m_toolsActive = false;
        }
    }
}

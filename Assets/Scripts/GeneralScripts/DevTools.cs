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
    public GameObject gunTwo;
    public GameObject gunThree;
    public Slider playerMovement;
    public TextMeshProUGUI moveText;
    public Slider playerDamage;
    public TextMeshProUGUI damageText;
    public Slider paintRadius;
    public TextMeshProUGUI radiusText;
    public KeyCode debugButton;
    private bool m_toolsActive;
    private GameObject m_player;

    void Start()
    {
        m_player = PlayerManager.instance.player;
        paintPosButton.onClick.AddListener(SwitchPaintPos);
    }

    void Update()
    {
        if (Input.GetKeyDown(debugButton))
        {
            ToggleTools();
        }
        m_player.GetComponent<PlayerMovement>().speed = playerMovement.value;
        m_player.GetComponent<PlayerWeapon>().paintRadius = paintRadius.value;
        m_player.GetComponent<PlayerWeapon>().damage = (int)playerDamage.value;
        moveText.text = playerMovement.value.ToString();
        damageText.text = paintRadius.value.ToString();
        radiusText.text = ((int)playerDamage.value).ToString();
    }
    private void SwitchPaintPos()
    {
        for (int i = 0; i < paintTrailPos.Count - 1; i++)
        {
            playerPaint.transform.localPosition = paintTrailPos[i];
            if (i == paintTrailPos.Count - 1)
            {
                i = 0;
            }
        }
    }

    private void ToggleTools()
    {
        if (!m_toolsActive)
        {
            Time.timeScale = 0;
            devToolUI.SetActive(true);
            PlayerManager.instance.player.GetComponent<PlayerMovement>().lockCursor = false;
            PlayerManager.instance.player.GetComponent<PlayerMovement>().HandleCursor();
            m_toolsActive = true;
        }
        else
        {
            Time.timeScale = 1;
            devToolUI.SetActive(false);
            PlayerManager.instance.player.GetComponent<PlayerMovement>().lockCursor = true;
            PlayerManager.instance.player.GetComponent<PlayerMovement>().HandleCursor();
            m_toolsActive = false;
        }
    }
}

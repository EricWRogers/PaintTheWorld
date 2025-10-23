using SuperPupSystems.Helper;
using UnityEngine;

public class PrepTimer : CountDownText
{
    void Start()
    {
        timer = GameManager.instance.GetComponent<Timer>();
    }
    void Update()
    {
        text.text = textHeader + Mathf.Ceil(time);
        if (timer.timeLeft <= 0)
        {
            text.alpha = 0;
        }
        else
        {
            text.alpha = 255;
        }
        
    }
}

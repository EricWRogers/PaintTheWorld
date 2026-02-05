using UnityEngine;
using TMPro;
using System.Collections;

public class KioskToast : MonoBehaviour
{
    public TMP_Text label;
    Coroutine co;

    public void Show(string msg, float seconds = 1.5f)
    {
        if (co != null) StopCoroutine(co);
        co = StartCoroutine(Run(msg, seconds));
    }

    IEnumerator Run(string msg, float seconds)
    {
        label.gameObject.SetActive(true);
        label.text = msg;
        yield return new WaitForSeconds(seconds);
        label.gameObject.SetActive(false);
    }
}

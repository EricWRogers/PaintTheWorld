using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using SuperPupSystems.Helper;

[RequireComponent(typeof(CanvasGroup))]
public class HealthBar : MonoBehaviour
{
    [Header("Segmented UI Images")]
    [SerializeField] private List<Image> healthSegments = new List<Image>();
    
    [Header("Pulse Settings")]
    [SerializeField] private float pulseScale = 1.2f;
    [SerializeField] private float pulseDuration = 0.15f;
    [SerializeField] private Color healColor = Color.green;

    private Health health;
    private int lastHealth;

    void Start()
    {
        if (PlayerManager.instance != null)
        {
            health = PlayerManager.instance.health;
            if (health != null)
            {
                lastHealth = health.currentHealth;
            }
        }

        if (healthSegments.Count == 0)
        {
            healthSegments.Clear();
            foreach (Transform child in transform)
            {
                Image img = child.GetComponent<Image>();
                if (img != null) healthSegments.Add(img);
            }
        }
            
        RefreshUI();
    }

    void Update()
    {
        if (health == null) return;

        if (health.currentHealth < lastHealth)
        {
            OnDamageTaken();
        }
        else if (health.currentHealth > lastHealth)
        {
            OnHealed();
        }

        lastHealth = health.currentHealth;
    }

    private void OnDamageTaken()
    {
        for (int i = healthSegments.Count - 1; i >= 0; i--)
        {
            if (i >= health.currentHealth && healthSegments[i].gameObject.activeSelf)
            {
                StartCoroutine(PulseAndHide(healthSegments[i]));
            }
        }
    }

    private void OnHealed()
    {
        for (int i = 0; i < healthSegments.Count; i++)
        {
            if (i < health.currentHealth && !healthSegments[i].gameObject.activeSelf)
            {
                StartCoroutine(PulseAndShow(healthSegments[i]));
            }
        }
    }

    private IEnumerator PulseAndHide(Image segment)
    {
        float elapsed = 0f;
        while (elapsed < pulseDuration)
        {
            segment.transform.localScale = Vector3.Lerp(Vector3.one, Vector3.one * pulseScale, elapsed / pulseDuration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        elapsed = 0f;
        while (elapsed < pulseDuration)
        {
            segment.transform.localScale = Vector3.Lerp(Vector3.one * pulseScale, Vector3.zero, elapsed / pulseDuration);
            elapsed += Time.deltaTime;
            yield return null;
        }
        segment.gameObject.SetActive(false);
    }

    private IEnumerator PulseAndShow(Image segment)
    {
        segment.gameObject.SetActive(true);
        segment.transform.localScale = Vector3.zero;
        Color originalColor = Color.white;

        float elapsed = 0f;
        while (elapsed < pulseDuration)
        {
            float t = elapsed / pulseDuration;
            segment.transform.localScale = Vector3.Lerp(Vector3.zero, Vector3.one * pulseScale, t);
            segment.color = Color.Lerp(originalColor, healColor, t);
            elapsed += Time.deltaTime;
            yield return null;
        }

        elapsed = 0f;
        while (elapsed < pulseDuration)
        {
            float t = elapsed / pulseDuration;
            segment.transform.localScale = Vector3.Lerp(Vector3.one * pulseScale, Vector3.one, t);
            segment.color = Color.Lerp(healColor, originalColor, t);
            elapsed += Time.deltaTime;
            yield return null;
        }

        segment.transform.localScale = Vector3.one;
        segment.color = originalColor;
    }

    private void RefreshUI()
    {
        if (health == null) return;
        for (int i = 0; i < healthSegments.Count; i++)
        {
            bool shouldBeActive = i < health.currentHealth;
            healthSegments[i].gameObject.SetActive(shouldBeActive);
            healthSegments[i].transform.localScale = shouldBeActive ? Vector3.one : Vector3.zero;
        }
    }
}
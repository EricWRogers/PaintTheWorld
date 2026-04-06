using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using SuperPupSystems.Helper;

[RequireComponent(typeof(CanvasGroup))]
public class HealthBar : MonoBehaviour
{
    [Header("Base Health Images")]
    [SerializeField] private List<Image> baseHealthSegments = new List<Image>();

    [Header("Bonus Health Images (max 4)")]
    [SerializeField] private List<Image> bonusHealthSegments = new List<Image>();

    [Header("Pulse Settings")]
    [SerializeField] private float pulseScale = 1.2f;
    [SerializeField] private float pulseDuration = 0.15f;
    [SerializeField] private Color healColor = Color.green;

    private Health health;
    private PlayerManager pm;

    private int displayedHealth;
    private int lastBonusHealth;
    private Vector3 originalSegmentScale = Vector3.one;

    private bool isAnimating = false;

    void Start()
    {
        if (baseHealthSegments.Count > 0 && baseHealthSegments[0] != null)
        {
            originalSegmentScale = baseHealthSegments[0].transform.localScale;
        }
        
        Rebind();
        ForceRefresh();
    }

    void Update()
    {
        // Rebind if scene changed
        if (PlayerManager.instance != pm || (PlayerManager.instance != null && PlayerManager.instance.health != health))
        {
            Rebind();
            ForceRefresh();
            return;
        }

        if (pm == null || health == null) return;

        
        if (pm.bonusHealthFromItems != lastBonusHealth)
        {
            ForceRefresh();
            lastBonusHealth = pm.bonusHealthFromItems;
            return;
        }

        if (isAnimating) return;

        int targetHealth = Mathf.Clamp(health.currentHealth, 0, GetVisibleSegmentCount());

        if (targetHealth < displayedHealth)
        {
            StartCoroutine(AnimateDamage(displayedHealth - targetHealth));
        }
        else if (targetHealth > displayedHealth)
        {
            StartCoroutine(AnimateHeal(targetHealth - displayedHealth));
        }
    }

    void Rebind()
    {
        pm = PlayerManager.instance;
        health = pm != null ? pm.health : null;
        lastBonusHealth = pm != null ? pm.bonusHealthFromItems : 0;
    }

    int GetVisibleSegmentCount()
    {
        if (pm == null) return baseHealthSegments.Count;
        return baseHealthSegments.Count + Mathf.Clamp(pm.bonusHealthFromItems, 0, bonusHealthSegments.Count);
    }

    List<Image> GetVisibleSegments()
    {
        List<Image> result = new List<Image>();

        for (int i = 0; i < baseHealthSegments.Count; i++)
            if (baseHealthSegments[i] != null)
                result.Add(baseHealthSegments[i]);

        int bonusCount = pm != null ? Mathf.Clamp(pm.bonusHealthFromItems, 0, bonusHealthSegments.Count) : 0;
        for (int i = 0; i < bonusCount; i++)
            if (bonusHealthSegments[i] != null)
                result.Add(bonusHealthSegments[i]);

        return result;
    }

    void ForceRefresh()
    {
        if (pm == null || health == null) return;

        StopAllCoroutines();
        isAnimating = false;

        List<Image> visible = GetVisibleSegments();
        int visibleCount = visible.Count;
        int currentHealth = Mathf.Clamp(health.currentHealth, 0, visibleCount);

        // Hide unused bonus hearts completely
        for (int i = 0; i < bonusHealthSegments.Count; i++)
        {
            if (bonusHealthSegments[i] == null) continue;

            bool shouldExist = i < Mathf.Clamp(pm.bonusHealthFromItems, 0, bonusHealthSegments.Count);
            if (!shouldExist)
            {
                bonusHealthSegments[i].gameObject.SetActive(false);
                bonusHealthSegments[i].transform.localScale = Vector3.zero;
                bonusHealthSegments[i].color = Color.white;
            }
        }

        
        for (int i = 0; i < visible.Count; i++)
        {
            bool filled = i < currentHealth;
            visible[i].gameObject.SetActive(filled);
            visible[i].transform.localScale = filled ? originalSegmentScale : Vector3.zero;
            visible[i].color = Color.white;
        }

        displayedHealth = currentHealth;
    }

    IEnumerator AnimateDamage(int amount)
    {
        isAnimating = true;

        List<Image> visible = GetVisibleSegments();

        for (int n = 0; n < amount; n++)
        {
            int indexToHide = displayedHealth - 1;
            if (indexToHide >= 0 && indexToHide < visible.Count)
            {
                yield return StartCoroutine(PulseAndHide(visible[indexToHide]));
                displayedHealth--;
            }
        }

        isAnimating = false;
    }

    IEnumerator AnimateHeal(int amount)
    {
        isAnimating = true;

        List<Image> visible = GetVisibleSegments();

        for (int n = 0; n < amount; n++)
        {
            int indexToShow = displayedHealth;
            if (indexToShow >= 0 && indexToShow < visible.Count)
            {
                yield return StartCoroutine(PulseAndShow(visible[indexToShow]));
                displayedHealth++;
            }
        }

        isAnimating = false;
    }

    private IEnumerator PulseAndHide(Image segment)
    {
        float elapsed = 0f;
        while (elapsed < pulseDuration)
        {
            segment.transform.localScale = Vector3.Lerp(originalSegmentScale, originalSegmentScale * pulseScale, elapsed / pulseDuration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        elapsed = 0f;
        while (elapsed < pulseDuration)
        {
            segment.transform.localScale = Vector3.Lerp(originalSegmentScale * pulseScale, originalSegmentScale, elapsed / pulseDuration);
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
            segment.transform.localScale = Vector3.Lerp(Vector3.zero, originalSegmentScale * pulseScale, t);
            segment.color = Color.Lerp(originalColor, healColor, t);
            elapsed += Time.deltaTime;
            yield return null;
        }

        elapsed = 0f;
        while (elapsed < pulseDuration)
        {
            float t = elapsed / pulseDuration;
            segment.transform.localScale = Vector3.Lerp(originalSegmentScale * pulseScale, originalSegmentScale, t);
            segment.color = Color.Lerp(healColor, originalColor, t);
            elapsed += Time.deltaTime;
            yield return null;
        }

        segment.transform.localScale = originalSegmentScale;
        segment.color = originalColor;
    }
}
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

    private readonly HashSet<Image> animating = new HashSet<Image>();
    private readonly List<Image> allSegments = new List<Image>();

    void Start()
    {
        Rebind();
        BuildSegmentList();
        ForceImmediateRefresh();
    }

    void Update()
    {
        // Rebind safely after scene loads
        if (PlayerManager.instance != pm || (PlayerManager.instance != null && PlayerManager.instance.health != health))
        {
            Rebind();
            BuildSegmentList();
            ForceImmediateRefresh();
            return;
        }

        if (pm == null || health == null) return;

        int baseCount = baseHealthSegments.Count;
        int bonusCount = Mathf.Clamp(pm.bonusHealthFromItems, 0, bonusHealthSegments.Count);
        int totalVisibleSlots = baseCount + bonusCount;
        int currentHealth = Mathf.Clamp(health.currentHealth, 0, totalVisibleSlots);

        // Base hearts always exist as slots
        for (int i = 0; i < baseHealthSegments.Count; i++)
        {
            if (baseHealthSegments[i] == null) continue;

            bool shouldBeFilled = i < currentHealth;
            SyncSegment(baseHealthSegments[i], shouldBeFilled);
        }

        // Bonus hearts only exist if earned
        for (int i = 0; i < bonusHealthSegments.Count; i++)
        {
            if (bonusHealthSegments[i] == null) continue;

            bool slotExists = i < bonusCount;
            bool shouldBeFilled = slotExists && (baseCount + i) < currentHealth;

            if (!slotExists)
            {
                // bonus slot doesn't exist at all
                if (bonusHealthSegments[i].gameObject.activeSelf || bonusHealthSegments[i].transform.localScale != Vector3.zero)
                {
                    bonusHealthSegments[i].gameObject.SetActive(false);
                    bonusHealthSegments[i].transform.localScale = Vector3.zero;
                    bonusHealthSegments[i].color = Color.white;
                }
            }
            else
            {
                SyncSegment(bonusHealthSegments[i], shouldBeFilled);
            }
        }
    }

    void Rebind()
    {
        pm = PlayerManager.instance;
        health = pm != null ? pm.health : null;
    }

    void BuildSegmentList()
    {
        allSegments.Clear();
        allSegments.AddRange(baseHealthSegments);
        allSegments.AddRange(bonusHealthSegments);
    }

    void ForceImmediateRefresh()
    {
        if (pm == null || health == null) return;

        int baseCount = baseHealthSegments.Count;
        int bonusCount = Mathf.Clamp(pm.bonusHealthFromItems, 0, bonusHealthSegments.Count);
        int totalVisibleSlots = baseCount + bonusCount;
        int currentHealth = Mathf.Clamp(health.currentHealth, 0, totalVisibleSlots);

        for (int i = 0; i < baseHealthSegments.Count; i++)
        {
            if (baseHealthSegments[i] == null) continue;

            bool shouldBeFilled = i < currentHealth;
            baseHealthSegments[i].gameObject.SetActive(shouldBeFilled);
            baseHealthSegments[i].transform.localScale = shouldBeFilled ? Vector3.one : Vector3.zero;
            baseHealthSegments[i].color = Color.white;
        }

        for (int i = 0; i < bonusHealthSegments.Count; i++)
        {
            if (bonusHealthSegments[i] == null) continue;

            bool slotExists = i < bonusCount;
            bool shouldBeFilled = slotExists && (baseCount + i) < currentHealth;

            bonusHealthSegments[i].gameObject.SetActive(shouldBeFilled);
            bonusHealthSegments[i].transform.localScale = shouldBeFilled ? Vector3.one : Vector3.zero;
            bonusHealthSegments[i].color = Color.white;
        }
    }

    void SyncSegment(Image segment, bool shouldBeFilled)
    {
        if (segment == null) return;
        if (animating.Contains(segment)) return;

        bool isFilledNow = segment.gameObject.activeSelf;

        if (shouldBeFilled && !isFilledNow)
        {
            StartCoroutine(PulseAndShow(segment));
        }
        else if (!shouldBeFilled && isFilledNow)
        {
            StartCoroutine(PulseAndHide(segment));
        }
    }

    private IEnumerator PulseAndHide(Image segment)
    {
        animating.Add(segment);

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
        segment.transform.localScale = Vector3.zero;
        animating.Remove(segment);
    }

    private IEnumerator PulseAndShow(Image segment)
    {
        animating.Add(segment);

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
        animating.Remove(segment);
    }
}
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using SuperPupSystems.Helper;

public class HealthBar : MonoBehaviour
{
    [Header("Health Bar UI")]
    [SerializeField] private Image healthFillImage;
    [SerializeField] private List<Image> uiImages = new List<Image>();
    
    [Header("Display Settings")]
    [SerializeField] private float outOfCombatDelay = 3f;
    [SerializeField] private float fadeDuration = 0.3f;
    private Health health;
    private CanvasGroup canvasGroup;
    private float timeSinceLastDamage = 0f;
    private bool isVisible = true;
    private Coroutine fadeCoroutine;

    void Start()
    {
        if (PlayerManager.instance != null)
        {
            health = PlayerManager.instance.health;
            
            if (health != null)
                health.hurt.AddListener(OnDamageTaken);
        }
        
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        
        if (uiImages.Count == 0)
            uiImages.AddRange(GetComponentsInChildren<Image>());
    }

    void Update()
    {
        if (health == null) return;
        
        if (healthFillImage != null)
            healthFillImage.fillAmount = (float)health.currentHealth / health.maxHealth;
        
        timeSinceLastDamage += Time.deltaTime;
        
        if (!isVisible && timeSinceLastDamage >= outOfCombatDelay)
            ShowUI();
    }

    private void OnDamageTaken()
    {
        timeSinceLastDamage = 0f;
        
        if (isVisible)
            HideUI();
    }

    private void HideUI()
    {
        if (!isVisible) return;
        
        isVisible = false;
        
        if (fadeCoroutine != null)
            StopCoroutine(fadeCoroutine);
        
        fadeCoroutine = StartCoroutine(FadeUI(1f, 0f));
    }

    private void ShowUI()
    {
        if (isVisible) return;
        
        isVisible = true;
        
        if (fadeCoroutine != null)
            StopCoroutine(fadeCoroutine);
        
        fadeCoroutine = StartCoroutine(FadeUI(0f, 1f));
    }

    private System.Collections.IEnumerator FadeUI(float startAlpha, float endAlpha)
    {
        float elapsed = 0f;
        
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(startAlpha, endAlpha, elapsed / fadeDuration);
            
            canvasGroup.alpha = alpha;
            
            foreach (Image img in uiImages)
            {
                if (img != null)
                {
                    Color color = img.color;
                    color.a = alpha;
                    img.color = color;
                }
            }
            
            yield return null;
        }
        
        canvasGroup.alpha = endAlpha;
        foreach (Image img in uiImages)
        {
            if (img != null)
            {
                Color color = img.color;
                color.a = endAlpha;
                img.color = color;
            }
        }
    }

    void OnDestroy()
    {
        if (health != null)
            health.hurt.RemoveListener(OnDamageTaken);
    }
}

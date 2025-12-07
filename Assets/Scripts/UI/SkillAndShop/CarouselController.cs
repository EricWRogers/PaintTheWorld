using UnityEngine;
using UnityEngine.UI;

public class CarouselController : MonoBehaviour
{
    [Header("Wiring")]
    public ScrollRect scrollRect;
    public Scrollbar scrollbar;         // the scrollbar
    public Button arrowLeft;
    public Button arrowRight;

    [Header("Behaviour")]
    public int pageSize = 1;            // how many cards per “page”
    public float fallbackStep = 0.25f;  // used if layout size can’t be read

    RectTransform content;

    void Awake()
    {
        content = scrollRect ? scrollRect.content : null;
        if (scrollRect && scrollbar) scrollRect.horizontalScrollbar = scrollbar;
        if (arrowLeft)  { arrowLeft.onClick.RemoveAllListeners();  arrowLeft.onClick.AddListener(() => Nudge(-1)); }
        if (arrowRight) { arrowRight.onClick.RemoveAllListeners(); arrowRight.onClick.AddListener(() => Nudge(+1)); }
    }

    void Nudge(int dir)
    {
        if (!scrollRect) return;

        float step = fallbackStep;
        var vp = scrollRect.viewport;
        if (content && vp)
        {
            float cw = content.rect.width;
            float vw = vp.rect.width;
            if (cw > 0f && vw > 0f)
            {
                float pageNorm = Mathf.Clamp01(vw / cw);
                step = Mathf.Max(pageNorm * pageSize, 0.05f);
            }
        }

        float t = Mathf.Clamp01(scrollRect.horizontalNormalizedPosition + dir * step);
        scrollRect.horizontalNormalizedPosition = t;
        if (scrollbar) scrollbar.value = t;
    }
}

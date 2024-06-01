using UnityEngine;
using UnityEngine.UI;

public class ScrollViewHider : MonoBehaviour
{
    public ScrollRect scrollRect;
    public RectTransform content;
    public float fadeDistance = 100f; // Distance over which the elements will fade

    void Start()
    {
        if (scrollRect == null)
            scrollRect = GetComponent<ScrollRect>();
        if (content == null)
            content = scrollRect.content;
    }

    void Update()
    {
        float scrollPosition = scrollRect.verticalNormalizedPosition;
        foreach (Transform child in content)
        {
            RectTransform childRect = child.GetComponent<RectTransform>();
            if (childRect != null)
            {
                float distanceToView = Mathf.Abs(childRect.anchoredPosition.y - content.anchoredPosition.y);
                float alpha = Mathf.Clamp01(1 - (distanceToView / fadeDistance));
                SetAlpha(child.gameObject, alpha);
            }
        }
    }

    void SetAlpha(GameObject obj, float alpha)
    {
        CanvasGroup canvasGroup = obj.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = obj.AddComponent<CanvasGroup>();
        }
        canvasGroup.alpha = alpha;
    }
}

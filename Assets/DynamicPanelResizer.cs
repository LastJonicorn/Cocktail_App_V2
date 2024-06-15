using UnityEngine;
using UnityEngine.UI;

public class DynamicPanelResizer : MonoBehaviour
{
    public RectTransform panel; // Reference to the panel's RectTransform
    public RectTransform contentContainer; // Reference to the container that holds the content

    private void Start()
    {
        AdjustPanelHeight();
    }

    private void Update()
    {
        AdjustPanelHeight(); // Adjust the panel height dynamically (optional: can be called less frequently if performance is a concern)
    }

    public void AdjustPanelHeight()
    {
        float totalHeight = 0;

        foreach (RectTransform child in contentContainer)
        {
            if (child.gameObject.activeSelf) // Only consider active children
            {
                totalHeight += child.rect.height;
                totalHeight += contentContainer.GetComponent<VerticalLayoutGroup>().spacing; // Include spacing
            }
        }

        // Remove the last spacing addition (optional)
        if (contentContainer.childCount > 0)
        {
            totalHeight -= contentContainer.GetComponent<VerticalLayoutGroup>().spacing;
        }

        panel.sizeDelta = new Vector2(panel.sizeDelta.x, totalHeight);
    }
}

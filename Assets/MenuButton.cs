using UnityEngine;
using UnityEngine.UI;

public class ToggleElement : MonoBehaviour
{
    public GameObject element; // The element to enable/disable
    private Button menuButton; // Reference to the menuButton component

    void Start()
    {
        menuButton = GetComponent<Button>();
        menuButton.onClick.AddListener(Toggle);

        element.SetActive(false);
    }

    void Toggle()
    {
        if (element != null)
        {
            element.SetActive(!element.activeSelf);
        }
    }
}

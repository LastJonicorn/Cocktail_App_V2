using UnityEngine;
using UnityEngine.UI;

public class ToggleElement : MonoBehaviour
{
    public GameObject elementToToggle; // The elementToToggleOn to enable/disable
    public GameObject secondElementToToggle; // The elementToToggleOn to enable/disable
    private Button toggleButton; // Reference to the toggleButton component

    void Start()
    {
        toggleButton = GetComponent<Button>();
        toggleButton.onClick.AddListener(Toggle);
        toggleButton.onClick.AddListener(ToggleSecond);

        elementToToggle.SetActive(false);
    }

    void Toggle()
    {
        if (elementToToggle != null)
        {
            elementToToggle.SetActive(!elementToToggle.activeSelf);
        }
    }

    void ToggleSecond()
    {
        if (secondElementToToggle != null)
        {
            secondElementToToggle.SetActive(!secondElementToToggle.activeSelf);
        }
    }
}

using UnityEngine;
using UnityEngine.UI;

public class ToggleElementOn : MonoBehaviour
{
    public GameObject elementToToggleOn; // The elementToToggleOn to enable/disable
    public GameObject secondElementToToggleOn; // The elementToToggleOn to enable/disable
    private Button toggleButton; // Reference to the toggleButton component

    void Start()
    {
        toggleButton = GetComponent<Button>();
        toggleButton.onClick.AddListener(Toggle);
        toggleButton.onClick.AddListener(ToggleSecond);
    }

    void Toggle()
    {
        if (elementToToggleOn != null)
        {
            elementToToggleOn.SetActive(true);
        }
    }

    void ToggleSecond()
    {
        if (secondElementToToggleOn != null)
        {
            secondElementToToggleOn.SetActive(true);
        }
    }
}

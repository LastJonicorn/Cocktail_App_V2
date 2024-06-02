using UnityEngine;
using UnityEngine.UI;

public class ToggleElementOff : MonoBehaviour
{
    public GameObject elementToToggleOff; // The elementToToggleOn to enable/disable
    public GameObject secondElementToToggleOff; // The elementToToggleOn to enable/disable
    private Button toggleButton; // Reference to the toggleButton component

    void Start()
    {
        toggleButton = GetComponent<Button>();
        toggleButton.onClick.AddListener(Toggle);
        toggleButton.onClick.AddListener(ToggleSecond);
    }

    void Toggle()
    {
        if (elementToToggleOff != null)
        {
            elementToToggleOff.SetActive(false);
        }
    }

    void ToggleSecond()
    {
        if (secondElementToToggleOff != null)
        {
            secondElementToToggleOff.SetActive(false);
        }
    }
}

using UnityEngine;
using UnityEngine.UI;

public class ToggleElementOff : MonoBehaviour
{
    [Header("Menu Items")]

    public GameObject elementToToggleOff; // The elementToToggleOn to enable/disable
    public GameObject secondElementToToggleOff; // The elementToToggleOn to enable/disable
    public GameObject thirdElementToToggleOff; // The elementToToggleOn to enable/disable
    public GameObject fourthElementToToggleOff; // The elementToToggleOn to enable/disable

    [Header("Screens")]

    public GameObject fifthElementToToggleOff; // The elementToToggleOn to enable/disable

    [Header("Menu Panel")]

    public GameObject sixthElementToToggleOff; // The elementToToggleOn to enable/disable

    private Button toggleButton; // Reference to the toggleButton component

    void Start()
    {
        toggleButton = GetComponent<Button>();
        toggleButton.onClick.AddListener(Toggle);
        toggleButton.onClick.AddListener(ToggleSecond);
        toggleButton.onClick.AddListener(ToggleThird);
        toggleButton.onClick.AddListener(ToggleFourth);
        toggleButton.onClick.AddListener(ToggleFifth);
        toggleButton.onClick.AddListener(ToggleSixth);

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

    void ToggleThird()
    {
        if (thirdElementToToggleOff != null)
        {
            thirdElementToToggleOff.SetActive(false);
        }
    }

    void ToggleFourth()
    {
        if (fourthElementToToggleOff != null)
        {
            fourthElementToToggleOff.SetActive(false);
        }
    }

    void ToggleFifth()
    {
        if (fifthElementToToggleOff != null)
        {
            fifthElementToToggleOff.SetActive(false);
        }
    }

    void ToggleSixth()
    {
        if (sixthElementToToggleOff != null)
        {
            sixthElementToToggleOff.SetActive(false);
        }
    }
}

using UnityEngine;
using UnityEngine.UI;

public class ToggleElementOff : MonoBehaviour
{
    [Header("Menu Items")]

    public GameObject infoScreen; // The elementToToggleOn to enable/disable
    public GameObject addDrinkScreen; // The elementToToggleOn to enable/disable
    public GameObject yourDrinksScreen; // The elementToToggleOn to enable/disable
    public GameObject favLiquorScreen; // The elementToToggleOn to enable/disable

    [Header("Screens")]

    public GameObject searchScreen; // The elementToToggleOn to enable/disable
    public GameObject randomScreen; // The elementToToggleOn to enable/disable
    public GameObject favoritesScreen; // The elementToToggleOn to enable/disable

    [Header("Menu Panel")]

    public GameObject menuPanel; // The elementToToggleOn to enable/disable

    [Header("Camera Controller")]

    public GameObject cameraController; // The elementToToggleOn to enable/disable

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
        toggleButton.onClick.AddListener(ToggleSeventh);
        toggleButton.onClick.AddListener(ToggleEighth);
        toggleButton.onClick.AddListener(ToggleNinth);
    }

    void Toggle()
    {
        if (infoScreen != null)
        {
            infoScreen.SetActive(false);
        }
    }

    void ToggleSecond()
    {
        if (addDrinkScreen != null)
        {
            addDrinkScreen.SetActive(false);
        }
    }

    void ToggleThird()
    {
        if (yourDrinksScreen != null)
        {
            yourDrinksScreen.SetActive(false);
        }
    }

    void ToggleFourth()
    {
        if (favLiquorScreen != null)
        {
            favLiquorScreen.SetActive(false);
        }
    }

    void ToggleFifth()
    {
        if (searchScreen != null)
        {
            searchScreen.SetActive(false);
        }
    }

    void ToggleSixth()
    {
        if (menuPanel != null)
        {
            menuPanel.SetActive(false);
        }
    }

    void ToggleSeventh()
    {
        if (randomScreen != null)
        {
            randomScreen.SetActive(false);
        }
    }
    void ToggleEighth()
    {
        if (favoritesScreen != null)
        {
            favoritesScreen.SetActive(false);
        }
    }

    void ToggleNinth()
    {
        if (cameraController != null)
        {
            cameraController.SetActive(false);
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AddFavorite : MonoBehaviour
{
    public Button addToFavoritesButton;
    private Drink currentDrink;

    // Define the colors for the button states
    public Color defaultColor = Color.white;
    public Color inFavoritesColor = Color.green;

    void Start()
    {
        // Check if the addToFavoritesButton is assigned
        if (addToFavoritesButton != null)
        {
            // Add a listener to the button to call AddCurrentDrinkToFavorites when clicked
            addToFavoritesButton.onClick.AddListener(AddCurrentDrinkToFavorites);
        }
        else
        {
            Debug.LogError("Add to Favorites Button is not assigned in the inspector.");
        }
    }

    // Method to set the current drink
    public void SetCurrentDrink(Drink drink)
    {
        currentDrink = drink;

        // Check if the drink is already in favorites and change button color accordingly
        if (currentDrink != null && FavoritesManager.Instance.IsFavorite(currentDrink.strDrink))
        {
            // Change button color to indicate the drink is already in favorites
            SetButtonColor(inFavoritesColor);
        }
        else
        {
            // Reset button color to default
            SetButtonColor(defaultColor);
        }
    }

    // Method called when the button is clicked
    void AddCurrentDrinkToFavorites()
    {
        if (currentDrink != null)
        {
            // Check if the drink is already in favorites
            if (FavoritesManager.Instance.IsFavorite(currentDrink.strDrink))
            {
                Debug.LogWarning("Drink is already in favorites: " + currentDrink.strDrink);
            }
            else
            {
                // Add the current drink to favorites using the FavoritesManager instance
                FavoritesManager.Instance.AddToFavorites(currentDrink);
                Debug.Log("Added to favorites: " + currentDrink.strDrink);

                // Change button color to indicate the drink is now in favorites
                SetButtonColor(inFavoritesColor);
            }
        }
        else
        {
            Debug.LogWarning("No current drink set. Cannot add to favorites.");
        }
    }

    // Method to set the button color
    void SetButtonColor(Color color)
    {
        // Get the button's image component
        Image buttonImage = addToFavoritesButton.GetComponent<Image>();
        if (buttonImage != null)
        {
            // Modify the image color directly
            buttonImage.color = color;
        }
        else
        {
            Debug.LogError("Image component not found on the button.");
        }
    }
}

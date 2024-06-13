using UnityEngine;
using UnityEngine.UI;

public class AddToFavorites : MonoBehaviour
{
    public Button addToFavoritesButton;
    private Drink currentDrink;

    void Start()
    {
        if (addToFavoritesButton != null)
        {
            addToFavoritesButton.onClick.AddListener(AddCurrentDrinkToFavorites);
        }
    }

    public void SetCurrentDrink(Drink drink)
    {
        currentDrink = drink;
    }

    void AddCurrentDrinkToFavorites()
    {
        if (currentDrink != null)
        {
            FavoritesManager.Instance.AddToFavorites(currentDrink);
            Debug.Log("Added to favorites: " + currentDrink.strDrink);
        }
    }

    // Subscribe to the OnFavoriteClicked event
    void OnEnable()
    {
        FavoritesManager.OnFavoriteClicked += OpenFavoriteDetailPanel;
    }

    // Unsubscribe from the event when this script is disabled
    void OnDisable()
    {
        FavoritesManager.OnFavoriteClicked -= OpenFavoriteDetailPanel;
    }

    // Method to open the detail panel for the selected favorite drink
    void OpenFavoriteDetailPanel(Drink drink)
    {
        // Implement your logic to open the detail panel here
        Debug.Log("Opening detail panel for: " + drink.strDrink);
    }
}

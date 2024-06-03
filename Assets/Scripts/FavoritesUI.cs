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
}

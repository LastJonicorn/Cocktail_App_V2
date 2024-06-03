using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AddFavorite : MonoBehaviour
{

    public Button addToFavoritesButton;

    private Drink currentDrink;

    // Start is called before the first frame update
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

using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.Collections;

public class FavoritesManager : MonoBehaviour
{
    public static FavoritesManager Instance;

    public GameObject favoriteItemPrefab;
    public Transform favoritesContainer;

    private List<Drink> favoriteDrinks = new List<Drink>();

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void AddToFavorites(Drink drink)
    {
        if (!favoriteDrinks.Contains(drink))
        {
            favoriteDrinks.Add(drink);
            StartCoroutine(DisplayFavorite(drink));
            Debug.Log("Added to favorites: " + drink.strDrink);
        }
        else
        {
            Debug.Log("Drink already in favorites: " + drink.strDrink);
        }
    }

    IEnumerator DisplayFavorite(Drink drink)
    {
        GameObject favoriteItem = Instantiate(favoriteItemPrefab, favoritesContainer);
        if (favoriteItem != null)
        {
            TextMeshProUGUI drinkNameText = favoriteItem.GetComponentInChildren<TextMeshProUGUI>();
            Image drinkImage = favoriteItem.GetComponentInChildren<Image>();

            if (drinkNameText != null && drinkImage != null)
            {
                drinkNameText.text = drink.strDrink;

                if (!string.IsNullOrEmpty(drink.strDrinkThumb))
                {
                    UnityWebRequest webRequest = UnityWebRequestTexture.GetTexture(drink.strDrinkThumb);
                    yield return webRequest.SendWebRequest();

                    if (webRequest.result == UnityWebRequest.Result.ConnectionError || webRequest.result == UnityWebRequest.Result.ProtocolError)
                    {
                        Debug.LogError("Error loading image: " + webRequest.error);
                    }
                    else
                    {
                        Texture2D texture = DownloadHandlerTexture.GetContent(webRequest);
                        if (texture != null)
                        {
                            drinkImage.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
                        }
                    }
                }
            }
            else
            {
                Debug.LogError("Favorite item prefab does not have the required TextMeshProUGUI and Image components.");
            }
        }
        else
        {
            Debug.LogError("Failed to instantiate favorite item prefab.");
        }
    }

    public void LoadFavorites()
    {
        foreach (Transform child in favoritesContainer)
        {
            Destroy(child.gameObject);
        }

        foreach (Drink drink in favoriteDrinks)
        {
            StartCoroutine(DisplayFavorite(drink));
        }
    }
}

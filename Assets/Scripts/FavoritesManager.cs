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
            LoadFavoritesFromPlayerPrefs();
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
            SaveFavoritesToPlayerPrefs();
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

            // Get all Image components in the prefab, including inactive ones
            Image[] images = favoriteItem.GetComponentsInChildren<Image>(true);

            // Find the specific Image component you need (e.g., by name or hierarchy level)
            Image drinkImage = null;
            foreach (Image img in images)
            {
                if (img.gameObject.name == "Favorite_Image") // Replace with the actual name of the child object
                {
                    drinkImage = img;
                    break;
                }
            }

            if (drinkNameText != null)
            {
                drinkNameText.text = drink.strDrink;
            }
            else
            {
                Debug.LogError("TextMeshProUGUI component not found in prefab.");
            }

            if (drinkImage != null)
            {
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
                            drinkImage.preserveAspect = true; // Preserve the aspect ratio of the image
                        }
                        else
                        {
                            Debug.LogError("Downloaded texture is null");
                        }
                    }
                }
            }
            else
            {
                Debug.LogError("Image component not found in child named 'SpecificChildObjectName'.");
            }

            // Force layout rebuild to ensure proper scroll view updating
            Canvas.ForceUpdateCanvases();
            LayoutRebuilder.ForceRebuildLayoutImmediate(favoritesContainer.GetComponent<RectTransform>());
        }
        else
        {
            Debug.LogError("Failed to instantiate favorite item prefab.");
        }
    }






    private void LoadFavoritesFromPlayerPrefs()
    {
        if (PlayerPrefs.HasKey("FavoriteDrinks"))
        {
            string json = PlayerPrefs.GetString("FavoriteDrinks");
            favoriteDrinks = JsonUtility.FromJson<FavoritesList>(json).drinks;
            LoadFavorites();
        }
    }

    private void SaveFavoritesToPlayerPrefs()
    {
        FavoritesList favoritesList = new FavoritesList { drinks = favoriteDrinks };
        string json = JsonUtility.ToJson(favoritesList);
        PlayerPrefs.SetString("FavoriteDrinks", json);
        PlayerPrefs.Save();
        Debug.Log("Data saved to PlayerPrefs");
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

    [System.Serializable]
    private class FavoritesList
    {
        public List<Drink> drinks;
    }
}

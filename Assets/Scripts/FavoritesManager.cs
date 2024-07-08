using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class FavoritesManager : MonoBehaviour
{
    public static FavoritesManager Instance;

    public GameObject favoriteItemPrefab;
    public Transform favoritesContainer;
    public GameObject favoriteDetailPanel; // Reference to the panel to open
    public GameObject favoriteScreen;

    public static event Action<Drink> OnFavoriteClicked;

    private HashSet<Drink> favoriteDrinks = new HashSet<Drink>();

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
        if (favoriteDrinks.Add(drink))
        {
            StartCoroutine(DisplayFavorite(drink));
            SaveFavoritesToPlayerPrefs();
            Debug.Log("Added to favorites: " + drink.strDrink);
        }
        else
        {
            Debug.Log("Drink already in favorites: " + drink.strDrink);
        }
    }

    public bool IsFavorite(string drinkName)
    {
        return favoriteDrinks.Any(drink => drink.strDrink == drinkName);
    }

    IEnumerator DisplayFavorite(Drink drink)
    {
        GameObject favoriteItem = Instantiate(favoriteItemPrefab, favoritesContainer);

        if (favoriteItem != null)
        {
            // Set up the text component
            TextMeshProUGUI drinkNameText = favoriteItem.GetComponentInChildren<TextMeshProUGUI>();
            if (drinkNameText != null)
            {
                drinkNameText.text = drink.strDrink;
            }
            else
            {
                Debug.LogError("TextMeshProUGUI component not found in prefab.");
            }

            // Set up the image component
            Image drinkImage = favoriteItem.GetComponentInChildren<Image>();
            if (drinkImage != null && !string.IsNullOrEmpty(drink.strDrinkThumb))
            {
                using (UnityWebRequest webRequest = UnityWebRequestTexture.GetTexture(drink.strDrinkThumb))
                {
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
                Debug.LogError("Image component not found or drink thumb URL is null or empty.");
            }

            // Set up the onClick method dynamically
            Button Detailbutton = favoriteItem.GetComponentInChildren<Button>();
            if (Detailbutton != null)
            {
                Detailbutton.onClick.AddListener(() => OnFavoriteItemClicked(drink));
            }
            else
            {
                Debug.LogError("Button component not found in prefab.");
            }

            // Force layout rebuild to ensure proper scroll view updating
            Canvas.ForceUpdateCanvases();

            // Find the button with the tag "RemoveFave" among the children
            Button[] buttons = favoriteItem.GetComponentsInChildren<Button>(true);
            Button removeButton = null;

            foreach (var button in buttons)
            {
                if (button.CompareTag("RemoveFave"))
                {
                    removeButton = button;
                    break;
                }
            }

            if (removeButton != null)
            {
                // Add onClick listener to the removeButton
                removeButton.onClick.AddListener(() => RemoveFromFavorites(drink, favoriteItem));
            }
            else
            {
                Debug.LogError("Button with tag RemoveFave not found in favorite item prefab.");
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
    public void RemoveFromFavorites(Drink drink, GameObject favoriteItem)
    {
        if (favoriteDrinks.Remove(drink))
        {
            SaveFavoritesToPlayerPrefs();
            Destroy(favoriteItem);
            Debug.Log("Removed from favorites: " + drink.strDrink);
        }
        else
        {
            Debug.LogError("Drink not found in favorites: " + drink.strDrink);
        }
    }

    void OnFavoriteItemClicked(Drink drink)
    {
        FavoriteClicked(drink);
    }

    private void LoadFavoritesFromPlayerPrefs()
    {
        if (PlayerPrefs.HasKey("FavoriteDrinks"))
        {
            string json = PlayerPrefs.GetString("FavoriteDrinks");
            favoriteDrinks = new HashSet<Drink>(JsonUtility.FromJson<FavoritesList>(json).drinks);
            LoadFavorites();
        }
    }

    private void SaveFavoritesToPlayerPrefs()
    {
        FavoritesList favoritesList = new FavoritesList { drinks = new List<Drink>(favoriteDrinks) };
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

    public void FavoriteClicked(Drink drink)
    {
        // Open the detail panel when a favorite is clicked
        if (favoriteDetailPanel != null)
        {
            favoriteDetailPanel.SetActive(true);
            favoriteScreen.SetActive(false);
            // Pass the selected drink to the detail panel script
            DetailPanelScript detailPanelScript = favoriteDetailPanel.GetComponent<DetailPanelScript>();
            if (detailPanelScript != null)
            {
                detailPanelScript.DisplayDrinkDetails(drink);
            }
            else
            {
                Debug.LogError("Detail panel script not found.");
            }
        }
        else
        {
            Debug.LogError("Favorite detail panel is not assigned.");
        }
    }

    [System.Serializable]
    private class FavoritesList
    {
        public List<Drink> drinks;
    }
}

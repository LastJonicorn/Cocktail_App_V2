using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.IO;
using System.Collections;
using System.Collections.Generic;

public class DisplaySavedDrinks : MonoBehaviour
{
    public GameObject drinkPrefab;
    public Transform contentTransform;

    // Use OnEnable instead of Start
    void OnEnable()
    {
        LoadAndDisplayDrinks();
    }

    public void LoadAndDisplayDrinks()
    {
        // Clear existing drinks if any
        ClearDrinkDisplay();

        List<OwnDrink> drinks = LoadOwnDrinks();

        if (drinks == null || drinks.Count == 0)
        {
            Debug.LogWarning("No drinks to display.");
            return;
        }

        foreach (OwnDrink drink in drinks)
        {
            GameObject drinkObject = Instantiate(drinkPrefab, contentTransform);

            TextMeshProUGUI drinkNameText = drinkObject.GetComponentInChildren<TextMeshProUGUI>();
            RawImage drinkImage = drinkObject.GetComponentInChildren<RawImage>();

            if (drinkNameText != null)
            {
                drinkNameText.text = drink.drinkName;
            }
            else
            {
                Debug.LogWarning("TextMeshProUGUI component not found in drink prefab.");
            }

            if (drinkImage != null && !string.IsNullOrEmpty(drink.picturePath))
            {
                Debug.Log("Loading picture for drink: " + drink.drinkName + " from path: " + drink.picturePath);
                StartCoroutine(LoadPicture(drink.picturePath, drinkImage));
            }
            else
            {
                Debug.LogWarning("RawImage component not found or picture path is empty for drink: " + drink.drinkName);
            }

            // Find the button with the tag "RemoveOwn" among the children
            Button[] buttons = drinkObject.GetComponentsInChildren<Button>(true);
            Button removeButton = null;

            foreach (var button in buttons)
            {
                if (button.CompareTag("RemoveOwn"))
                {
                    removeButton = button;
                    break;
                }
            }

            if (removeButton != null)
            {
                // Remove any existing listeners to prevent duplicates
                removeButton.onClick.RemoveAllListeners();

                // Add onClick listener to the removeButton
                removeButton.onClick.AddListener(() => DeleteDrink(drink.drinkName));
            }
            else
            {
                Debug.LogError("Button with tag RemoveOwn not found in drink prefab.");
            }
        }

        // Force layout rebuild to ensure proper scroll view updating
        Canvas.ForceUpdateCanvases();
        LayoutRebuilder.ForceRebuildLayoutImmediate(contentTransform.GetComponent<RectTransform>());
    }

    private IEnumerator LoadPicture(string path, RawImage targetImage)
    {
        string fullPath = Path.Combine(Application.persistentDataPath, path);

        if (File.Exists(fullPath))
        {
            byte[] fileData = File.ReadAllBytes(fullPath);
            Texture2D texture = new Texture2D(2, 2);
            texture.LoadImage(fileData);

            targetImage.texture = texture;

            float aspectRatio = (float)texture.width / texture.height;
            float targetWidth = 250;
            float targetHeight = targetWidth / aspectRatio;

            targetImage.rectTransform.sizeDelta = new Vector2(targetWidth, targetHeight);

            Debug.Log("Set RawImage size to: " + targetImage.rectTransform.sizeDelta);
        }
        else
        {
            Debug.LogError("Failed to load picture: File does not exist at path: " + fullPath);
        }

        yield return null;
    }

    public List<OwnDrink> LoadOwnDrinks()
    {
        List<OwnDrink> drinks = new List<OwnDrink>();

        if (PlayerPrefs.HasKey("Drinks"))
        {
            string json = PlayerPrefs.GetString("Drinks");
            OwnDrinkListWrapper wrapper = JsonUtility.FromJson<OwnDrinkListWrapper>(json);
            drinks = wrapper.drinks;
        }

        return drinks;
    }

    public void DeleteDrink(string drinkName)
    {
        List<OwnDrink> drinks = LoadOwnDrinks();

        OwnDrink drinkToRemove = drinks.Find(d => d.drinkName == drinkName);
        if (drinkToRemove != null)
        {
            drinks.Remove(drinkToRemove);
            SaveOwnDrinks(drinks);
            Debug.Log("Deleted drink: " + drinkName);

            // Refresh the display
            LoadAndDisplayDrinks();
        }
        else
        {
            Debug.LogWarning("Drink not found: " + drinkName);
        }
    }

    private void SaveOwnDrinks(List<OwnDrink> drinks)
    {
        OwnDrinkListWrapper wrapper = new OwnDrinkListWrapper { drinks = drinks };
        string json = JsonUtility.ToJson(wrapper);
        PlayerPrefs.SetString("Drinks", json);
        PlayerPrefs.Save();
    }

    private void ClearDrinkDisplay()
    {
        // Destroy all children of contentTransform
        foreach (Transform child in contentTransform)
        {
            Destroy(child.gameObject);
        }
    }
}

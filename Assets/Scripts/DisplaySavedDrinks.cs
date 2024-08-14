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
    public GameObject ownDetailPanel;
    public GameObject ownDrinks;
    private DetailPanelScript detailPanelScript;

    public GameObject confirmationPanel;  // Reference to the confirmation panel
    public Button confirmDeleteButton;    // Reference to the confirm button
    public Button cancelButton;           // Reference to the cancel button
    private string drinkToDelete;         // Store the name of the drink to delete

    void OnEnable()
    {
        LoadAndDisplayDrinks();
    }

    public void LoadAndDisplayDrinks()
    {
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
            RawImage drinkRawImage = drinkObject.GetComponentInChildren<RawImage>();
            Image drinkImage = drinkObject.GetComponentInChildren<Image>();

            if (drinkNameText != null)
            {
                drinkNameText.text = drink.drinkName;
            }
            else
            {
                Debug.LogWarning("TextMeshProUGUI component not found in drink prefab.");
            }

            if (!string.IsNullOrEmpty(drink.picturePath))
            {
                Debug.Log("Loading picture for drink: " + drink.drinkName + " from path: " + drink.picturePath);
                if (drinkRawImage != null)
                {
                    StartCoroutine(LoadPicture(drink.picturePath, targetRawImage: drinkRawImage));
                }
                else if (drinkImage != null)
                {
                    StartCoroutine(LoadPicture(drink.picturePath, targetImage: drinkImage));
                }
                else
                {
                    Debug.LogWarning("Neither RawImage nor Image component found for drink: " + drink.drinkName);
                }
            }
            else
            {
                Debug.LogWarning("Picture path is empty for drink: " + drink.drinkName);
            }

            Button[] buttons = drinkObject.GetComponentsInChildren<Button>(true);
            Button detailButton = null;
            Button removeButton = null;

            foreach (var button in buttons)
            {
                if (button.CompareTag("DetailOwn"))
                {
                    detailButton = button;
                    break;
                }
            }

            if (detailButton != null)
            {
                detailButton.onClick.RemoveAllListeners();
                detailButton.onClick.AddListener(() => OnOwnDrinkEntryClicked(drink));
            }
            else
            {
                Debug.LogError("Button with tag DetailOwn not found in drink prefab.");
            }

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
                removeButton.onClick.RemoveAllListeners();
                removeButton.onClick.AddListener(() => DeleteDrink(drink.drinkName));  // Show confirmation panel
            }
            else
            {
                Debug.LogError("Button with tag RemoveOwn not found in drink prefab.");
            }

        }

        Canvas.ForceUpdateCanvases();
        LayoutRebuilder.ForceRebuildLayoutImmediate(contentTransform.GetComponent<RectTransform>());
    }

    void OnOwnDrinkEntryClicked(OwnDrink drink)
    {
        Debug.Log("OwnDrink entry clicked: " + drink.drinkName + ", " + drink.ingredients.Count + " ingredients");

        if (ownDetailPanel != null)
        {
            ownDetailPanel.SetActive(true);

            if (detailPanelScript == null)
            {
                detailPanelScript = ownDetailPanel.GetComponent<DetailPanelScript>();
            }

            if (detailPanelScript != null)
            {
                detailPanelScript.DisplayOwnDrinkDetails(drink);
            }
            else
            {
                Debug.LogError("DetailPanelScript component not found on ownDetailPanel.");
            }
        }
        else
        {
            Debug.LogError("ownDetailPanel is not assigned.");
        }
        ownDrinks.SetActive(false);
    }
    private IEnumerator LoadPicture(string path, RawImage targetRawImage = null, Image targetImage = null)
    {
        string fullPath = Path.Combine(Application.persistentDataPath, path);

        Debug.Log("Attempting to load picture from path: " + fullPath);

        if (File.Exists(fullPath))
        {
            Debug.Log("File exists at path: " + fullPath);
            byte[] fileData = File.ReadAllBytes(fullPath);
            Texture2D texture = new Texture2D(2, 2, TextureFormat.RGB24, false);

            if (texture.LoadImage(fileData))
            {
                Debug.Log("Texture loaded successfully. Width: " + texture.width + ", Height: " + texture.height);

                if (targetRawImage != null)
                {
                    targetRawImage.texture = texture;
                    float aspectRatio = (float)texture.width / texture.height;
                    float targetWidth = 250f;
                    float targetHeight = targetWidth / aspectRatio;
                    targetRawImage.rectTransform.sizeDelta = new Vector2(targetWidth, targetHeight);

                    // Apply the rotation (90 degrees in this example)
                    targetRawImage.rectTransform.localEulerAngles = new Vector3(0, 0, -90); //This might break stuff!!!!!!!!!
                }
                else if (targetImage != null)
                {
                    Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
                    targetImage.sprite = sprite;
                    float aspectRatio = (float)texture.width / texture.height;
                    float targetWidth = 250f;
                    float targetHeight = targetWidth / aspectRatio;
                    targetImage.rectTransform.sizeDelta = new Vector2(targetWidth, targetHeight);

                    // Apply the rotation (90 degrees in this example)
                    targetImage.rectTransform.localEulerAngles = new Vector3(0, 0, -90); //This might break stuff!!!!!!!!!
                }
                else
                {
                    Debug.LogError("No target component specified for displaying the image.");
                }
            }
            else
            {
                Debug.LogError("Failed to load texture from file data.");
            }
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
        drinkToDelete = drinkName;  // Store the drink name for later deletion
        confirmationPanel.SetActive(true);  // Show the confirmation panel
    }

    public void ConfirmDelete()
    {
        List<OwnDrink> drinks = LoadOwnDrinks();
        OwnDrink drinkToRemove = drinks.Find(d => d.drinkName == drinkToDelete);

        if (drinkToRemove != null)
        {
            // Delete the associated image if it exists
            if (!string.IsNullOrEmpty(drinkToRemove.picturePath))
            {
                string fullPath = Path.Combine(Application.persistentDataPath, drinkToRemove.picturePath);
                if (File.Exists(fullPath))
                {
                    try
                    {
                        File.Delete(fullPath);
                        Debug.Log("Deleted image file at path: " + fullPath);
                    }
                    catch (IOException e)
                    {
                        Debug.LogError("Failed to delete image file: " + e.Message);
                    }
                }
                else
                {
                    Debug.LogWarning("Image file not found at path: " + fullPath);
                }
            }

            // Remove the drink from the list
            drinks.Remove(drinkToRemove);
            SaveOwnDrinks(drinks);
            Debug.Log("Deleted drink: " + drinkToDelete);
            LoadAndDisplayDrinks();
        }
        else
        {
            Debug.LogWarning("Drink not found: " + drinkToDelete);
        }

        confirmationPanel.SetActive(false);  // Hide the confirmation panel after deletion
    }


    public void CancelDelete()
    {
        confirmationPanel.SetActive(false);  // Hide the confirmation panel without deleting
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
        foreach (Transform child in contentTransform)
        {
            Destroy(child.gameObject);
        }
    }
}

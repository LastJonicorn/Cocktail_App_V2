using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.Networking;
using System.Collections.Generic;
using System.IO;

public class DetailPanelScript : MonoBehaviour
{
    public Image drinkImage;
    public TextMeshProUGUI nameLabel;
    public TextMeshProUGUI instructionsLabel;

    [Header("Ingredient and Measurement Pairs")]
    public GameObject[] ingredientPairs;

    // Reference to AddFavorite script
    public AddFavorite addFavoriteScript;

    // Display details for Drink object
    public void DisplayDrinkDetails(Drink drink)
    {
        // Update UI elements with drink details
        nameLabel.text = drink.strDrink;
        instructionsLabel.text = "Instructions: " + drink.strInstructions;

        // Load drink image
        StartCoroutine(LoadImage(drink.strDrinkThumb));

        // Update ingredients and measurements
        AssignText(ingredientPairs, drink);

        // Set current drink in AddFavorite script
        if (addFavoriteScript != null)
        {
            addFavoriteScript.SetCurrentDrink(drink);
        }

        // Activate the detail panel
        gameObject.SetActive(true);
    }
    private int CountIngredients(Drink drink)
    {
        int count = 0;
        for (int i = 1; i <= 15; i++)
        {
            string ingredient = GetIngredientText(drink, i);
            if (!string.IsNullOrEmpty(ingredient))
            {
                count++;
            }
            else
            {
                break; // No need to check further if ingredient is empty
            }
        }
        return count;
    }


    // Display details for OwnDrink object
    public void DisplayOwnDrinkDetails(OwnDrink ownDrink)
    {
        // Update UI elements with ownDrink details
        nameLabel.text = ownDrink.drinkName;
        instructionsLabel.text = ownDrink.instructions;

        // Load drink image
        StartCoroutine(LoadImageFromPath(ownDrink.picturePath));

        // Update ingredients and measurements
        AssignText(ingredientPairs, ownDrink);

        // Set current drink in AddFavorite script
        if (addFavoriteScript != null)
        {
            // Convert OwnDrink to Drink if needed
            Drink drink = ConvertOwnDrinkToDrink(ownDrink);
            addFavoriteScript.SetCurrentDrink(drink);
        }

        // Activate the detail panel
        gameObject.SetActive(true);
    }

    private void AssignText(GameObject[] ingredientPairs, Drink drink)
    {
        int ingredientCount = CountIngredients(drink);
        for (int i = 0; i < ingredientPairs.Length; i++)
        {
            TextMeshProUGUI[] texts = ingredientPairs[i].GetComponentsInChildren<TextMeshProUGUI>();
            if (texts.Length == 2)
            {
                texts[0].text = GetIngredientText(drink, i + 1);
                texts[1].text = GetMeasurementText(drink, i + 1);
                ingredientPairs[i].SetActive(i < ingredientCount); // Activate only if there are ingredients
            }
        }

        LayoutRebuilder.ForceRebuildLayoutImmediate(GetComponent<RectTransform>());
    }

    private void AssignText(GameObject[] ingredientPairs, OwnDrink ownDrink)
    {
        int ingredientCount = ownDrink.ingredients.Count;
        for (int i = 0; i < ingredientPairs.Length; i++)
        {
            TextMeshProUGUI[] texts = ingredientPairs[i].GetComponentsInChildren<TextMeshProUGUI>();
            if (texts.Length == 2)
            {
                texts[0].text = GetIngredientText(ownDrink, i + 1);
                texts[1].text = GetMeasurementText(ownDrink, i + 1);
                ingredientPairs[i].SetActive(i < ingredientCount); // Activate only if there are ingredients
            }
        }

        // Ensure that the first pair is visible if there's at least one ingredient
        if (ingredientCount > 0 && ingredientPairs.Length > 0)
        {
            ingredientPairs[0].SetActive(true);
        }

        LayoutRebuilder.ForceRebuildLayoutImmediate(GetComponent<RectTransform>());
    }


    private string GetIngredientText(Drink drink, int index)
    {
        switch (index)
        {
            case 1: return drink.strIngredient1;
            case 2: return drink.strIngredient2;
            case 3: return drink.strIngredient3;
            case 4: return drink.strIngredient4;
            case 5: return drink.strIngredient5;
            case 6: return drink.strIngredient6;
            case 7: return drink.strIngredient7;
            case 8: return drink.strIngredient8;
            case 9: return drink.strIngredient9;
            case 10: return drink.strIngredient10;
            case 11: return drink.strIngredient11;
            case 12: return drink.strIngredient12;
            case 13: return drink.strIngredient13;
            case 14: return drink.strIngredient14;
            case 15: return drink.strIngredient15;
            default: return "";
        }
    }

    private string GetMeasurementText(Drink drink, int index)
    {
        switch (index)
        {
            case 1: return drink.strMeasure1;
            case 2: return drink.strMeasure2;
            case 3: return drink.strMeasure3;
            case 4: return drink.strMeasure4;
            case 5: return drink.strMeasure5;
            case 6: return drink.strMeasure6;
            case 7: return drink.strMeasure7;
            case 8: return drink.strMeasure8;
            case 9: return drink.strMeasure9;
            case 10: return drink.strMeasure10;
            case 11: return drink.strMeasure11;
            case 12: return drink.strMeasure12;
            case 13: return drink.strMeasure13;
            case 14: return drink.strMeasure14;
            case 15: return drink.strMeasure15;
            default: return "";
        }
    }

    private string GetIngredientText(OwnDrink ownDrink, int index)
    {
        if (index <= ownDrink.ingredients.Count)
        {
            return ownDrink.ingredients[index - 1].name;
        }
        else
        {
            return "";
        }
    }

    private string GetMeasurementText(OwnDrink ownDrink, int index)
    {
        if (index <= ownDrink.ingredients.Count)
        {
            return ownDrink.ingredients[index - 1].measurement;
        }
        else
        {
            return "";
        }
    }

    private IEnumerator LoadImage(string imageUrl)
    {
        using (UnityWebRequest webRequest = UnityWebRequestTexture.GetTexture(imageUrl))
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
                    drinkImage.preserveAspect = true;
                }
                else
                {
                    Debug.LogError("Downloaded texture is null");
                }
            }
        }
    }

    private IEnumerator LoadImageFromPath(string path)
    {
        string fullPath = Path.Combine(Application.persistentDataPath, path);

        if (File.Exists(fullPath))
        {
            byte[] fileData = File.ReadAllBytes(fullPath);
            Texture2D texture = new Texture2D(2, 2);
            texture.LoadImage(fileData);

            // Create a sprite from the loaded texture
            Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));

            // Apply the sprite to the Image component
            drinkImage.sprite = sprite;
            drinkImage.preserveAspect = true;

            // Apply rotation to the Image component's RectTransform
            // For a 90-degree rotation, set localEulerAngles to new Vector3(0, 0, 90)
            drinkImage.rectTransform.localEulerAngles = new Vector3(0, 0, -90); //This might break stuff!!!!!!!!!
        }
        else
        {
            Debug.LogError("Failed to load picture: File does not exist at path: " + fullPath);
        }

        yield return null;
    }


    private Drink ConvertOwnDrinkToDrink(OwnDrink ownDrink)
    {
        // Implement your conversion logic here
        // This is just a placeholder assuming Drink has similar properties
        Drink drink = new Drink
        {
            strDrink = ownDrink.drinkName,
            strInstructions = ownDrink.instructions,
            strDrinkThumb = ownDrink.picturePath // You might need to adjust this
            // Add more properties as needed
        };

        return drink;
    }
}

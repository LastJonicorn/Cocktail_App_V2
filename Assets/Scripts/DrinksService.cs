using UnityEngine;
using UnityEngine.Networking;
using TMPro;
using System.Collections;
using UnityEngine.UI;
using System.IO;

public class APIRequest : MonoBehaviour
{
    private string apiUrl;

    public TextMeshProUGUI responseText;
    public TextMeshProUGUI instructionsText;
    public Image responseImage;
    public Button refreshButton;
    public ScrollRect scrollRect;

    [Header("Ingredient and Measurement Pairs")]
    public GameObject[] ingredientPairs;

    private Drink currentDrink;
    public AddFavorite addToFavoritesScript;

    void Start()
    {
        LoadConfig();
        StartCoroutine(GetRequest(apiUrl));
        refreshButton.onClick.AddListener(RefreshAPI);
    }

    void LoadConfig()
    {
        string configPath = Path.Combine(Application.streamingAssetsPath, "Config.txt");
        if (File.Exists(configPath))
        {
            try
            {
                string configJson = File.ReadAllText(configPath);
                Config config = JsonUtility.FromJson<Config>(configJson);
                apiUrl = config.apiUrl;
            }
            catch (System.Exception e)
            {
                Debug.LogError("Error reading config file: " + e.Message);
                apiUrl = "https://default.api.url";
            }
        }
        else
        {
            Debug.LogError("Config file not found. Using default API URL.");
            apiUrl = "https://default.api.url";
        }
    }

    IEnumerator GetRequest(string url)
    {
        using (UnityWebRequest webRequest = UnityWebRequest.Get(url))
        {
            yield return webRequest.SendWebRequest();

            if (webRequest.result == UnityWebRequest.Result.ConnectionError || webRequest.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError("Error: " + webRequest.error);
                responseText.text = "Error: " + webRequest.error;
            }
            else
            {
                HandleResponse(webRequest.downloadHandler.text);
            }
        }
    }

    void HandleResponse(string responseTextContent)
    {
        Debug.Log("Response: " + responseTextContent);

        try
        {
            DrinkResponse drinkResponse = JsonUtility.FromJson<DrinkResponse>(responseTextContent);
            if (drinkResponse != null && drinkResponse.drinks != null && drinkResponse.drinks.Length > 0)
            {
                currentDrink = drinkResponse.drinks[0];
                UpdateDrinkUI(currentDrink);

                // Set current drink in the AddToFavorites script
                if (addToFavoritesScript != null)
                {
                    addToFavoritesScript.SetCurrentDrink(currentDrink);
                }
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError("Error parsing response: " + e.Message);
        }
    }

    void UpdateDrinkUI(Drink drink)
    {
        responseText.text = drink.strDrink ?? "Unknown Drink";
        instructionsText.text = drink.strInstructions ?? "No instructions available";

        AssignText(ingredientPairs, new string[]
        {
            drink.strIngredient1, drink.strIngredient2, drink.strIngredient3, drink.strIngredient4,
            drink.strIngredient5, drink.strIngredient6, drink.strIngredient7, drink.strIngredient8,
            drink.strIngredient9, drink.strIngredient10, drink.strIngredient11, drink.strIngredient12,
            drink.strIngredient13, drink.strIngredient14, drink.strIngredient15
        }, new string[]
        {
            drink.strMeasure1, drink.strMeasure2, drink.strMeasure3, drink.strMeasure4,
            drink.strMeasure5, drink.strMeasure6, drink.strMeasure7, drink.strMeasure8,
            drink.strMeasure9, drink.strMeasure10, drink.strMeasure11, drink.strMeasure12,
            drink.strMeasure13, drink.strMeasure14, drink.strMeasure15
        });

        if (!string.IsNullOrEmpty(drink.strDrinkThumb))
        {
            StartCoroutine(LoadImage(drink.strDrinkThumb));
        }
    }

    void AssignText(GameObject[] ingredientPairs, string[] ingredients, string[] measurements)
    {
        for (int i = 0; i < ingredientPairs.Length; i++)
        {
            bool hasIngredient = !string.IsNullOrEmpty(ingredients[i]);
            bool hasMeasurement = !string.IsNullOrEmpty(measurements[i]);

            ingredientPairs[i].SetActive(hasIngredient || hasMeasurement);

            if (hasIngredient || hasMeasurement)
            {
                TextMeshProUGUI[] texts = ingredientPairs[i].GetComponentsInChildren<TextMeshProUGUI>();
                if (texts.Length == 2)
                {
                    texts[0].text = hasIngredient ? ingredients[i] : "";
                    texts[1].text = hasMeasurement ? measurements[i] : "";
                }
            }
        }

        LayoutRebuilder.ForceRebuildLayoutImmediate(GetComponent<RectTransform>());
    }

    IEnumerator LoadImage(string imageUrl)
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
                    responseImage.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
                }
            }
        }
    }

    public void RefreshAPI()
    {
        responseText.text = "";
        instructionsText.text = "";
        responseImage.sprite = null;
        foreach (var pair in ingredientPairs)
        {
            pair.SetActive(false);
        }

        // Scroll to top
        if (scrollRect != null)
        {
            scrollRect.verticalNormalizedPosition = 1f;
        }

        StartCoroutine(GetRequest(apiUrl));
    }
}

[System.Serializable]
public class Config
{
    public string apiUrl;
}

[System.Serializable]
public class DrinkResponse
{
    public Drink[] drinks;
}

[System.Serializable]
public class Drink
{
    public string idDrink;
    public string strDrink;
    public string strCategory;
    public string strAlcoholic;
    public string strGlass;
    public string strInstructions;
    public string strDrinkThumb;

    public string strIngredient1;
    public string strIngredient2;
    public string strIngredient3;
    public string strIngredient4;
    public string strIngredient5;
    public string strIngredient6;
    public string strIngredient7;
    public string strIngredient8;
    public string strIngredient9;
    public string strIngredient10;
    public string strIngredient11;
    public string strIngredient12;
    public string strIngredient13;
    public string strIngredient14;
    public string strIngredient15;

    public string strMeasure1;
    public string strMeasure2;
    public string strMeasure3;
    public string strMeasure4;
    public string strMeasure5;
    public string strMeasure6;
    public string strMeasure7;
    public string strMeasure8;
    public string strMeasure9;
    public string strMeasure10;
    public string strMeasure11;
    public string strMeasure12;
    public string strMeasure13;
    public string strMeasure14;
    public string strMeasure15;
}

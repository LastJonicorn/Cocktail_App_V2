using UnityEngine;
using UnityEngine.Networking;
using TMPro;
using System.Collections;
using UnityEngine.UI;
using System.IO;

public class DrinkSearch : MonoBehaviour
{
    private string apiUrl;
    public TMP_InputField searchInput; // Reference to the text input field

    public Transform contentPanel; // Parent panel to hold the drink entries
    public GameObject drinkEntryPrefab; // Prefab for individual drink entries

    void Start()
    {
        LoadConfig();
        searchInput.onValueChanged.AddListener(delegate { SearchForDrinks(searchInput.text); });
        SearchForDrinks("");
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
                apiUrl = config.apiUrl + "/search.php?s=";
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
            }
            else
            {
                HandleResponse(webRequest.downloadHandler.text);
            }
        }
    }

    void HandleResponse(string responseTextContent)
    {
        try
        {
            DrinkResponse drinkResponse = JsonUtility.FromJson<DrinkResponse>(responseTextContent);
            if (drinkResponse != null && drinkResponse.drinks != null)
            {
                ClearDrinks(); // Clear existing drink entries

                foreach (Drink drink in drinkResponse.drinks)
                {
                    GameObject drinkEntry = Instantiate(drinkEntryPrefab, contentPanel);
                    drinkEntry.transform.SetParent(contentPanel, false);

                    TextMeshProUGUI text = drinkEntry.GetComponentInChildren<TextMeshProUGUI>();
                    if (text == null)
                    {
                        text = drinkEntry.AddComponent<TextMeshProUGUI>();
                    }
                    text.text = drink.strDrink;

                    Image image = drinkEntry.GetComponentInChildren<Image>();
                    if (image == null)
                    {
                        image = drinkEntry.AddComponent<Image>();
                    }
                    if (!string.IsNullOrEmpty(drink.strDrinkThumb))
                    {
                        StartCoroutine(LoadImage(drink.strDrinkThumb, image));
                    }
                }
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError("Error parsing response: " + e.Message);
        }
    }

    void ClearDrinks()
    {
        foreach (Transform child in contentPanel)
        {
            Destroy(child.gameObject);
        }
    }

    IEnumerator LoadImage(string imageUrl, Image image)
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
                if (texture != null && image != null) // Check if image is not null
                {
                    Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
                    image.sprite = sprite;
                }
            }
        }
    }


    public void SearchForDrinks(string searchTerm)
    {
        string searchUrl = apiUrl + searchTerm;
        StartCoroutine(GetRequest(searchUrl));
    }
}

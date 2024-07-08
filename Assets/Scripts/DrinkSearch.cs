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
    public Button searchByNameButton; // Reference to the search by name button
    public Button searchByIngredientButton; // Reference to the search by ingredient button

    public Transform contentPanel; // Parent panel to hold the drink entries
    public GameObject drinkEntryPrefab; // Prefab for individual drink entries

    public GameObject searchDetailPanel; // Reference to the panel to open
    public GameObject searchScreen;
    public DetailPanelScript detailPanelScript; // Reference to DetailPanelScript attached to searchDetailPanel

    public GameObject loadingPanel; // Reference to the loading panel

    void Start()
    {
        StartCoroutine(LoadConfigAndInitialize());
    }

    IEnumerator LoadConfigAndInitialize()
    {
        yield return LoadConfig();

        searchInput.onValueChanged.AddListener(delegate { SearchForDrinks(searchInput.text); });
        searchByNameButton.onClick.AddListener(SearchByName);
        searchByIngredientButton.onClick.AddListener(SearchByIngredient);

        // Perform an initial search with an empty string to display all drinks
        SearchForDrinks("");
    }

    IEnumerator LoadConfig()
    {
        string configPath = Path.Combine(Application.streamingAssetsPath, "Config.txt");

        if (Application.platform == RuntimePlatform.Android)
        {
            UnityWebRequest request = UnityWebRequest.Get(configPath);
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError("Error reading config file: " + request.error);
                apiUrl = "https://default.api.url/search.php?";
            }
            else
            {
                try
                {
                    string configJson = request.downloadHandler.text;
                    apiUrl = JsonUtility.FromJson<Config>(configJson).apiUrl + "/search.php?";
                }
                catch (System.Exception e)
                {
                    Debug.LogError("Error parsing config file: " + e.Message);
                    apiUrl = "https://default.api.url/search.php?";
                }
            }
        }
        else
        {
            if (File.Exists(configPath))
            {
                try
                {
                    string configJson = File.ReadAllText(configPath);
                    apiUrl = JsonUtility.FromJson<Config>(configJson).apiUrl + "/search.php?";
                }
                catch (System.Exception e)
                {
                    Debug.LogError("Error reading config file: " + e.Message);
                    apiUrl = "https://default.api.url/search.php?";
                }
            }
            else
            {
                Debug.LogError("Config file not found. Using default API URL.");
                apiUrl = "https://default.api.url/search.php?";
            }
        }
    }

    IEnumerator GetRequest(string url)
    {
        ShowLoadingIndicator(true);

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

        ShowLoadingIndicator(false);
    }

    void HandleResponse(string responseTextContent)
    {
        try
        {
            DrinkResponse drinkResponse = JsonUtility.FromJson<DrinkResponse>(responseTextContent);
            if (drinkResponse != null && drinkResponse.drinks != null)
            {
                ClearDrinks(); // Clear existing drink entries

                int count = 0;
                foreach (Drink drink in drinkResponse.drinks)
                {
                    if (count >= 20) break; // Limit to 20 drinks

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
                        // Modify the image URL to use the 100x100 version
                        string lowResImageUrl = drink.strDrinkThumb.Replace("/preview", "/100x100");
                        StartCoroutine(LoadImage(lowResImageUrl, image));
                    }

                    // Add click handler to open detail panel
                    Button button = drinkEntry.GetComponentInChildren<Button>();
                    if (button != null)
                    {
                        button.onClick.AddListener(() => OnDrinkEntryClicked(drink));
                    }
                    else
                    {
                        Debug.LogError("Button component not found on drink entry prefab.");
                    }

                    count++;
                }
            }
            else
            {
                Debug.LogWarning("No drinks found in the response.");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError("Error handling response: " + e.Message);
        }
    }

    void OnDrinkEntryClicked(Drink drink)
    {
        Debug.Log("Drink entry clicked: " + drink.strDrink);

        if (searchDetailPanel != null)
        {
            searchDetailPanel.SetActive(true);
            searchScreen.SetActive(false);
            DetailPanelScript detailPanelScript = searchDetailPanel.GetComponent<DetailPanelScript>();
            if (detailPanelScript != null)
            {
                detailPanelScript.DisplayDrinkDetails(drink);
            }
            else
            {
                Debug.LogError("DetailPanelScript component not found on searchDetailPanel.");
            }
        }
        else
        {
            Debug.LogError("searchDetailPanel is not assigned.");
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
                if (texture != null && image != null)
                {
                    Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
                    image.sprite = sprite;
                }
            }
        }
    }

    public void SearchForDrinks(string searchTerm)
    {
        string searchUrl = string.IsNullOrEmpty(searchTerm) ? apiUrl + "s=" : apiUrl + "s=" + searchTerm;
        StartCoroutine(GetRequest(searchUrl));
    }

    public void SearchByIngredient()
    {
        string searchTerm = searchInput.text; // Use the existing search input field
        string searchUrl = apiUrl + "i=" + searchTerm;
        StartCoroutine(GetRequest(searchUrl));

        // Clear the search input field
        searchInput.text = "";
    }

    public void SearchByName()
    {
        string searchTerm = searchInput.text; // Use the existing search input field
        string searchUrl = apiUrl + "s=" + searchTerm;
        StartCoroutine(GetRequest(searchUrl));

        // Clear the search input field
        searchInput.text = "";
    }

    void ShowLoadingIndicator(bool show)
    {
        if (loadingPanel != null)
        {
            loadingPanel.SetActive(show);
        }
        else
        {
            Debug.LogError("Loading panel is not assigned.");
        }
    }
}

using UnityEngine;
using UnityEngine.Networking;
using TMPro;
using System.Collections;
using UnityEngine.UI;
using System.IO;

public class DrinkSearch : MonoBehaviour
{
    private string apiUrl;
    private int loadedDrinksCount = 0;
    private int drinksPerPage = 20; // Initial load
    private int additionalDrinksCount = 10; // Drinks to load on "Load More"

    public TMP_InputField searchInput;
    public Transform contentPanel;
    public GameObject drinkEntryPrefab;

    public string searchType;
    public string urlAddition;

    public GameObject searchDetailPanel;
    public GameObject searchScreen;
    public DetailPanelScript detailPanelScript;

    public GameObject loadingPanel;
    public GameObject loadMoreButtonPrefab; // Reference to the "Load More" button prefab

    private GameObject loadMoreButtonInstance; // Instance of the "Load More" button

    void Start()
    {
        StartCoroutine(LoadConfigAndInitialize());
    }

    IEnumerator LoadConfigAndInitialize()
    {
        yield return LoadConfig();

        searchInput.onValueChanged.AddListener(delegate { SearchForDrinks(searchInput.text); });

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
                    apiUrl = JsonUtility.FromJson<Config>(configJson).apiUrl + urlAddition;
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
                    apiUrl = JsonUtility.FromJson<Config>(configJson).apiUrl + urlAddition;
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
                int count = 0;
                foreach (Drink drink in drinkResponse.drinks)
                {
                    if (count >= drinksPerPage + loadedDrinksCount) break; // Limit the number of drinks to load

                    if (count >= loadedDrinksCount) // Start loading from the current loaded count
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
                    }

                    count++;
                }

                loadedDrinksCount += count;

                // Show or hide the Load More button based on remaining drinks
                if (drinkResponse.drinks.Length > loadedDrinksCount)
                {
                    ShowLoadMoreButton(true);
                }
                else
                {
                    ShowLoadMoreButton(false);
                }
            }
            else
            {
                Debug.LogWarning("No drinks found in the response.");
                ShowLoadMoreButton(false);
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError("Error handling response: " + e.Message);
            ShowLoadMoreButton(false);
        }
    }

    IEnumerator SearchDrinkById(string idDrink)
    {
        string url = $"https://www.thecocktaildb.com/api/json/v1/1/lookup.php?i={idDrink}";
        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError($"Error fetching drink details: {request.error}");
            }
            else
            {
                string jsonResponse = request.downloadHandler.text;
                DrinkResponse drinkResponse = JsonUtility.FromJson<DrinkResponse>(jsonResponse);

                if (drinkResponse.drinks != null && drinkResponse.drinks.Length > 0)
                {
                    Drink retrievedDrink = drinkResponse.drinks[0];
                    DisplayRetrievedDrinkDetails(retrievedDrink);
                }
                else
                {
                    Debug.LogError("No drinks found in the response.");
                }
            }
        }
    }

    void DisplayRetrievedDrinkDetails(Drink drink)
    {
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

    void OnDrinkEntryClicked(Drink drink)
    {
        Debug.Log("Drink entry clicked: " + drink.strDrink + drink.strIngredient1 + drink.strMeasure1 + drink.strInstructions + drink.idDrink);

        if (urlAddition == "filter.php?")
        {
            // Perform search by drink ID
            StartCoroutine(SearchDrinkById(drink.idDrink));
        }
        else if (searchDetailPanel != null)
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
        loadedDrinksCount = 0; // Reset loaded count
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
        ClearDrinks();
        string searchUrl = string.IsNullOrEmpty(searchTerm) ? apiUrl + searchType : apiUrl + searchType + searchTerm;
        StartCoroutine(GetRequest(searchUrl));
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

    void ShowLoadMoreButton(bool show)
    {
        if (show)
        {
            if (loadMoreButtonInstance == null)
            {
                loadMoreButtonInstance = Instantiate(loadMoreButtonPrefab, contentPanel);
                loadMoreButtonInstance.transform.SetParent(contentPanel, false);

                Button loadMoreButton = loadMoreButtonInstance.GetComponentInChildren<Button>();
                if (loadMoreButton != null)
                {
                    loadMoreButton.onClick.AddListener(() => LoadMoreDrinks());
                }
                else
                {
                    Debug.LogError("Button component not found on Load More button prefab.");
                }
            }

            // Ensure the button is always at the bottom
            loadMoreButtonInstance.transform.SetAsLastSibling();
        }
        else if (loadMoreButtonInstance != null)
        {
            Destroy(loadMoreButtonInstance);
        }
    }

    public void LoadMoreDrinks()
    {
        string searchUrl = apiUrl + searchType + searchInput.text;
        StartCoroutine(GetRequest(searchUrl));
    }
}

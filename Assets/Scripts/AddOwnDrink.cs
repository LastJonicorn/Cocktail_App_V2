using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Android;
using System.IO;

[System.Serializable]
public class OwnDrink
{
    public string drinkName;
    public string picturePath;
    public List<Ingredient> ingredients = new List<Ingredient>();
    public string instructions;
}

[System.Serializable]
public class Ingredient
{
    public string name;
    public string measurement;
}

public class AddOwnDrink : MonoBehaviour
{
    private const string DrinksKey = "Drinks";
    public TMP_InputField drinkNameInput;
    public TMP_InputField instructionsInput;
    public Button takePictureButton;
    public Button addIngredientButton;
    public Button saveButton;
    public TextMeshProUGUI feedbackText;
    public GameObject feedbackTextContainer; // The GameObject that contains the feedback text
    public RawImage rawImage;
    public CameraController cameraController;
    public VerticalLayoutGroup layoutGroup;
    public List<GameObject> ingredientMeasurementPairs;

    //public Image drinkImage;
    private bool isPictureSaved = false;

    public Sprite defaultSprite;
    public Button retakePictureButton;

    private string picturePath;
    private List<Ingredient> ingredients = new List<Ingredient>();
    private int activePairsCount = 0;

    void Start()
    {
        takePictureButton.onClick.AddListener(RequestPermissionsAndTakePicture);
        addIngredientButton.onClick.AddListener(ActivateNextIngredientMeasurementPair);
        saveButton.onClick.AddListener(SaveOwnDrink);

        RequestPermissions();
        feedbackTextContainer.SetActive(false); // Initially deactivate the feedback text container
    }


    private void OnEnable()
    {
        feedbackText.text = "";
        takePictureButton.gameObject.SetActive(true);
        retakePictureButton.gameObject.SetActive(false);

        if (!string.IsNullOrEmpty(picturePath))
        {
            LoadPicture(picturePath);
        }

        if (cameraController != null)
        {
            cameraController.ReinitializeCamera();
        }
        ClearFeedbackTextAfterDelay(0);

        RefreshForm();
    }

    private void OnDisable()
    {
        if (cameraController != null)
        {
            cameraController.StopCamera();
        }

        if (!isPictureSaved && !string.IsNullOrEmpty(picturePath))
        {
            DeletePicture(picturePath);
        }
    }


    private void RequestPermissionsAndTakePicture()
    {
        if (RequestPermissions())
        {
            TakePicture();
        }
    }

    private bool RequestPermissions()
    {
        bool allPermissionsGranted = true;

        if (Application.platform == RuntimePlatform.Android)
        {
            if (!Permission.HasUserAuthorizedPermission(Permission.Camera))
            {
                Permission.RequestUserPermission(Permission.Camera);
                allPermissionsGranted = false;
            }

            if (!Permission.HasUserAuthorizedPermission(Permission.ExternalStorageWrite))
            {
                Permission.RequestUserPermission(Permission.ExternalStorageWrite);
                allPermissionsGranted = false;
            }

            if (!Permission.HasUserAuthorizedPermission(Permission.ExternalStorageRead))
            {
                Permission.RequestUserPermission(Permission.ExternalStorageRead);
                allPermissionsGranted = false;
            }
        }

        return allPermissionsGranted;
    }

    public void TakePicture()
    {
        if (cameraController != null)
        {
            //picturePath = "";
            rawImage.texture = defaultSprite.texture;
            rawImage.rectTransform.sizeDelta = new Vector2(500, 500);

            cameraController.TakePicture();
            StartCoroutine(UpdatePicturePath());
        }
        else
        {
            Debug.LogError("CameraController reference not set in AddOwnDrink script.");
        }
    }
    public void RetakePicture()
    {
        cameraController.ClearPicturePath(); //This is new if something breaks
        picturePath = "";

        cameraController.ReinitializeCamera();

        if (rawImage != null)
        {
            rawImage.texture = null;
            rawImage.rectTransform.sizeDelta = new Vector2(500, 500);
        }

        LayoutRebuilder.ForceRebuildLayoutImmediate(layoutGroup.GetComponent<RectTransform>());
    }

    private IEnumerator UpdatePicturePath()
    {
        string path = cameraController.GetPicturePath();

        while (string.IsNullOrEmpty(path))
        {
            yield return new WaitForSeconds(0.5f);
            path = cameraController.GetPicturePath();
        }

        picturePath = path;

        Debug.Log($"Picture path updated: {picturePath}");

        // Load the picture on the main thread
        UnityMainThreadDispatcher.Instance().Enqueue(() =>
        {
            picturePath = cameraController.GetPicturePath();
            //LoadAndDisplayPicture(picturePath);
        });
    }

    private void LoadPicture(string path)
    {
        string fullPath = Path.Combine(Application.persistentDataPath, path);
        Debug.Log($"Attempting to load picture from: {fullPath}");

        if (File.Exists(fullPath))
        {
            try
            {
                byte[] fileData = File.ReadAllBytes(fullPath);
                Texture2D texture = new Texture2D(2, 2);
                if (texture.LoadImage(fileData))
                {
                    Debug.Log($"Texture loaded successfully. Size: {texture.width}x{texture.height}");
                    if (rawImage != null)
                    {
                        rawImage.texture = texture;
                        rawImage.rectTransform.sizeDelta = new Vector2(500, 500);
                        LayoutRebuilder.ForceRebuildLayoutImmediate(rawImage.rectTransform);
                        Debug.Log("RawImage texture set and layout rebuilt");
                    }
                    else
                    {
                        Debug.LogError("RawImage is null when trying to load picture.");
                    }
                }
                else
                {
                    Debug.LogError("Failed to load image data into texture.");
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Error loading picture: {ex.Message}\n{ex.StackTrace}");
            }
        }
        else
        {
            Debug.LogError($"Failed to load picture: File does not exist at path: {fullPath}");
            SetFeedbackText("No image found to load"); //Delete this later
        }

        UnityMainThreadDispatcher.Instance().Enqueue(() =>
        {
            ForceUIRefresh();
        });
    }


    public void ActivateNextIngredientMeasurementPair()
    {
        Debug.Log($"ActivateNextIngredientMeasurementPair called. Current active pairs count: {activePairsCount}");

        if (activePairsCount < ingredientMeasurementPairs.Count)
        {
            ingredientMeasurementPairs[activePairsCount].SetActive(true);
            activePairsCount++;
            //Debug.Log($"Activated ingredient pair {activePairsCount - 1}");
        }
        else
        {
            Debug.LogWarning("No more ingredient pairs to activate.");
        }
    }

    public void SaveOwnDrink()
    {
        Debug.Log("SaveOwnDrink called");

        string drinkName = drinkNameInput.text;
        if (string.IsNullOrEmpty(drinkName))
        {
            SetFeedbackText("Drink name cannot be empty.");
            return;
        }

        string instructions = instructionsInput.text;

        ingredients.Clear();
        for (int i = 0; i < activePairsCount; i++)
        {
            GameObject pair = ingredientMeasurementPairs[i];
            TMP_InputField ingredientInput = pair.transform.Find("IngredientInput").GetComponent<TMP_InputField>();
            TMP_InputField measurementInput = pair.transform.Find("MeasurementInput").GetComponent<TMP_InputField>();

            string ingredientName = ingredientInput.text;
            string measurement = measurementInput.text;

            if (string.IsNullOrEmpty(ingredientName))
            {
                SetFeedbackText("Ingredient name cannot be empty.");
                return;
            }

            if (string.IsNullOrEmpty(measurement))
            {
                SetFeedbackText("Measurement cannot be empty.");
                return;
            }

            ingredients.Add(new Ingredient { name = ingredientName, measurement = measurement });
        }

        if (string.IsNullOrEmpty(instructions))
        {
            SetFeedbackText("Instructions cannot be empty.");
            return;
        }

        // Ensure that the picturePath is correctly set
        picturePath = cameraController.GetPicturePath();

        Debug.Log("Current picturePath is: " + picturePath);

        if (string.IsNullOrEmpty(picturePath))
        {
            // Assign the default sprite's path if no picture was taken
            picturePath = SaveDefaultSprite();
            Debug.Log("Default sprite path assigned: " + picturePath);
        }

        OwnDrink ownDrink = new OwnDrink
        {
            drinkName = drinkName,
            picturePath = picturePath,
            ingredients = ingredients,
            instructions = instructions
        };

        List<OwnDrink> drinks = LoadOwnDrinks();
        drinks.Add(ownDrink);

        string json = JsonUtility.ToJson(new OwnDrinkListWrapper { drinks = drinks });
        PlayerPrefs.SetString(DrinksKey, json);
        PlayerPrefs.Save();

        SetFeedbackText("Drink " + drinkName + " saved successfully");

        isPictureSaved = true;  // Mark the picture as saved

        RefreshForm();
    }



    private void DeletePicture(string path)
    {
        string fullPath = Path.Combine(Application.persistentDataPath, path);
        if (File.Exists(fullPath))
        {
            try
            {
                File.Delete(fullPath);
                Debug.Log($"Picture deleted: {fullPath}");
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Failed to delete picture: {ex.Message}");
            }
        }
        else
        {
            Debug.LogWarning($"No file found to delete at path: {fullPath}");
        }
    }


    private string SaveDefaultSprite()
    {
        string path = Path.Combine(Application.persistentDataPath, "defaultSprite.png");
        if (defaultSprite != null)
        {
            Texture2D texture = SpriteToTexture(defaultSprite);
            File.WriteAllBytes(path, texture.EncodeToPNG());
        }
        return "defaultSprite.png";
    }

    private Texture2D SpriteToTexture(Sprite sprite)
    {
        Texture2D texture = new Texture2D((int)sprite.rect.width, (int)sprite.rect.height);
        texture.SetPixels(sprite.texture.GetPixels((int)sprite.rect.x, (int)sprite.rect.y, (int)sprite.rect.width, (int)sprite.rect.height));
        texture.Apply();
        return texture;
    }

    private void RefreshForm()
    {
        drinkNameInput.text = "";
        instructionsInput.text = "";

        foreach (GameObject pair in ingredientMeasurementPairs)
        {
            pair.SetActive(false);
        }

        LayoutRebuilder.ForceRebuildLayoutImmediate(layoutGroup.GetComponent<RectTransform>());

        if (ingredientMeasurementPairs.Count > 0)
        {
            GameObject firstPair = ingredientMeasurementPairs[0];
            firstPair.SetActive(true);
            activePairsCount = 1;

            TMP_InputField ingredientInput = firstPair.transform.Find("IngredientInput").GetComponent<TMP_InputField>();
            TMP_InputField measurementInput = firstPair.transform.Find("MeasurementInput").GetComponent<TMP_InputField>();
            ingredientInput.text = "";
            measurementInput.text = "";
        }
        else
        {
            activePairsCount = 0;
        }

        cameraController.ClearPicturePath(); //This is new if something breaks
        picturePath = "";

        if (rawImage != null)
        {
            rawImage.texture = null;
            rawImage.rectTransform.sizeDelta = new Vector2(500, 500);
        }

        isPictureSaved = false;

        cameraController.ReinitializeCamera();

        LayoutRebuilder.ForceRebuildLayoutImmediate(layoutGroup.GetComponent<RectTransform>());
    }

    public List<OwnDrink> LoadOwnDrinks()
    {
        if (PlayerPrefs.HasKey(DrinksKey))
        {
            string json = PlayerPrefs.GetString(DrinksKey);
            OwnDrinkListWrapper wrapper = JsonUtility.FromJson<OwnDrinkListWrapper>(json);
            return wrapper.drinks;
        }
        return new List<OwnDrink>();
    }

    private void SetFeedbackText(string text)
    {
        feedbackText.text = text;
        feedbackTextContainer.SetActive(true);  // Activate the GameObject
        ClearFeedbackTextAfterDelay();
    }

    private void ClearFeedbackTextAfterDelay(float delay = 3.0f)
    {
        StartCoroutine(ClearFeedbackTextCoroutine(delay));
    }

    private IEnumerator ClearFeedbackTextCoroutine(float delay)
    {
        yield return new WaitForSeconds(delay);
        feedbackText.text = "";
        feedbackTextContainer.SetActive(false);  // Deactivate the GameObject
    }

    private void ForceUIRefresh()
    {
        if (rawImage != null)
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(rawImage.rectTransform);
            Canvas.ForceUpdateCanvases();
        }
    }
}

[System.Serializable]
public class OwnDrinkListWrapper
{
    public List<OwnDrink> drinks;
}
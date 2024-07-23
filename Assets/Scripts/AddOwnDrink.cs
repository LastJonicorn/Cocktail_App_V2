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
    public RawImage rawImage;
    public CameraController cameraController;
    public VerticalLayoutGroup layoutGroup;
    public List<GameObject> ingredientMeasurementPairs;

    private string picturePath;
    private List<Ingredient> ingredients = new List<Ingredient>();
    private int activePairsCount = 0;

    void Start()
    {
        takePictureButton.onClick.AddListener(RequestPermissionsAndTakePicture);
        addIngredientButton.onClick.AddListener(ActivateNextIngredientMeasurementPair);
        saveButton.onClick.AddListener(SaveOwnDrink);

        RequestPermissions();
    }

    private void OnEnable()
    {
        if (!string.IsNullOrEmpty(picturePath))
        {
            LoadPicture(picturePath);
        }

        if (cameraController != null)
        {
            cameraController.ReinitializeCamera();
        }

        RefreshForm();
    }

    private void OnDisable()
    {
        if (cameraController != null)
        {
            cameraController.StopCamera();
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
            picturePath = "";
            rawImage.texture = null;
            rawImage.rectTransform.sizeDelta = new Vector2(500, 500);

            cameraController.TakePicture();
            StartCoroutine(UpdatePicturePath());
        }
        else
        {
            Debug.LogError("CameraController reference not set in AddOwnDrink script.");
        }
    }

    private IEnumerator UpdatePicturePath()
    {
        yield return new WaitForSeconds(0.5f);

        while (string.IsNullOrEmpty(cameraController.GetPicturePath()))
        {
            yield return null;
        }

        picturePath = cameraController.GetPicturePath();

        if (!string.IsNullOrEmpty(picturePath))
        {
            LoadPicture(picturePath);
        }
    }

    private void LoadPicture(string path)
    {
        string fullPath = Path.Combine(Application.persistentDataPath, path);

        if (File.Exists(fullPath))
        {
            byte[] fileData = File.ReadAllBytes(fullPath);
            Texture2D texture = new Texture2D(2, 2);
            texture.LoadImage(fileData);

            if (rawImage != null)
            {
                rawImage.texture = texture;
                rawImage.rectTransform.sizeDelta = new Vector2(500, 500);
                rawImage.SetNativeSize();
            }
            else
            {
                Debug.LogWarning("RawImage is null when trying to load picture.");
            }
        }
        else
        {
            Debug.LogError("Failed to load picture: File does not exist at path: " + fullPath);
        }
    }

    public void ActivateNextIngredientMeasurementPair()
    {
        Debug.Log($"ActivateNextIngredientMeasurementPair called. Current active pairs count: {activePairsCount}");

        if (activePairsCount < ingredientMeasurementPairs.Count)
        {
            ingredientMeasurementPairs[activePairsCount].SetActive(true);
            activePairsCount++;
            Debug.Log($"Activated ingredient pair {activePairsCount - 1}");
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

            Debug.Log($"Pair {i}: Ingredient = {ingredientName}, Measurement = {measurement}");

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

        // Ensure the picture path is obtained from the CameraController
        string picturePath = cameraController.GetPicturePath();

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

        SetFeedbackText("Drink " + drinkName + " saved succesfully");

        RefreshForm();
    }



    private void RefreshForm()
    {
        Debug.Log("RefreshForm called");

        drinkNameInput.text = "";
        instructionsInput.text = "";

        foreach (GameObject pair in ingredientMeasurementPairs)
        {
            pair.SetActive(false);
        }

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

        picturePath = "";

        if (rawImage != null)
        {
            rawImage.texture = null;
            rawImage.rectTransform.sizeDelta = new Vector2(500, 500);
        }
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

    private void SetFeedbackText(string message)
    {
        feedbackText.text = message;
        StartCoroutine(ClearFeedbackTextAfterDelay(3));
    }

    private IEnumerator ClearFeedbackTextAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        feedbackText.text = "";
    }
}

[System.Serializable]
public class OwnDrinkListWrapper
{
    public List<OwnDrink> drinks;
}

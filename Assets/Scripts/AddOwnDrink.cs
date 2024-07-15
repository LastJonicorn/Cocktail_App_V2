using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Android;

[System.Serializable]
public class OwnDrink
{
    public string drinkName;
    public string picturePath;
    public List<Ingredient> ingredients;
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
    private int activePairsCount = 0; // Start from 0

    void Start()
    {
        takePictureButton.onClick.AddListener(RequestPermissionsAndTakePicture);
        addIngredientButton.onClick.AddListener(ActivateNextIngredientMeasurementPair);
        saveButton.onClick.AddListener(SaveOwnDrink);

        // Request necessary permissions at startup
        RequestPermissions();
    }

    private void OnEnable()
    {
        if (!string.IsNullOrEmpty(picturePath))
        {
            LoadPicture(picturePath);
        }

        // Reinitialize camera feed
        if (cameraController != null)
        {
            cameraController.ReinitializeCamera();
        }
    }

    private void OnDisable()
    {
        // Stop camera feed
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
            // Clear the previous picture path before taking a new picture
            picturePath = "";
            rawImage.texture = null;
            rawImage.rectTransform.sizeDelta = new Vector2(500, 500); // Maintain the size

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
        // Wait a bit to ensure the picture is saved correctly
        yield return new WaitForSeconds(0.5f);

        // Ensure picture is saved correctly
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
        if (System.IO.File.Exists(path))
        {
            byte[] fileData = System.IO.File.ReadAllBytes(path);
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
            Debug.LogError("Failed to load picture: File does not exist at path: " + path);
        }
    }




    public void ActivateNextIngredientMeasurementPair()
    {
        if (activePairsCount < ingredientMeasurementPairs.Count)
        {
            ingredientMeasurementPairs[activePairsCount].SetActive(true);
            activePairsCount++;
        }
        else
        {
            Debug.LogWarning("No more ingredient pairs to activate.");
        }
    }

    public void SaveOwnDrink()
    {
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

        SetFeedbackText("Drink saved: " + drinkName);

        // Refresh the form
        RefreshForm();
    }

    private void RefreshForm()
    {
        // Clear input fields
        drinkNameInput.text = "";
        instructionsInput.text = "";

        // Deactivate all ingredient pairs
        foreach (GameObject pair in ingredientMeasurementPairs)
        {
            pair.SetActive(false);
        }

        // Activate the first ingredient pair
        if (ingredientMeasurementPairs.Count > 0)
        {
            ingredientMeasurementPairs[0].SetActive(true);
            activePairsCount = 1;
        }
        else
        {
            activePairsCount = 0;
        }

        // Clear the input fields of the first ingredient pair
        if (activePairsCount > 0)
        {
            TMP_InputField ingredientInput = ingredientMeasurementPairs[0].transform.Find("IngredientInput").GetComponent<TMP_InputField>();
            TMP_InputField measurementInput = ingredientMeasurementPairs[0].transform.Find("MeasurementInput").GetComponent<TMP_InputField>();
            ingredientInput.text = "";
            measurementInput.text = "";
        }

        // Reset the picture path
        picturePath = "";

        // Ensure the rawImage is visible and set its size to 500x500
        if (rawImage != null)
        {
            if (!string.IsNullOrEmpty(picturePath) && System.IO.File.Exists(picturePath))
            {
                LoadPicture(picturePath);
            }
            else
            {
                rawImage.texture = null; // Or set it to a default texture if you have one
            }

            // Set the size of the rawImage
            rawImage.rectTransform.sizeDelta = new Vector2(500, 500);
        }

        // Clear feedback text
        feedbackText.text = "";
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

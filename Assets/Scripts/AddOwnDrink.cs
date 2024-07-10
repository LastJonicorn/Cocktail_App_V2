using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

[System.Serializable]
public class OwnDrink
{
    public string drinkName;
    public string picturePath; // Path to the picture file
    public List<Ingredient> ingredients; // Changed to List of Ingredient objects
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
    public Button takePictureButton; // Button to take a picture
    public Button addIngredientButton; // Button to add ingredient and measurement pair
    public Button saveButton; // Button to save the drink
    public TextMeshProUGUI feedbackText; // Text for displaying feedback to the user
    public RawImage rawImage; // UI element to display the picture
    public GameObject ingredientMeasurementPrefab; // Prefab for ingredient and measurement pair
    public CameraController cameraController; // Reference to CameraController script
    public Transform ingredientsParent; // Parent object to hold dynamically added ingredients
    public VerticalLayoutGroup layoutGroup; // Vertical layout group to handle the layout

    private string picturePath;
    private List<Ingredient> ingredients = new List<Ingredient>(); // List to store ingredients

    private bool isCameraInitialized = false; // Flag to track if camera is initialized

    void Start()
    {
        // Add listeners to buttons
        takePictureButton.onClick.AddListener(TakePicture);
        addIngredientButton.onClick.AddListener(AddIngredientMeasurementPair);
        saveButton.onClick.AddListener(SaveOwnDrink);

        // Initialize camera and display the current picture if available
        InitializeCamera();
    }

    private void InitializeCamera()
    {
        if (cameraController != null)
        {
            cameraController.InitializeCamera();
            isCameraInitialized = true; // Set the flag
        }
        else
        {
            Debug.LogError("CameraController reference not set in AddOwnDrink script.");
        }
    }

    private void OnEnable()
    {
        // Check if camera was initialized and display current picture
        if (isCameraInitialized)
        {
            DisplayCurrentPicture();
        }
    }

    private void DisplayCurrentPicture()
    {
        string currentPicturePath = cameraController.GetPicturePath();
        if (!string.IsNullOrEmpty(currentPicturePath))
        {
            LoadPicture(currentPicturePath);
        }
    }

    private void LoadPicture(string path)
    {
        if (System.IO.File.Exists(path))
        {
            byte[] fileData = System.IO.File.ReadAllBytes(path);
            Texture2D texture = new Texture2D(2, 2);
            texture.LoadImage(fileData); // Load data into the texture

            if (rawImage != null)
            {
                rawImage.texture = texture;
                rawImage.rectTransform.sizeDelta = new Vector2(500, 500); // Set size if needed
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

    public void TakePicture()
    {
        if (cameraController != null)
        {
            cameraController.TakePicture();
            UpdatePicturePath();
        }
        else
        {
            Debug.LogError("CameraController reference not set in AddOwnDrink script.");
        }
    }

    private void UpdatePicturePath()
    {
        picturePath = cameraController.GetPicturePath();

        // Display the newly taken picture
        if (!string.IsNullOrEmpty(picturePath))
        {
            LoadPicture(picturePath);
        }
    }

    public void AddIngredientMeasurementPair()
    {
        if (ingredientMeasurementPrefab != null && ingredientsParent != null)
        {
            GameObject pair = Instantiate(ingredientMeasurementPrefab, ingredientsParent);
            pair.transform.SetParent(ingredientsParent);

            TMP_InputField ingredientInput = pair.transform.Find("IngredientInput").GetComponent<TMP_InputField>();
            TMP_InputField measurementInput = pair.transform.Find("MeasurementInput").GetComponent<TMP_InputField>();

            // Add a listener to the ingredient input field
            ingredientInput.onValueChanged.AddListener(delegate { OnIngredientValueChanged(pair); });

            // Add a listener to the measurement input field
            measurementInput.onValueChanged.AddListener(delegate { OnMeasurementValueChanged(pair); });

            // Refresh the layout group
            StartCoroutine(DelayedLayoutRefresh());
        }
        else
        {
            Debug.LogError("Ingredient Measurement Prefab or Ingredients Parent not assigned in AddOwnDrink script.");
        }
    }

    private void OnIngredientValueChanged(GameObject pair)
    {
        TMP_InputField ingredientInput = pair.transform.Find("IngredientInput").GetComponent<TMP_InputField>();
        TMP_InputField measurementInput = pair.transform.Find("MeasurementInput").GetComponent<TMP_InputField>();

        string ingredientName = ingredientInput.text;
        string measurement = measurementInput.text;

        // Find if this ingredient already exists in the list
        Ingredient existingIngredient = ingredients.Find(ing => ing.name == ingredientName);

        // If the ingredient exists, update its measurement
        if (existingIngredient != null)
        {
            existingIngredient.measurement = measurement;
        }
        else
        {
            // Otherwise, add a new ingredient to the list
            ingredients.Add(new Ingredient { name = ingredientName, measurement = measurement });
        }
    }

    private void OnMeasurementValueChanged(GameObject pair)
    {
        TMP_InputField ingredientInput = pair.transform.Find("IngredientInput").GetComponent<TMP_InputField>();
        TMP_InputField measurementInput = pair.transform.Find("MeasurementInput").GetComponent<TMP_InputField>();

        string ingredientName = ingredientInput.text;
        string measurement = measurementInput.text;

        // Find if this ingredient already exists in the list
        Ingredient existingIngredient = ingredients.Find(ing => ing.name == ingredientName);

        // If the ingredient exists, update its measurement
        if (existingIngredient != null)
        {
            existingIngredient.measurement = measurement;
        }
        else
        {
            // Otherwise, add a new ingredient to the list
            ingredients.Add(new Ingredient { name = ingredientName, measurement = measurement });
        }
    }

    private IEnumerator DelayedLayoutRefresh()
    {
        // Wait for the end of the frame before refreshing layout
        yield return new WaitForEndOfFrame();

        // Force update of the layout group
        LayoutRebuilder.ForceRebuildLayoutImmediate(layoutGroup.GetComponent<RectTransform>());
    }

    public void SaveOwnDrink()
    {
        string drinkName = drinkNameInput.text;
        if (string.IsNullOrEmpty(drinkName))
        {
            feedbackText.text = "Drink name cannot be empty.";
            return;
        }

        string instructions = instructionsInput.text;

        if (ingredients.Count == 0 || string.IsNullOrEmpty(instructions))
        {
            feedbackText.text = "All fields must be filled out.";
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

        feedbackText.text = "Drink saved: " + drinkName;
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
}

[System.Serializable]
public class OwnDrinkListWrapper
{
    public List<OwnDrink> drinks;
}

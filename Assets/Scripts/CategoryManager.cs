using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;

public class CategoryManager : MonoBehaviour
{
    public GameObject addCategoryPanel;
    public TMP_InputField categoryInputField;
    public Button addCategoryButton;
    public Button saveCategoryButton; // Button to save and close the category panel
    public Transform contentPanel;
    public GameObject textPrefab;
    public GameObject itemPrefab; // Prefab for items under categories

    public GameObject addItemPanel; // Panel to add items
    public TMP_InputField itemInputField; // Input field for item name
    public TMP_InputField ratingInputField; // Input field for item rating
    public Button confirmAddItemButton; // Button to confirm adding the item

    private Dictionary<string, GameObject> categoryTextObjects = new Dictionary<string, GameObject>();
    private Dictionary<string, List<string>> categoriesWithItems = new Dictionary<string, List<string>>(); // Store items for each category
    private List<string> categories = new List<string>(); // Store category names
    private string currentCategoryName; // Store the category currently being edited

    void Start()
    {
        addCategoryButton.onClick.AddListener(OpenAddCategoryPanel);
        saveCategoryButton.onClick.AddListener(AddCategory); // Link the save button to AddCategory
        confirmAddItemButton.onClick.AddListener(AddItemToCurrentCategory); // Link the confirm button to AddItemToCurrentCategory

        // Set validation for rating input field
        ratingInputField.onValidateInput += ValidateRatingInput;

        LoadCategories();
        DisplayCategories();

        // Ensure the addItemPanel is hidden at start
        addItemPanel.SetActive(false);
    }

    // Validation callback for numeric input
    private char ValidateRatingInput(string text, int charIndex, char addedChar)
    {
        // Allow only numeric characters
        if (char.IsDigit(addedChar))
        {
            return addedChar;
        }
        return '\0'; // Disallow the character
    }

    public void OpenAddCategoryPanel()
    {
        addCategoryPanel.SetActive(true);
    }

    public void AddCategory()
    {
        string categoryName = categoryInputField.text;
        if (!string.IsNullOrEmpty(categoryName) && !categories.Contains(categoryName))
        {
            categories.Add(categoryName);
            categoriesWithItems[categoryName] = new List<string>(); // Initialize item list for new category
            SaveCategories();
            categoryInputField.text = ""; // Clear the input field
            addCategoryPanel.SetActive(false); // Deactivate the panel after saving
            CreateCategoryText(categoryName); // Update the UI with the new category
        }
    }

    private void SaveCategories()
    {
        // Save categories
        PlayerPrefs.SetString("Categories", string.Join(",", categories.ToArray()));

        // Save items for each category
        foreach (var category in categoriesWithItems)
        {
            string itemsJson = JsonUtility.ToJson(new ItemListWrapper { items = category.Value });
            PlayerPrefs.SetString(category.Key + "_Items", itemsJson);
        }
        PlayerPrefs.Save();
    }

    private void LoadCategories()
    {
        if (PlayerPrefs.HasKey("Categories"))
        {
            string savedCategories = PlayerPrefs.GetString("Categories");
            categories = new List<string>(savedCategories.Split(','));

            // Load items for each category
            foreach (string category in categories)
            {
                if (PlayerPrefs.HasKey(category + "_Items"))
                {
                    string itemsJson = PlayerPrefs.GetString(category + "_Items");
                    var itemListWrapper = JsonUtility.FromJson<ItemListWrapper>(itemsJson);
                    categoriesWithItems[category] = itemListWrapper.items;
                }
                else
                {
                    categoriesWithItems[category] = new List<string>();
                }
            }
        }
    }

    private void DisplayCategories()
    {
        foreach (string category in categories)
        {
            CreateCategoryText(category);

            // Display items for the category
            if (categoriesWithItems.TryGetValue(category, out List<string> items))
            {
                foreach (string itemJson in items)
                {
                    var itemData = JsonUtility.FromJson<ItemData>(itemJson);
                    AddItemToCategory(category, itemData.name, itemData.rating);
                }
            }
        }
    }

    private void CreateCategoryText(string categoryName)
    {
        GameObject categoryObj = Instantiate(textPrefab, contentPanel);
        TextMeshProUGUI textComponent = categoryObj.GetComponentInChildren<TextMeshProUGUI>();
        Button addButton = categoryObj.GetComponentInChildren<Button>();
        Button deleteCategoryButton = categoryObj.transform.Find("DeleteCategoryButton").GetComponent<Button>(); // Assuming your delete button is named "DeleteCategoryButton"

        textComponent.text = categoryName;
        addButton.onClick.AddListener(() => OpenAddItemPanel(categoryName));

        // Create a new empty GameObject to act as a container for items
        GameObject itemsContainer = new GameObject("ItemsContainer");
        itemsContainer.transform.SetParent(contentPanel, false); // Set parent to contentPanel

        // Add VerticalLayoutGroup component and configure it
        VerticalLayoutGroup layoutGroup = itemsContainer.AddComponent<VerticalLayoutGroup>();
        layoutGroup.childAlignment = TextAnchor.MiddleCenter; // Align items to middle center
        layoutGroup.spacing = 10f; // Example spacing between items
        layoutGroup.padding = new RectOffset(0, 0, 0, 0); // Adjust padding as needed

        // Configure the RectTransform
        RectTransform rectTransform = itemsContainer.GetComponent<RectTransform>();
        rectTransform.anchorMin = new Vector2(0, 0); // Set anchors to stretch horizontally
        rectTransform.anchorMax = new Vector2(1, 1); // Set anchors to stretch vertically
        rectTransform.pivot = new Vector2(0.5f, 0.5f); // Center pivot
        rectTransform.anchoredPosition = Vector2.zero; // Center alignment

        // Ensure it is below the category object
        itemsContainer.transform.SetAsLastSibling();

        // Store the reference to the container in the dictionary
        categoryTextObjects[categoryName] = itemsContainer;

        // Add listener to delete category button
        deleteCategoryButton.onClick.AddListener(() => DeleteCategory(categoryName, categoryObj, itemsContainer));
    }

    private void DeleteCategory(string categoryName, GameObject categoryObj, GameObject itemsContainer)
    {
        // Remove the category from the list
        if (categories.Contains(categoryName))
        {
            categories.Remove(categoryName);
            categoriesWithItems.Remove(categoryName); // Remove items associated with this category

            // Destroy the category and its items in the UI
            Destroy(categoryObj);
            Destroy(itemsContainer);

            // Remove the category data from PlayerPrefs
            PlayerPrefs.DeleteKey(categoryName + "_Items");
            SaveCategories(); // Save changes
        }
    }


    private void OpenAddItemPanel(string categoryName)
    {
        currentCategoryName = categoryName;
        addItemPanel.SetActive(true);
    }

    public void CloseAddItemPanel()
    {
        addItemPanel.SetActive(false);
    }

    public void CloseAddCategoryPanel()
    {
        addCategoryPanel.SetActive(false);
    }

    private void AddItemToCurrentCategory()
    {
        string itemName = itemInputField.text;
        string ratingText = ratingInputField.text;

        if (!string.IsNullOrEmpty(itemName) && !string.IsNullOrEmpty(ratingText))
        {
            // Add item with rating to the category
            AddItemToCategory(currentCategoryName, itemName, ratingText);
            itemInputField.text = ""; // Clear the item name input field
            ratingInputField.text = ""; // Clear the rating input field
            addItemPanel.SetActive(false); // Deactivate the panel after adding the item

            // Add item and rating to the list and save
            categoriesWithItems[currentCategoryName].Add(JsonUtility.ToJson(new ItemData { name = itemName, rating = ratingText }));
            SaveCategories();
        }
    }

    private void AddItemToCategory(string categoryName, string itemName, string rating)
    {
        if (categoryTextObjects.TryGetValue(categoryName, out GameObject itemsContainer))
        {
            GameObject newItemObj = Instantiate(itemPrefab, itemsContainer.transform);

            // Find the components
            TextMeshProUGUI itemText = newItemObj.transform.Find("ItemNameText").GetComponentInChildren<TextMeshProUGUI>();
            TextMeshProUGUI ratingText = newItemObj.transform.Find("RatingText").GetComponentInChildren<TextMeshProUGUI>();
            Button deleteButton = newItemObj.transform.Find("DeleteButton").GetComponent<Button>(); // Assuming your delete button is named "DeleteButton"

            if (itemText != null && ratingText != null && deleteButton != null)
            {
                itemText.text = itemName;
                ratingText.text = rating + " / 10";

                // Add listener to delete button
                deleteButton.onClick.AddListener(() => DeleteItem(categoryName, itemName, newItemObj));
            }
            else
            {
                Debug.LogError("Item components not found in the itemPrefab.");
            }
        }
        else
        {
            Debug.LogError($"Category '{categoryName}' not found in categoryTextObjects.");
        }
    }

    private void DeleteItem(string categoryName, string itemName, GameObject itemObj)
    {
        // Remove item from the list
        if (categoriesWithItems.TryGetValue(categoryName, out List<string> items))
        {
            items.Remove(itemName);

            // Destroy the item GameObject
            Destroy(itemObj);

            // Save changes to PlayerPrefs
            SaveCategories();
        }
    }


    // Helper class to wrap item lists for JSON serialization
    [System.Serializable]
    private class ItemData
    {
        public string name;
        public string rating;
    }

    [System.Serializable]
    private class ItemListWrapper
    {
        public List<string> items;
    }
}

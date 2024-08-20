using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;

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
    public Button confirmAddItemButton; // Button to confirm adding the item

    private Dictionary<string, GameObject> categoryTextObjects = new Dictionary<string, GameObject>();
    private List<string> categories = new List<string>(); // Ensure this is declared here
    private string currentCategoryName; // Store the category currently being edited

    void Start()
    {
        addCategoryButton.onClick.AddListener(OpenAddCategoryPanel);
        saveCategoryButton.onClick.AddListener(AddCategory); // Link the save button to AddCategory
        confirmAddItemButton.onClick.AddListener(AddItemToCurrentCategory); // Link the confirm button to AddItemToCurrentCategory

        LoadCategories();
        DisplayCategories();

        // Ensure the addItemPanel is hidden at start
        addItemPanel.SetActive(false);
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
            PlayerPrefs.SetString("Categories", string.Join(",", categories.ToArray()));
            PlayerPrefs.Save();
            categoryInputField.text = ""; // Clear the input field
            addCategoryPanel.SetActive(false); // Deactivate the panel after saving
            CreateCategoryText(categoryName); // Update the UI with the new category
        }
    }

    private void LoadCategories()
    {
        if (PlayerPrefs.HasKey("Categories"))
        {
            string savedCategories = PlayerPrefs.GetString("Categories");
            categories = new List<string>(savedCategories.Split(','));
        }
    }

    private void DisplayCategories()
    {
        foreach (string category in categories)
        {
            CreateCategoryText(category);
        }
    }

    private void CreateCategoryText(string categoryName)
    {
        // Instantiate the category text prefab
        GameObject categoryObj = Instantiate(textPrefab, contentPanel);
        TextMeshProUGUI textComponent = categoryObj.GetComponentInChildren<TextMeshProUGUI>();
        Button addButton = categoryObj.GetComponentInChildren<Button>();

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
    }



    private void OpenAddItemPanel(string categoryName)
    {
        currentCategoryName = categoryName;
        addItemPanel.SetActive(true);
    }

    private void AddItemToCurrentCategory()
    {
        string itemName = itemInputField.text;
        if (!string.IsNullOrEmpty(itemName))
        {
            AddItemToCategory(currentCategoryName, itemName);
            itemInputField.text = ""; // Clear the input field
            addItemPanel.SetActive(false); // Deactivate the panel after adding the item
        }
    }

    private void AddItemToCategory(string categoryName, string itemName)
    {
        if (categoryTextObjects.TryGetValue(categoryName, out GameObject itemsContainer))
        {
            GameObject newItemObj = Instantiate(itemPrefab, itemsContainer.transform);
            TextMeshProUGUI itemText = newItemObj.GetComponentInChildren<TextMeshProUGUI>();
            itemText.text = itemName;
        }
    }

}

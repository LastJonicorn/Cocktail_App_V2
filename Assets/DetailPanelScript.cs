// DetailPanelScript.cs
using UnityEngine;
using TMPro;

public class DetailPanelScript : MonoBehaviour
{
    public TextMeshProUGUI nameLabel;
    public TextMeshProUGUI categoryLabel;
    public TextMeshProUGUI instructionsLabel;

    public void DisplayDrinkDetails(Drink drink)
    {
        // Update UI elements with drink details
        nameLabel.text = drink.strDrink;
        categoryLabel.text = "Category: " + drink.strCategory;
        instructionsLabel.text = "Instructions: " + drink.strInstructions;
    }
}

using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.Networking;

public class DetailPanelScript : MonoBehaviour
{
    public Image drinkImage;
    public TextMeshProUGUI nameLabel;
    public TextMeshProUGUI instructionsLabel;

    [Header("Ingredient and Measurement Pairs")]
    public GameObject[] ingredientPairs;

    public void DisplayDrinkDetails(Drink drink)
    {
        // Update UI elements with drink details
        nameLabel.text = drink.strDrink;
        instructionsLabel.text = "Instructions: " + drink.strInstructions;

        // Load drink image
        StartCoroutine(LoadImage(drink.strDrinkThumb));

        // Update ingredients and measurements
        AssignText(ingredientPairs, new string[]
        {
            drink.strIngredient1, drink.strIngredient2, drink.strIngredient3, drink.strIngredient4,
            drink.strIngredient5, drink.strIngredient6, drink.strIngredient7, drink.strIngredient8,
            drink.strIngredient9, drink.strIngredient10, drink.strIngredient11, drink.strIngredient12,
            drink.strIngredient13, drink.strIngredient14, drink.strIngredient15
        }, new string[]
        {
            drink.strMeasure1, drink.strMeasure2, drink.strMeasure3, drink.strMeasure4,
            drink.strMeasure5, drink.strMeasure6, drink.strMeasure7, drink.strMeasure8,
            drink.strMeasure9, drink.strMeasure10, drink.strMeasure11, drink.strMeasure12,
            drink.strMeasure13, drink.strMeasure14, drink.strMeasure15
        });
    }

    private void AssignText(GameObject[] ingredientPairs, string[] ingredients, string[] measurements)
    {
        for (int i = 0; i < ingredientPairs.Length; i++)
        {
            bool hasIngredient = !string.IsNullOrEmpty(ingredients[i]);
            bool hasMeasurement = !string.IsNullOrEmpty(measurements[i]);

            ingredientPairs[i].SetActive(hasIngredient || hasMeasurement);

            if (hasIngredient || hasMeasurement)
            {
                TextMeshProUGUI[] texts = ingredientPairs[i].GetComponentsInChildren<TextMeshProUGUI>();
                if (texts.Length == 2)
                {
                    texts[0].text = hasIngredient ? ingredients[i] : "";
                    texts[1].text = hasMeasurement ? measurements[i] : "";
                }
            }
        }

        LayoutRebuilder.ForceRebuildLayoutImmediate(GetComponent<RectTransform>());
    }

    private IEnumerator LoadImage(string imageUrl)
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
                if (texture != null)
                {
                    drinkImage.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
                    drinkImage.preserveAspect = true;
                }
                else
                {
                    Debug.LogError("Downloaded texture is null");
                }
            }
        }
    }
}

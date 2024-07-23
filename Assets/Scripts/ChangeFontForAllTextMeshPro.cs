using UnityEngine;
using TMPro;

public class ChangeFontForAllTextMeshPro : MonoBehaviour
{
    public TMP_FontAsset newFont; // Assign the new font in the Inspector

    void OnEnable()
    {
        ChangeAllTextMeshProFonts();
    }

    void ChangeAllTextMeshProFonts()
    {
        // Find all TextMeshProUGUI components in the scene
        TextMeshProUGUI[] textMeshProUGUIs = FindObjectsOfType<TextMeshProUGUI>(true); // Include inactive objects
        foreach (TextMeshProUGUI textMesh in textMeshProUGUIs)
        {
            textMesh.font = newFont;
        }

        // Find all TextMeshPro components (for 3D text) in the scene
        TextMeshPro[] textMeshPros = FindObjectsOfType<TextMeshPro>(true); // Include inactive objects
        foreach (TextMeshPro textMesh in textMeshPros)
        {
            textMesh.font = newFont;
        }
    }
}

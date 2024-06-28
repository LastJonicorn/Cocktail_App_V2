using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class CameraController : MonoBehaviour
{
    public RawImage rawImage;
    private WebCamTexture webCamTexture;

    void Start()
    {
        // Initialize the WebCamTexture
        webCamTexture = new WebCamTexture();
        rawImage.texture = webCamTexture;
        rawImage.material.mainTexture = webCamTexture;
        webCamTexture.Play();

        // Set the RawImage dimensions to 500x500
        rawImage.rectTransform.sizeDelta = new Vector2(500, 500);
    }

    void OnDisable()
    {
        // Stop the WebCamTexture when the GameObject is disabled
        if (webCamTexture != null)
        {
            webCamTexture.Stop();
            rawImage.texture = null;
            rawImage.material.mainTexture = null;
        }
    }

    public void TakePicture()
    {
        // Start coroutine to capture the photo
        StartCoroutine(CapturePhoto());
    }

    private IEnumerator CapturePhoto()
    {
        // Wait for the end of the frame to capture the photo
        yield return new WaitForEndOfFrame();

        // Capture the photo from the WebCamTexture
        Texture2D photo = new Texture2D(webCamTexture.width, webCamTexture.height);
        photo.SetPixels(webCamTexture.GetPixels());
        photo.Apply();

        // Crop and resize the photo to a square
        Texture2D squarePhoto = CropToSquare(photo);

        // Save the photo to persistent data path
        byte[] bytes = squarePhoto.EncodeToPNG();
        string filePath = System.IO.Path.Combine(Application.persistentDataPath, "photo.png");
        System.IO.File.WriteAllBytes(filePath, bytes);

        Debug.Log("Photo saved to: " + filePath);

        // Update RawImage to show the captured square photo
        rawImage.texture = squarePhoto;

        // Clean up temporary texture
        Destroy(photo);
    }

    private Texture2D CropToSquare(Texture2D original)
    {
        int size = Mathf.Min(original.width, original.height);
        int x = (original.width - size) / 2;
        int y = (original.height - size) / 2;

        // Create a cropped square texture
        Texture2D cropped = new Texture2D(size, size);
        cropped.SetPixels(original.GetPixels(x, y, size, size));
        cropped.Apply();

        // Resize the cropped image to 500x500
        Texture2D resizedCropped = ResizeTexture(cropped, 500, 500);

        // Clean up temporary cropped texture
        Destroy(cropped);

        return resizedCropped;
    }

    private Texture2D ResizeTexture(Texture2D original, int width, int height)
    {
        RenderTexture rt = RenderTexture.GetTemporary(width, height);
        rt.filterMode = FilterMode.Trilinear;

        // Set the active RenderTexture and blit the original texture into it
        RenderTexture.active = rt;
        Graphics.Blit(original, rt);
        Texture2D result = new Texture2D(width, height);
        result.ReadPixels(new Rect(0, 0, width, height), 0, 0);
        result.Apply();

        // Clean up RenderTexture
        RenderTexture.active = null;
        RenderTexture.ReleaseTemporary(rt);

        return result;
    }
}

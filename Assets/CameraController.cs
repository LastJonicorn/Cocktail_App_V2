using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class CameraController : MonoBehaviour
{
    public RawImage rawImage;
    public AspectRatioFitter aspectRatioFitter;
    private WebCamTexture webCamTexture;

    void Start()
    {
        // Start the camera
        webCamTexture = new WebCamTexture();
        rawImage.texture = webCamTexture;
        rawImage.material.mainTexture = webCamTexture;
        webCamTexture.Play();
    }

    public void TakePicture()
    {
        StartCoroutine(CapturePhoto());
    }

    private IEnumerator CapturePhoto()
    {
        // Wait for the end of the frame to capture the photo
        yield return new WaitForEndOfFrame();

        Texture2D photo = new Texture2D(webCamTexture.width, webCamTexture.height);
        photo.SetPixels(webCamTexture.GetPixels());
        photo.Apply();

        // Crop the photo to a square
        Texture2D squarePhoto = CropToSquare(photo);

        // Save the photo to persistent data path
        byte[] bytes = squarePhoto.EncodeToPNG();
        string filePath = System.IO.Path.Combine(Application.persistentDataPath, "photo.png");
        System.IO.File.WriteAllBytes(filePath, bytes);

        Debug.Log("Photo saved to: " + filePath);

        // Update RawImage to show the captured square photo
        rawImage.texture = squarePhoto;
        rawImage.rectTransform.sizeDelta = new Vector2(squarePhoto.width, squarePhoto.height);
    }

    private Texture2D CropToSquare(Texture2D original)
    {
        int size = Mathf.Min(original.width, original.height);
        int x = (original.width - size) / 2;
        int y = (original.height - size) / 2;

        Texture2D cropped = new Texture2D(size, size);
        cropped.SetPixels(original.GetPixels(x, y, size, size));
        cropped.Apply();

        return cropped;
    }

    void Update()
    {
        // Adjust the aspect ratio of the RawImage to maintain a square display
        if (webCamTexture.width > 100)
        {
            float aspectRatio = (float)webCamTexture.width / (float)webCamTexture.height;
            if (aspectRatio > 1) // Width > Height
            {
                aspectRatioFitter.aspectRatio = 1.0f / aspectRatio;
            }
            else // Height >= Width
            {
                aspectRatioFitter.aspectRatio = aspectRatio;
            }
            rawImage.rectTransform.sizeDelta = new Vector2(Mathf.Min(rawImage.rectTransform.sizeDelta.x, rawImage.rectTransform.sizeDelta.y), Mathf.Min(rawImage.rectTransform.sizeDelta.x, rawImage.rectTransform.sizeDelta.y));
        }
    }
}

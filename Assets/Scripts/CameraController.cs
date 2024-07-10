using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class CameraController : MonoBehaviour
{
    public RawImage rawImage;
    private WebCamTexture webCamTexture;
    private string picturePath;

    void Start()
    {
        // Initialize the WebCamTexture
        webCamTexture = new WebCamTexture();
        rawImage.texture = webCamTexture;
        rawImage.material.mainTexture = webCamTexture;
        webCamTexture.Play();

        // Set the RawImage dimensions to 500x500
        rawImage.rectTransform.sizeDelta = new Vector2(500, 500);

        StartCoroutine(InitializeCamera());
    }

    public IEnumerator InitializeCamera()
    {
        // Wait until the camera is fully initialized
        while (!webCamTexture.didUpdateThisFrame)
        {
            yield return null;
        }

        if (webCamTexture.width > 16 && webCamTexture.height > 16)
        {
            // Successfully initialized
            Debug.Log("Camera successfully initialized");
        }
        else
        {
            Debug.LogError("Failed to initialize camera");
        }
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

        // Rotate the photo to correct orientation
        Texture2D rotatedPhoto = RotateTexture(photo, GetRotationAngle());

        // Crop the rotated photo to a square
        Texture2D squarePhoto = CropToSquare(rotatedPhoto);

        // Save the photo to persistent data path
        byte[] bytes = squarePhoto.EncodeToPNG();
        picturePath = System.IO.Path.Combine(Application.persistentDataPath, System.Guid.NewGuid().ToString() + ".png");
        System.IO.File.WriteAllBytes(picturePath, bytes);

        Debug.Log("Photo saved to: " + picturePath);

        // Update RawImage to show the captured square photo
        rawImage.texture = squarePhoto;

        // Clean up temporary textures
        Destroy(photo);
        Destroy(rotatedPhoto);
    }

    public string GetPicturePath()
    {
        return picturePath;
    }

    private int GetRotationAngle()
    {
#if UNITY_ANDROID
        // Check the device orientation
        switch (Screen.orientation)
        {
            case ScreenOrientation.Portrait:
                return 0;
            case ScreenOrientation.LandscapeLeft:
                return -90;
            case ScreenOrientation.LandscapeRight:
                return 90;
            case ScreenOrientation.PortraitUpsideDown:
                return 180;
            default:
                return 0;
        }
#else
        return 0;
#endif
    }

    private Texture2D RotateTexture(Texture2D original, int angle)
    {
        Texture2D rotated = new Texture2D(original.height, original.width);
        Color32[] originalPixels = original.GetPixels32();
        Color32[] rotatedPixels = new Color32[originalPixels.Length];

        int w = original.width;
        int h = original.height;

        for (int i = 0; i < originalPixels.Length; ++i)
        {
            int x = i % w;
            int y = i / w;

            int newX = x;
            int newY = y;

            switch (angle)
            {
                case 90:
                    newX = h - y - 1;
                    newY = x;
                    break;
                case -90:
                    newX = y;
                    newY = w - x - 1;
                    break;
                case 180:
                    newX = w - x - 1;
                    newY = h - y - 1;
                    break;
            }

            rotatedPixels[newY * rotated.width + newX] = originalPixels[i];
        }

        rotated.SetPixels32(rotatedPixels);
        rotated.Apply();
        return rotated;
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

        return cropped;
    }
}

using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.IO;

public class CameraController : MonoBehaviour
{
    public RawImage cameraRawImage;
    private WebCamTexture webCamTexture;
    private string picturePath;
    private bool isCapturing = false;

    public void InitializeCamera()
    {
        if (webCamTexture != null)
        {
            webCamTexture.Stop();
            webCamTexture = null;
        }

        // Initialize the WebCamTexture
        webCamTexture = new WebCamTexture();
        cameraRawImage.texture = webCamTexture;
        cameraRawImage.material.mainTexture = webCamTexture;
        webCamTexture.Play();

        StartCoroutine(CheckCameraInitialization());
    }

    private IEnumerator CheckCameraInitialization()
    {
        while (!webCamTexture.didUpdateThisFrame)
        {
            yield return null;
        }

        if (webCamTexture.width > 16 && webCamTexture.height > 16)
        {
            // Force the camera feed to be displayed correctly in portrait mode
            AdjustCameraForPortraitMode();
        }
        else
        {
            Debug.LogError("Failed to initialize camera");
        }
    }

    private void AdjustCameraForPortraitMode()
    {
        // Force the camera feed to portrait mode by rotating the RawImage
        cameraRawImage.rectTransform.localEulerAngles = new Vector3(0, 0, 90);

        // Flip the image if it's mirrored (for front cameras)
        bool isVertical = webCamTexture.videoVerticallyMirrored;
        cameraRawImage.rectTransform.localScale = new Vector3(isVertical ? 1 : -1, -1, 1);
    }

    public void StopCamera()
    {
        if (webCamTexture != null)
        {
            webCamTexture.Stop();
            webCamTexture = null;
        }

        if (cameraRawImage != null)
        {
            cameraRawImage.texture = null;
            cameraRawImage.material.mainTexture = null;
        }
    }

    public void TakePicture()
    {
        if (webCamTexture != null && webCamTexture.isPlaying && !isCapturing)
        {
            StartCoroutine(CapturePhoto());
        }
        else
        {
            Debug.LogError("Webcam is not running or a photo is already being captured.");
        }
    }

    private IEnumerator CapturePhoto()
    {
        isCapturing = true;

        yield return new WaitForEndOfFrame();

        if (webCamTexture == null || !webCamTexture.isPlaying)
        {
            Debug.LogError("Webcam is not running, cannot capture photo.");
            isCapturing = false;
            yield break;
        }

        try
        {
            // Capture the photo from the webcam
            Texture2D photo = new Texture2D(webCamTexture.width, webCamTexture.height, TextureFormat.RGB24, false);
            photo.SetPixels(webCamTexture.GetPixels());
            photo.Apply();

            // Rotate the captured photo to match the portrait orientation
            Texture2D rotatedPhoto = RotateTextureLeft(photo); // Rotate left (90 degrees counter-clockwise)

            // Optionally resize the rotated photo
            Texture2D squarePhoto = ResizeTexture(rotatedPhoto, 500, 500);

            // Save the rotated and resized photo
            byte[] bytes = squarePhoto.EncodeToPNG();
            string newPicturePath = Path.Combine(Application.persistentDataPath, System.Guid.NewGuid().ToString() + ".png");
            File.WriteAllBytes(newPicturePath, bytes);

            picturePath = newPicturePath;

            Debug.Log("Photo saved to: " + picturePath);

            // Clean up
            Destroy(photo);
            Destroy(rotatedPhoto);
            Destroy(squarePhoto);
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"An error occurred while capturing the photo: {ex.Message}");
        }

        isCapturing = false;
    }

    private Texture2D RotateTextureLeft(Texture2D original)
    {
        int width = original.width;
        int height = original.height;
        Texture2D rotated = new Texture2D(height, width);

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                rotated.SetPixel(y, width - x - 1, original.GetPixel(x, y));
            }
        }

        rotated.Apply();
        return rotated;
    }

    private Texture2D ResizeTexture(Texture2D original, int width, int height)
    {
        RenderTexture rt = RenderTexture.GetTemporary(width, height);
        rt.filterMode = FilterMode.Trilinear;

        RenderTexture.active = rt;
        Graphics.Blit(original, rt);
        Texture2D result = new Texture2D(width, height);
        result.ReadPixels(new Rect(0, 0, width, height), 0, 0);
        result.Apply();

        RenderTexture.active = null;
        RenderTexture.ReleaseTemporary(rt);

        return result;
    }

    public string GetPicturePath()
    {
        return picturePath;
    }

    public void ReinitializeCamera()
    {
        InitializeCamera();
    }
}

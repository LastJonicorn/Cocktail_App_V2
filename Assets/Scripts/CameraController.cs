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
            //AdjustCameraForPortraitMode();
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
    public void PauseCamera()
    {
        if (webCamTexture != null && webCamTexture.isPlaying)
        {
            webCamTexture.Pause();

            // Keep the current frame on the screen by not changing the texture
            // Ensure the RawImage keeps the current camera texture
            cameraRawImage.texture = webCamTexture;
            cameraRawImage.material.mainTexture = webCamTexture;
        }
        else
        {
            Debug.LogError("WebCamTexture is not running, cannot pause the camera.");
        }
    }


    public void ResumeCamera()
    {
        if (webCamTexture != null && !webCamTexture.isPlaying)
        {
            webCamTexture.Play();  // This resumes the WebCamTexture after pausing
        }
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

            // Optionally resize the captured photo
            Texture2D squarePhoto = ResizeTexture(photo, 500, 500);

            // Save the resized photo
            byte[] bytes = squarePhoto.EncodeToPNG();
            string newPicturePath = Path.Combine(Application.persistentDataPath, System.Guid.NewGuid().ToString() + ".png");
            File.WriteAllBytes(newPicturePath, bytes);

            picturePath = newPicturePath;

            Debug.Log("Photo saved to: " + picturePath);

            // Clean up
            Destroy(photo);
            Destroy(squarePhoto);

            PauseCamera();

            Texture2D loadTest = new Texture2D(2, 2);
            if (loadTest.LoadImage(File.ReadAllBytes(picturePath)))
            {
                Debug.Log($"Image captured and loaded successfully. Size: {loadTest.width}x{loadTest.height}");
            }
            else
            {
                Debug.LogError("Failed to load captured image.");
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"An error occurred while capturing the photo: {ex.Message}");
        }

        isCapturing = false;
    }

    //Deleted bunch of rotation logic here 

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

    public void ClearPicturePath()
    {
        picturePath = null;
    }

    public void ReinitializeCamera()
    {
        InitializeCamera();
    }
}
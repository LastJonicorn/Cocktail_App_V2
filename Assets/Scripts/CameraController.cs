using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.IO;

public class CameraController : MonoBehaviour
{
    public RawImage cameraRawImage;
    private WebCamTexture webCamTexture;
    private string picturePath;

    public void InitializeCamera()
    {
        if (webCamTexture != null)
        {
            webCamTexture.Stop();
            webCamTexture = null;
        }

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
            Debug.Log("Camera successfully initialized");
            AdjustCameraOrientation();
        }
        else
        {
            Debug.LogError("Failed to initialize camera");
        }
    }

    private void AdjustCameraOrientation()
    {
        int rotationAngle = -webCamTexture.videoRotationAngle;
        bool isVertical = webCamTexture.videoVerticallyMirrored;

        // Adjust the rotation and flipping based on the camera orientation
        cameraRawImage.rectTransform.localEulerAngles = new Vector3(0, 0, rotationAngle);

        if (isVertical)
        {
            cameraRawImage.rectTransform.localScale = new Vector3(1, -1, 1);
        }
        else
        {
            cameraRawImage.rectTransform.localScale = new Vector3(1, 1, 1);
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
        if (webCamTexture != null && webCamTexture.isPlaying)
        {
            StartCoroutine(CapturePhoto());
        }
        else
        {
            Debug.LogError("Webcam is not running, cannot take picture.");
        }
    }

    private IEnumerator CapturePhoto()
    {
        yield return new WaitForEndOfFrame();

        if (webCamTexture == null || !webCamTexture.isPlaying)
        {
            Debug.LogError("Webcam is not running, cannot capture photo.");
            yield break;
        }

        Texture2D photo = new Texture2D(webCamTexture.width, webCamTexture.height, TextureFormat.RGB24, false);
        photo.SetPixels32(webCamTexture.GetPixels32());
        photo.Apply();

        Texture2D squarePhoto = ResizeTexture(photo, 500, 500);

        byte[] bytes = squarePhoto.EncodeToPNG();
        string newPicturePath = Path.Combine(Application.persistentDataPath, System.Guid.NewGuid().ToString() + ".png");
        File.WriteAllBytes(newPicturePath, bytes);

        picturePath = newPicturePath;

        Debug.Log("Photo saved to: " + picturePath);

        Destroy(photo);
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

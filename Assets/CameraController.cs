using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class CameraController : MonoBehaviour
{
    public RawImage rawImage;
    private WebCamTexture webCamTexture;

    void Start()
    {
        // Start the camera
        webCamTexture = new WebCamTexture();
        rawImage.texture = webCamTexture;
        rawImage.material.mainTexture = webCamTexture;
        webCamTexture.Play();

        // Set the RawImage dimensions to 500x500
        rawImage.rectTransform.sizeDelta = new Vector2(500, 500);
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
    }

    private Texture2D CropToSquare(Texture2D original)
    {
        int size = Mathf.Min(original.width, original.height);
        int x = (original.width - size) / 2;
        int y = (original.height - size) / 2;

        Texture2D cropped = new Texture2D(size, size);
        cropped.SetPixels(original.GetPixels(x, y, size, size));
        cropped.Apply();

        // Resize the cropped image to 500x500
        Texture2D resizedCropped = ResizeTexture(cropped, 500, 500);

        return resizedCropped;
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
}

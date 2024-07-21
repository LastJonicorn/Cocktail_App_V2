using UnityEngine;
using UnityEngine.Android;

public class PermissionsRequester : MonoBehaviour
{
    void Start()
    {
        // Check if the camera permission is not granted
        if (!Permission.HasUserAuthorizedPermission(Permission.Camera))
        {
            // Request the camera permission
            Permission.RequestUserPermission(Permission.Camera);
        }

        // Check if the write external storage permission is not granted
        if (!Permission.HasUserAuthorizedPermission(Permission.ExternalStorageWrite))
        {
            // Request the write external storage permission
            Permission.RequestUserPermission(Permission.ExternalStorageWrite);
        }

        // Check if the write external storage permission is not granted
        if (!Permission.HasUserAuthorizedPermission(Permission.ExternalStorageRead))
        {
            // Request the write external storage permission
            Permission.RequestUserPermission(Permission.ExternalStorageRead);
        }
    }
}

using UnityEngine;

public class PrefabManager : MonoBehaviour
{
    // Reference to the current prefab instance
    public GameObject currentPrefabInstance;

    // Array or List to hold multiple prefabs
    public GameObject[] prefabs;

    // Method to disable current prefab and instantiate a new one by index
    public void SwapPrefabByIndex(int index)
    {
        if (index < 0 || index >= prefabs.Length)
        {
            Debug.LogError("Prefab index out of range");
            return;
        }

        if (currentPrefabInstance != null)
        {
            // Disable the current prefab instance
            currentPrefabInstance.SetActive(false);
        }

        // Instantiate the new prefab
        GameObject newPrefabInstance = Instantiate(prefabs[index]);

        // Optionally, set the position and rotation of the new prefab instance
        newPrefabInstance.transform.position = Vector3.zero;
        newPrefabInstance.transform.rotation = Quaternion.identity;

        // Update the reference to the current prefab instance
        currentPrefabInstance = newPrefabInstance;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1)) // Press 1 to instantiate the first prefab
        {
            SwapPrefabByIndex(0);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2)) // Press 2 to instantiate the second prefab
        {
            SwapPrefabByIndex(1);
        }
        else if (Input.GetKeyDown(KeyCode.A)) // Press A to instantiate prefab by name
        {
            SwapPrefabByName("PrefabAName");
        }
    }


    // Method to disable current prefab and instantiate a new one by name
    public void SwapPrefabByName(string prefabName)
    {
        GameObject prefabToInstantiate = null;

        foreach (GameObject prefab in prefabs)
        {
            if (prefab.name == prefabName)
            {
                prefabToInstantiate = prefab;
                break;
            }
        }

        if (prefabToInstantiate == null)
        {
            Debug.LogError("Prefab not found: " + prefabName);
            return;
        }

        if (currentPrefabInstance != null)
        {
            // Disable the current prefab instance
            currentPrefabInstance.SetActive(false);
        }

        // Instantiate the new prefab
        GameObject newPrefabInstance = Instantiate(prefabToInstantiate);

        // Optionally, set the position and rotation of the new prefab instance
        newPrefabInstance.transform.position = Vector3.zero;
        newPrefabInstance.transform.rotation = Quaternion.identity;

        // Update the reference to the current prefab instance
        currentPrefabInstance = newPrefabInstance;
    }
}

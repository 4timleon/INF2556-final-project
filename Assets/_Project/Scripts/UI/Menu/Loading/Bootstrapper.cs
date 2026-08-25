using UnityEngine;

public class Bootstrapper : MonoBehaviour {

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    public static void InitializeMasterSystems() {

        if (FindFirstObjectByType<LoadingManager>() != null) return;

        GameObject managerPrefab = Resources.Load<GameObject>("MasterLoadingManager");
        if (managerPrefab == null) {
            Debug.LogError("Master Loading Manager not found in Resources folder");
            return;
        }

        GameObject managerInstance = Instantiate(managerPrefab);
        DontDestroyOnLoad(managerInstance);
    }
}

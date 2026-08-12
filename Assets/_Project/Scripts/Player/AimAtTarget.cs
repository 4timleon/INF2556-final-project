using UnityEngine;

public class AimAtTarget : MonoBehaviour {

    [SerializeField] private Camera mainCamera;
    [SerializeField] private PlayerInputHandler playerInputHandler;

    private void Update() {
        Vector3 centerOfScreen = new Vector3(Screen.width * 0.5f, Screen.height * 0.5f, 0f);
        Ray ray = mainCamera.ScreenPointToRay(centerOfScreen);
        if (Physics.Raycast(ray, out RaycastHit hit)) {
            transform.position = hit.point;
        }
    }

}

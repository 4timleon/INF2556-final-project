using UnityEngine;

public class BodyPart : MonoBehaviour {

    [SerializeField] private DetachController detachController;
    [SerializeField] private Detach detachableBodyPart;

    public void TakeDamage(float value) {
        detachController.Hit(detachableBodyPart, value);
    }
}

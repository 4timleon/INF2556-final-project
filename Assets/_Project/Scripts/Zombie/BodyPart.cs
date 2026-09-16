using UnityEngine;

public class BodyPart : MonoBehaviour
{

    [SerializeField] private Detach detachableBodyPart;

    private DetachController detachController;

    private void Start()
    {
        detachController = GetComponentInParent<DetachController>();
    }

    public void TakeDamage(float value)
    {
        detachController.Hit(detachableBodyPart, value);
    }
}

using UnityEngine;

public class BodyPart : MonoBehaviour
{
    [SerializeField] private Detach detachableBodyPart;

    private Collider bodyPartCollider;
    private DetachController detachController;

    private void Start()
    {
        bodyPartCollider = GetComponent<Collider>();
        detachController = GetComponentInParent<DetachController>();
        bodyPartCollider.enabled = true;
    }

    public void TakeDamage(float value, Vector3 direction)
    {
        detachController.Hit(detachableBodyPart, value, direction, bodyPartCollider);
    }
}

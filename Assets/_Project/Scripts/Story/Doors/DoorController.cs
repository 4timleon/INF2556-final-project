using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(BoxCollider))]
public class DoorController : MonoBehaviour, IInteractable
{

    [SerializeField] private NavMeshObstacle obstacle;

    [Header("Door Settings")]
    [SerializeField] private string doorId;
    [SerializeField] private int cost = 500;
    [SerializeField] private GameObject rotationObject;
    [SerializeField] private Vector3 closedEuler = new Vector3(0, -90, 0f);
    [SerializeField] private Vector3 openedEuler = new Vector3(0, 0f, 0f);

    [Header("SpawnPoints References")]
    public SpawnPoint[] spawnPoints;

    void Awake()
    {
        ToggleDoor(false);
        obstacle.enabled = true;
    }

    public void OnInteractStarted() => TryOpenDoor();

    public string GetText() => $"Press <sprite name=\"keyboard_e\"> to open Door | {cost}$";

    private void TryOpenDoor()
    {
        if (GameData.Money < cost) return;

        GameData.Money -= cost;
        gameObject.layer = 0;
        OpenDoor();
    }

    private void OpenDoor()
    {
        ToggleDoor(true);
        GameEventSystem.OnDoorOpened?.Invoke(this);
        obstacle.enabled = false;
    }

    private void ToggleDoor(bool toggle)
    {
        rotationObject.LeanCancel();

        Quaternion startRot = rotationObject.transform.localRotation;
        Quaternion targetRot = Quaternion.Euler(toggle ? openedEuler : closedEuler);

        LeanTween.value(rotationObject, 0f, 1f, 1f)
            .setEase(LeanTweenType.easeInOutQuad)
            .setOnUpdate((float t) =>
            {
                rotationObject.transform.localRotation = Quaternion.Slerp(startRot, targetRot, t);
            });
    }
}
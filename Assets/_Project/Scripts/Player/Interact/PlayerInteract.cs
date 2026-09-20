using System;
using EditorAttributes;
using UnityEngine;

public class PlayerInteract : MonoBehaviour
{
    [Title("References", 14)]
    [SerializeField] private Camera playerCamera;
    [SerializeField] private float interactDistance = 3f;
    [SerializeField] private LayerMask interactableLayer;

    private IInteractable currentInteractable;
    private bool isSmoking = false;
    private bool isDowned = false;

    void OnEnable()
    {
        GameEventSystem.OnPlayerInteractStarted += Event_PlayerInteractStarted;
        GameEventSystem.OnPlayerInteractEnded += Event_PlayerInteractEnded;
        GameEventSystem.OnPlayerSmokeJointStarted += Event_JointStarted;
        GameEventSystem.OnPlayerSmokeJointEnded += Event_JointEnded;
        GameEventSystem.OnPlayerPreDeath += Event_PreDeath;
    }

    void OnDisable()
    {
        GameEventSystem.OnPlayerInteractStarted -= Event_PlayerInteractStarted;
        GameEventSystem.OnPlayerInteractEnded -= Event_PlayerInteractEnded;
        GameEventSystem.OnPlayerSmokeJointStarted -= Event_JointStarted;
        GameEventSystem.OnPlayerSmokeJointEnded -= Event_JointEnded;
        GameEventSystem.OnPlayerPreDeath -= Event_PreDeath;
    }

    void Event_JointStarted() => isSmoking = true;
    void Event_JointEnded() => isSmoking = false;
    private void Event_PreDeath(bool obj) => isDowned = obj;

    void Update()
    {
        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);

        if (Physics.Raycast(ray, out RaycastHit hit, interactDistance, interactableLayer) && !isSmoking && !isDowned)
        {
            if (hit.collider.TryGetComponent(out IInteractable interactable))
            {
                currentInteractable = interactable;
                GameEventSystem.OnInteractionEnter?.Invoke(interactable);
                return;
            }
        }

        if (currentInteractable != null)
        {
            currentInteractable.OnInteractEnded();
            GameEventSystem.OnInteractionExit?.Invoke();
        }

        currentInteractable = null;
    }

    void Event_PlayerInteractStarted()
    {
        if (currentInteractable == null || isSmoking || isDowned) return;

        currentInteractable.OnInteractStarted();
    }

    void Event_PlayerInteractEnded()
    {
        if (currentInteractable == null) return;

        currentInteractable.OnInteractEnded();
    }
}

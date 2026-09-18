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

    void OnEnable()
    {
        GameEventSystem.OnPlayerInteractStarted += Event_PlayerInteractStarted;
        GameEventSystem.OnPlayerInteractEnded += Event_PlayerInteractEnded;
    }

    void OnDisable()
    {
        GameEventSystem.OnPlayerInteractStarted -= Event_PlayerInteractStarted;
        GameEventSystem.OnPlayerInteractEnded -= Event_PlayerInteractEnded;
    }

    void Update()
    {
        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);

        if (Physics.Raycast(ray, out RaycastHit hit, interactDistance, interactableLayer))
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
            GameEventSystem.OnInteractionExit?.Invoke();
        }

        currentInteractable = null;
    }

    void Event_PlayerInteractStarted()
    {
        if (currentInteractable == null) return;

        currentInteractable.OnInteractStarted();
    }

    void Event_PlayerInteractEnded()
    {
        if (currentInteractable == null) return;

        currentInteractable.OnInteractEnded();
    }
}

using System;
using TMPro;
using UnityEngine;

public class InteractUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI interactText;

    private void OnEnable()
    {
        GameEventSystem.OnInteractionEnter += Event_InteractionEnter;
        GameEventSystem.OnInteractionExit += Event_InteractionExit;
    }

    private void OnDisable()
    {
        GameEventSystem.OnInteractionEnter -= Event_InteractionEnter;
        GameEventSystem.OnInteractionExit -= Event_InteractionExit;
    }

    private void Start()
    {
        interactText.gameObject.SetActive(false);
    }

    private void Event_InteractionEnter(IInteractable interactable)
    {
        interactText.gameObject.SetActive(true);
        interactText.text = interactable.GetText();
    }

    private void Event_InteractionExit()
    {
        interactText.gameObject.SetActive(false);
    }
}

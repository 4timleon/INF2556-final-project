using UnityEngine;

public class BarricadeInteract : MonoBehaviour, IInteractable
{
    [SerializeField] private BarricadeController barricadeController;

    public string GetText() => barricadeController.GetText();
    public void OnInteractStarted() => barricadeController.OnInteractStarted();
    public void OnInteractEnded() => barricadeController.OnInteractEnded();
}

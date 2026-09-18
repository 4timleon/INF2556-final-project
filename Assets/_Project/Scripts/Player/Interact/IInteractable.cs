using UnityEngine;

public interface IInteractable
{
    void OnInteractStarted();
    void OnInteractEnded() { }
    string GetText();
}

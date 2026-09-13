using System;
using UnityEngine;

public class InteractUI : MonoBehaviour
{
    private void Start()
    {
        gameObject.SetActive(false);
    }

    private void ShowInteract(bool state)
    {
        gameObject.SetActive(state);
    }

}

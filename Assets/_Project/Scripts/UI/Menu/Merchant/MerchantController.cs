using UnityEngine;

public class MerchantController : MonoBehaviour, IInteractable
{
    [SerializeField] private MenuManager menuManager;
    [SerializeField] private MerchantUI merchantUI;

    void Start()
    {
        firstOpen = true;
    }

    public string GetText() => "Press <sprite name=\"keyboard_e\"> to open Shop";

    private bool firstOpen;

    public void OnInteractStarted()
    {
        menuManager.OpenMerchantMenu();

        if (firstOpen)
        {
            merchantUI.RerollWeaponCards(true);
            firstOpen = false;
        }
    }
}

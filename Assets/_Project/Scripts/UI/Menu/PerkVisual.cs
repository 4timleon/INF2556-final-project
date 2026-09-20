using UnityEngine;

public class PerkVisual : MonoBehaviour
{

    [SerializeField] private GameObject perkPrefab;

    private int applyHash = Animator.StringToHash("Apply");

    public GameObject SpawnAndAnimatePerk(Transform parent)
    {
        GameObject perkInstance = Instantiate(perkPrefab, parent);
        perkInstance.GetComponent<Animator>().SetTrigger(applyHash);

        return perkInstance;
    }
}

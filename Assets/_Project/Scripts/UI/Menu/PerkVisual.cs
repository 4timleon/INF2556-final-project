using UnityEngine;

public class PerkVisual : MonoBehaviour {

    [SerializeField] private GameObject perkPrefab;

    private int applyHash = Animator.StringToHash("Apply");

    public void SpawnAndAnimatePerk(Transform parent) {
        GameObject skullInstance = Instantiate(perkPrefab, parent);
        skullInstance.GetComponent<Animator>().SetTrigger(applyHash);
    }
}

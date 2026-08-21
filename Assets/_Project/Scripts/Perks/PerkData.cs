using UnityEngine;

[CreateAssetMenu(fileName = "PerkData", menuName = "Scriptable Objects/PerkData")]
public abstract class PerkData : ScriptableObject
{
    [Header("Perk Info")]
    public string perkId;
    public string perkName;
    [TextArea] public string description;
    public Sprite icon;
    public int cost;

    public abstract void ApplyPerk(GameObject player);

    public abstract void RemovePerk(GameObject player);
}

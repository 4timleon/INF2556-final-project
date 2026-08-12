using EditorAttributes;
using UnityEngine;

[CreateAssetMenu(fileName = "NewStatType", menuName = "Weapon/Stat Type")]
public class StatTypeSO : ScriptableObject
{
    public string statName;
    public float defaultValue;
}

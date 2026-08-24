using UnityEngine;
using UnityEngine.InputSystem;

public enum PerkType {
    Juggernaut,
    Skull
}

public class PerksUI : MonoBehaviour {

    [SerializeField] private PerkVisual juggernaut;
    [SerializeField] private PerkVisual skull;

    private void ApplyPerkAnimation(PerkType perk) {
        switch (perk) {
            case PerkType.Juggernaut:
                juggernaut.SpawnAndAnimatePerk(transform);
                break;
            case PerkType.Skull:
                skull.SpawnAndAnimatePerk(transform);
                break;
        }
    }

    private void Update() {
        if (Keyboard.current.digit1Key.wasPressedThisFrame) {
            ApplyPerkAnimation(PerkType.Juggernaut);
        }

        if (Keyboard.current.digit2Key.wasPressedThisFrame) {
            ApplyPerkAnimation(PerkType.Skull);
        }

    }
}

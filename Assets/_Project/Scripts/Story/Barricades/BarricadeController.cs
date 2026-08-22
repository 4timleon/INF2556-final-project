using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class BarricadeController : MonoBehaviour {
    [Header("Settings")]
    [SerializeField] private int plankCount = 5;
    [SerializeField] private float repairAnimationDuration = 1f;

    [Header("References")]
    [SerializeField] private Animator[] plankAnimators;
    [SerializeField] private Rigidbody[] rigidbodies;

    private bool[] plankBroken;
    private float cooldownTimer;
    private bool playerInRange;

    private void Awake() {
        plankBroken = new bool[plankCount];

        foreach (Rigidbody rb in rigidbodies)
            rb.isKinematic = true;
    }

    private void Update() {
        if (!playerInRange) return;

        if (cooldownTimer > 0f) {
            cooldownTimer -= Time.deltaTime;
            return;
        }

        int targetIndex = GetLowestBrokenPlankIndex();
        if (targetIndex == -1) return; // All planks are repaired

        var interact = GameControls.GetInputActions().Player.Interact;

        //Hold or press to repair
        if (interact.IsPressed() || interact.WasPressedThisFrame()) {
            RepairPlank(targetIndex);
            cooldownTimer = repairAnimationDuration;
        }
    }

    private int GetLowestBrokenPlankIndex() {
        for (int i = 0; i < plankBroken.Length; i++) {
            if (plankBroken[i]) return i;
        }
        return -1;
    }

    private void RepairPlank(int index) {
        plankBroken[index] = false;

        foreach (Rigidbody rb in rigidbodies)
            rb.isKinematic = true;

        if (plankAnimators != null && index < plankAnimators.Length && plankAnimators[index] != null)
            plankAnimators[index].SetTrigger("Repair");
    }

    public void BreakNextPlank() {
        for (int i = plankBroken.Length - 1; i >= 0; i--) {
            if (!plankBroken[i]) {
                plankBroken[i] = true;

                rigidbodies[i].isKinematic = false;
                rigidbodies[i].AddForce(-transform.forward * 2.5f, ForceMode.Impulse);

                return;
            }
        }
    }

    public bool GetAllPlanksDestroyed() {
        for (int i = 0; i < plankBroken.Length; i++) {
            if (!plankBroken[i]) return false;
        }

        return true;
    }

    private void OnTriggerEnter(Collider other) {
        if (other.CompareTag("Player"))
            playerInRange = true;

        Zombie zombie = other.GetComponentInParent<Zombie>();
        if (zombie != null) {
            zombie.BarricadeReached(this);
        }
    }

    private void OnTriggerExit(Collider other) {
        if (other.CompareTag("Player"))
            playerInRange = false;

        Zombie zombie = other.GetComponentInParent<Zombie>();
        if (zombie != null && !zombie.IsAttackingBarricade) {
            zombie.ClearTargetBarricade();
        }
    }
}
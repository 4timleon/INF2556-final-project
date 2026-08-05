using UnityEngine;
using UnityEngine.InputSystem;

public enum BodyPartType {
    Head,
    Body,
    BodyTop,
    Hip,
    LArmPalm,
    LForearm,
    LUpperarm,
    LLegCalf,
    LLegThigh,
    RArmPalm,
    RForearm,
    RUpperarm,
    RLegCalf,
    RLegThigh,
}

public class DetachController : MonoBehaviour {

    [SerializeField] private Detach[] detachables;

    private void Update() {
        if (!Keyboard.current.spaceKey.wasPressedThisFrame) return;

        Detach randomBodyPart = detachables[Random.Range(0, detachables.Length)];
        randomBodyPart.DetachMesh();
    }

    public void BodyPartDetach(BodyPartType bodyPartType, Vector3 pushDirection) {
        switch (bodyPartType) {
            case BodyPartType.Head:
                break;
            case BodyPartType.Body:
                break;
            case BodyPartType.BodyTop:
                break;
            case BodyPartType.Hip:
                break;
            case BodyPartType.LArmPalm:
                break;
            case BodyPartType.LForearm:
                break;
            case BodyPartType.LUpperarm:
                break;
            case BodyPartType.LLegCalf:
                break;
            case BodyPartType.LLegThigh:
                break;
            case BodyPartType.RArmPalm:
                break;
            case BodyPartType.RForearm:
                break;
            case BodyPartType.RUpperarm:
                break;
            case BodyPartType.RLegCalf:
                break;
            case BodyPartType.RLegThigh:
                break;
        }
    }
}

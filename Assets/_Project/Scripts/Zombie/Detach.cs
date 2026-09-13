using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class Detach : MonoBehaviour
{

    [SerializeField] private SkinnedMeshRenderer mesh;
    [SerializeField] private BodyPartType bodyPart;
    [SerializeField] private DetachedBodyPart detachedPrefab;
    [SerializeField] private float pushForce = 3f;
    [SerializeField] private Vector3 pushDirection = new Vector3(-1f, 0.2f, 0f);

    public float DamageTaken { get; private set; }
    public bool IsDetached { get; private set; } = false;
    public BodyPartType BodyPartType => bodyPart;

    //Hit Variables
    private Color originalColor;
    private Material zombieMaterial;
    private readonly Color hitColor = Color.red;

    //Dissolve
    private float dissolveMeter = 1f;
    private float dissolveTime = 1.5f;
    private float dissolveDelay = 1f;

    private Coroutine hitfeedback;

    private void Start()
    {
        if (mesh != null)
        {
            zombieMaterial = mesh.material;
            originalColor = zombieMaterial.color;
        }
    }

    public void TakeBodyPartDamage(float value)
    {
        DamageTaken += value;

        if (hitfeedback != null)
        {
            StopCoroutine(hitfeedback);
        }
        hitfeedback = StartCoroutine(HitFeedback());
    }

    private IEnumerator HitFeedback()
    {
        zombieMaterial.color = hitColor;
        yield return new WaitForSeconds(0.1f);
        zombieMaterial.color = originalColor;
    }

    public void DetachMesh()
    {
        mesh.enabled = false;

        DetachedBodyPart newMesh = Instantiate(detachedPrefab, mesh.bounds.center, mesh.rootBone.rotation);
        Rigidbody rb = newMesh.GetComponentInChildren<Rigidbody>();

        if (rb != null)
        {
            rb.AddForce(pushDirection.normalized * pushForce, ForceMode.Impulse);
        }

        IsDetached = true;

        StartCoroutine(newMesh.Dissolve());
    }

    private void DetachMesh(Vector3 pushDirection)
    {
        mesh.enabled = false;

        DetachedBodyPart newMesh = Instantiate(detachedPrefab, mesh.bounds.center, mesh.rootBone.rotation);
        Rigidbody rb = newMesh.GetComponentInChildren<Rigidbody>();

        if (rb != null)
        {
            rb.AddForce(pushDirection.normalized * pushForce, ForceMode.Impulse);
        }

        IsDetached = true;

        StartCoroutine(newMesh.Dissolve());
    }

    public IEnumerator DissolveBodyPart()
    {
        if (IsDetached) yield break;

        yield return new WaitForSeconds(dissolveDelay);

        if (IsLeanTweenFull())
        {
            LeanTween.cancel(gameObject);
            Destroy(gameObject, 2f);
        }

        LeanTween.value(gameObject, dissolveMeter, 0f, dissolveTime)
            .setOnUpdate((float value) =>
            {
                zombieMaterial.SetFloat("_DissolveMeter", value);
            })
            .setDestroyOnComplete(true);
    }

    public bool IsLeanTweenFull()
    {
        return LeanTween.tweensRunning >= LeanTween.maxSimulataneousTweens;
    }

    private void OnDestroy()
    {
        LeanTween.cancel(gameObject);
    }

}

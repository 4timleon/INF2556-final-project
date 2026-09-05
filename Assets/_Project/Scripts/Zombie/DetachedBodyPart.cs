using System.Collections;
using UnityEngine;

public class DetachedBodyPart : MonoBehaviour
{
    [SerializeField] private MeshRenderer meshRenderer;
    [SerializeField] private ParticleSystem dissolveParticle;

    //Dissolve
    private float dissolveMeter = 1f;
    private float dissolveTime = 1.5f;
    private float dissolveDelay = 1f;

    public IEnumerator Dissolve()
    {
        yield return new WaitForSeconds(dissolveDelay);

        dissolveParticle.Play();

        Material targetMaterial = meshRenderer.material;

        if (IsLeanTweenFull())
        {
            LeanTween.cancel(gameObject);
            Destroy(gameObject, 2f);
        }

        LeanTween.value(gameObject, dissolveMeter, 0f, dissolveTime)
            .setOnUpdate((float value) =>
            {
                targetMaterial.SetFloat("_DissolveMeter", value);
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

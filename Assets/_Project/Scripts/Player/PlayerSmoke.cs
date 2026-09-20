using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Animations.Rigging;

public class PlayerSmoke : MonoBehaviour
{
    [SerializeField] private GameObject jointPrefab;
    [SerializeField] private Transform jointSocket;
    [SerializeField] private ParticleSystem smokeParticles;
    [SerializeField] private Rig[] rigs;

    [SerializeField] private Animator animator;
    [SerializeField] private string smokeAnimationName;
    [SerializeField] private string idleAnimationName;
    [SerializeField] private int animationLayer;

    private int smokeAnimationHash;
    private int idleAnimationHash;
    private GameObject jointInstance;
    private WeaponController weaponController;

    void Start()
    {
        smokeAnimationHash = Animator.StringToHash(smokeAnimationName);
        idleAnimationHash = Animator.StringToHash(idleAnimationName);

        SpawnJoint();
    }

    void OnEnable()
    {
        GameEventSystem.OnPlayerWeaponDraw += Event_WeaponDraw;
    }

    void OnDisable()
    {
        GameEventSystem.OnPlayerWeaponDraw -= Event_WeaponDraw;
    }

    private void Event_WeaponDraw(WeaponController weaponController) => this.weaponController = weaponController;

    public void SmokeJoint(Action onComplete, AudioSource audioSource)
    {
        if (weaponController == null || weaponController.IsLocked) return;

        StartCoroutine(JointRoutine(onComplete, audioSource));
    }

    IEnumerator JointRoutine(Action onComplete, AudioSource audioSource)
    {
        GameEventSystem.OnPlayerSmokeJointStarted?.Invoke();

        smokeParticles.Stop();
        weaponController.IsLocked = true;

        bool smokeFinished = false;

        animator.CrossFadeWithCallback(this, smokeAnimationHash, .25f, animationLayer).SetOnComplete(() => smokeFinished = true);

        yield return new WaitForSeconds(.5f);

        ToggleRigs(0f);

        jointInstance.SetActive(true);
        weaponController.gameObject.SetActive(false);

        yield return new WaitForSeconds(0.5f);

        if (audioSource.isPlaying)
        {
            audioSource.Stop();
        }
        SoundManager.Instance?.PlaySmokeSound(audioSource);

        yield return new WaitForSeconds(1.5f);

        ToggleRigs(1f);

        jointInstance.SetActive(false);
        weaponController.gameObject.SetActive(true);

        yield return new WaitUntil(() => smokeFinished);

        onComplete?.Invoke();
        GameEventSystem.OnPlayerSmokeJointEnded?.Invoke();

        animator.CrossFadeInFixedTime(idleAnimationHash, .15f, animationLayer);

        weaponController.IsLocked = false;
        smokeParticles.Play();
    }

    void SpawnJoint()
    {
        jointInstance = Instantiate(jointPrefab, Vector3.zero, Quaternion.identity);
        jointInstance.transform.SetParent(jointSocket);
        jointInstance.transform.localPosition = Vector3.zero;
        jointInstance.transform.localRotation = Quaternion.identity;

        jointInstance.SetActive(false);
    }

    void ToggleRigs(float to)
    {
        LeanTween.value(gameObject, rigs[0].weight, to, .25f).setOnUpdate((value) =>
        {
            for (int i = 0; i < rigs.Length; i++)
            {
                rigs[i].weight = value;
            }
        });
    }
}

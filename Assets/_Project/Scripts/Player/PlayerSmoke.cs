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
        GameEventSystem.OnPlayerSmokeJointStarted += Event_SmokeJointStarted;
        GameEventSystem.OnPlayerWeaponDraw += Event_WeaponDraw;
    }

    void OnDisable()
    {
        GameEventSystem.OnPlayerSmokeJointStarted -= Event_SmokeJointStarted;
        GameEventSystem.OnPlayerWeaponDraw -= Event_WeaponDraw;
    }

    void Update()
    {
        if (GameControls.GetInputActions().Player.Crouch.WasPerformedThisFrame())
        {
            GameEventSystem.OnPlayerSmokeJointStarted?.Invoke();
        }
    }

    private void Event_WeaponDraw(WeaponController weaponController) => this.weaponController = weaponController;

    void Event_SmokeJointStarted()
    {
        if (weaponController == null || weaponController.IsLocked) return;

        StartCoroutine(SmokeJoint());
    }

    IEnumerator SmokeJoint()
    {
        smokeParticles.Stop();
        weaponController.IsLocked = true;

        bool smokeFinished = false;

        animator.CrossFadeWithCallback(this, smokeAnimationHash, .25f, animationLayer).SetOnComplete(() => smokeFinished = true);

        yield return new WaitForSeconds(.5f);

        ToggleRigs(0f);

        jointInstance.SetActive(true);
        weaponController.gameObject.SetActive(false);

        yield return new WaitForSeconds(2f);

        ToggleRigs(1f);

        jointInstance.SetActive(false);
        weaponController.gameObject.SetActive(true);

        yield return new WaitUntil(() => smokeFinished);

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

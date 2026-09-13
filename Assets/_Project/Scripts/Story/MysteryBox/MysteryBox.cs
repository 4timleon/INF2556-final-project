using System.Collections;
using System.Collections.Generic;
using EditorAttributes;
using UnityEngine;
using Random = UnityEngine.Random;

public class MysteryBox : MonoBehaviour
{
    [Title("References:", 14)]
    [SerializeField] private WeaponBuilder weaponBuilder;
    [SerializeField] private PlayerArmory playerArmory;
    [SerializeField] private GameObject djKhaled;
    [SerializeField] private AudioSource audioSource;
    [Space]
    [Title("Settings:", 14)]
    [SerializeField] private Vector3 closedEuler = new Vector3(-90f, 0f, 0f);
    [SerializeField] private Vector3 openedEuler = new Vector3(-175f, 0f, 0f);
    [Space]
    [SerializeField] private float cost = 0;
    [SerializeField] private float waitPerWeapon = 0.5f;
    [Space]
    [Title("Transforms:", 14)]
    [SerializeField] private GameObject lidObject;
    [SerializeField] private Transform weaponHolder;

    public Transform SpawnPoint { get; set; }

    private bool canOpen;
    private bool canOptain;

    private GameObject currentVisible;

    private List<GameObject> weaponPool;

    private Coroutine currentBoxRoutine;

    void Start()
    {
        weaponPool = new();
        canOpen = true;
        canOptain = false;
        currentVisible = null;
        currentBoxRoutine = null;
        djKhaled.SetActive(false);
    }

    public void TryBuyBox()
    {
        if (GameData.Money < cost || canOpen == false) return;

        GenerateWeapons();

        currentBoxRoutine = StartCoroutine(BoxRoutine());
    }

    public void OptainWeapon()
    {
        if (canOptain == false || currentVisible == null) return;

        playerArmory.AddWeapon(currentVisible.GetComponent<WeaponController>());
        weaponPool.Remove(currentVisible);
        CancelBoxRoutine();
    }

    IEnumerator BoxRoutine()
    {
        weaponHolder.transform.localPosition = Vector3.zero;

        canOpen = false;

        ToggleLid(true);

        bool lifeIsRoblox = false;

        weaponHolder.LeanMoveLocalY(1.5f, waitPerWeapon * weaponPool.Count);

        for (int i = 0; i < weaponPool.Count; i++)
        {
            if (currentVisible != null) currentVisible.SetActive(false);

            if (i >= weaponPool.Count - 1)
            {
                if (Random.Range(0, 100) <= 10)
                {
                    lifeIsRoblox = true;
                    djKhaled.SetActive(true);
                    audioSource.Play();
                    break;
                }
            }

            if (!lifeIsRoblox)
            {
                currentVisible = weaponPool[i];

                weaponPool[i].SetActive(true);
                yield return new WaitForSeconds(waitPerWeapon);
            }
        }

        yield return new WaitForSeconds(0.1f);

        if (lifeIsRoblox)
        {
            yield return new WaitForSeconds(2f);

            KhaledTrigger();
            yield break;
        }

        canOptain = true;

        bool optainDelay = false;

        weaponHolder.LeanMoveLocalY(0f, 5f).setEaseInCubic().setOnComplete(() => optainDelay = true);

        yield return new WaitUntil(() => optainDelay);

        CancelBoxRoutine();
    }

    void KhaledTrigger()
    {
        gameObject.LeanMoveLocalY(transform.position.y + 100f, 5f).setOnComplete(() =>
        {
            GameEventSystem.OnMysteryBoxDisappear?.Invoke(SpawnPoint);
            CancelBoxRoutine();
        });
    }

    void CancelBoxRoutine()
    {
        if (currentBoxRoutine != null)
        {
            StopCoroutine(currentBoxRoutine);
        }

        LeanTween.cancel(weaponHolder.gameObject);

        ToggleLid(false);

        currentVisible.SetActive(false);
        currentVisible = null;

        djKhaled.SetActive(false);

        canOpen = true;
        canOptain = false;

        GenerateWeapons();
    }

    void GenerateWeapons()
    {
        foreach (GameObject obj in weaponPool)
        {
            Destroy(obj);
        }

        weaponPool.Clear();

        for (int i = 0; i < 10; i++)
        {
            WeaponController weapon = weaponBuilder.BuildWeapon();

            weapon.gameObject.SetActive(false);
            weapon.transform.SetParent(weaponHolder);
            weapon.transform.localPosition = Vector3.zero;
            weapon.transform.localRotation = Quaternion.identity;
            weapon.transform.localScale = Vector3.one * 0.2f;

            weapon.GetComponent<WeaponController>().CenterOnSocket(weaponHolder);

            weaponPool.Add(weapon.gameObject);
        }
    }

    void ToggleLid(bool toggle)
    {
        lidObject.LeanCancel();

        Quaternion startRot = lidObject.transform.localRotation;
        Quaternion targetRot = Quaternion.Euler(toggle ? openedEuler : closedEuler);

        LeanTween.value(lidObject, 0f, 1f, 1f)
            .setEase(LeanTweenType.easeInOutQuad)
            .setOnUpdate((float t) =>
            {
                lidObject.transform.localRotation = Quaternion.Slerp(startRot, targetRot, t);
            });
    }
}

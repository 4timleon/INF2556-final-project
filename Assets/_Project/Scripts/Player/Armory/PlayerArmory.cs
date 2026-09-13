using System.Collections;
using System.Collections.Generic;
using EditorAttributes;
using UnityEngine;

public class PlayerArmory : MonoBehaviour
{
    [Title("Weapon Socket:", 14)]
    [SerializeField] private Transform rightHandSocket;
    [Space]
    [Title("Animation:", 14)]
    [SerializeField] private Animator animator;
    [SerializeField] private string holsterAnimationName;
    [SerializeField] private string movementAnimationName;
    [SerializeField] private int rightLayer;

    private WeaponController currentWeaponController;
    private int currentIndex;

    private int weaponControllersPointer;
    private int maxWeaponControllers;
    private List<WeaponController> weaponControllers;

    private int holsterAnimationHash;
    private int movementAnimationHash;

    public bool IsSwitching { get; private set; }

    void Awake()
    {
        weaponControllers = new();
        weaponControllersPointer = -1;
        currentIndex = -1;
        maxWeaponControllers = 2;

        for (int i = 0; i < maxWeaponControllers; i++)
        {
            weaponControllers.Add(null);
        }

        holsterAnimationHash = Animator.StringToHash(holsterAnimationName);
        movementAnimationHash = Animator.StringToHash(movementAnimationName);
    }

    void OnEnable()
    {
        GameEventSystem.OnPlayerWeaponScroll += Event_WeaponScroll;
    }

    void OnDisable()
    {
        GameEventSystem.OnPlayerWeaponScroll -= Event_WeaponScroll;
    }

    private void Event_WeaponScroll(Vector2 scrollValue)
    {
        if (IsSwitching || weaponControllersPointer <= 0 || currentWeaponController.IsLocked) return;

        float scrollY = scrollValue.y;
        int nextIndex = currentIndex;

        if (scrollY > 0f) // Scroll Up
        {
            nextIndex++;
            if (nextIndex > weaponControllersPointer)
            {
                nextIndex = 0;
            }
        }
        else if (scrollY < 0f) // Scroll Down
        {
            nextIndex--;

            if (nextIndex < 0)
            {
                nextIndex = weaponControllersPointer;
            }
        }

        if (nextIndex != currentIndex)
        {
            StartCoroutine(SwitchWeapon(nextIndex));
        }
    }

    public void SetMaxWeaponAmount(int amount)
    {
        maxWeaponControllers = amount;
    }

    public void AddWeapon(WeaponController weaponController)
    {
        if (weaponControllersPointer >= maxWeaponControllers - 1)
        {
            ReplaceWeapon(weaponController);
            return;
        }

        weaponControllersPointer++;
        weaponControllers[weaponControllersPointer] = weaponController;
        TransformWeapon(weaponController);

        StartCoroutine(SwitchWeapon(weaponControllersPointer));
        return;
    }

    void ReplaceWeapon(WeaponController weaponController)
    {
        if (currentIndex < 0 || currentIndex > weaponControllersPointer) return;

        weaponControllers[currentIndex] = weaponController;
        TransformWeapon(weaponController);

        StartCoroutine(SwitchWeapon(currentIndex, true));
    }

    void TransformWeapon(WeaponController weaponController)
    {
        weaponController.gameObject.SetActive(false);

        Transform weaponTransform = weaponController.transform;

        weaponTransform.SetParent(rightHandSocket);
        weaponTransform.localPosition = Vector3.zero;
        weaponTransform.localRotation = Quaternion.identity;
        weaponTransform.localScale = Vector3.one;
    }

    public IEnumerator SwitchWeapon(int toIndex, bool removePrevious = false)
    {
        if (!removePrevious && toIndex == currentIndex)
        {
            yield break;
        }

        if (toIndex < 0 || toIndex > weaponControllersPointer || weaponControllers[toIndex] == null)
        {
            yield break;
        }

        IsSwitching = true;

        if (currentWeaponController != null)
        {
            currentWeaponController.IsLocked = true;
            GameEventSystem.OnPlayerWeaponHolster?.Invoke();

            bool holsterFinished = false;
            animator.CrossFadeWithCallback(this, holsterAnimationHash, 0.25f, rightLayer)
                    .SetOnComplete(() => holsterFinished = true);

            yield return new WaitUntil(() => holsterFinished);

            currentWeaponController.IsLocked = false;

            if (removePrevious)
            {
                Destroy(currentWeaponController.gameObject);
            }
            else
            {
                currentWeaponController.gameObject.SetActive(false);
            }
        }

        currentWeaponController = weaponControllers[toIndex];
        currentIndex = toIndex;
        currentWeaponController.gameObject.SetActive(true);

        currentWeaponController.IsLocked = true;

        GameEventSystem.OnPlayerWeaponTransform?.Invoke(currentWeaponController);

        bool drawFinished = false;
        animator.CrossFadeWithCallback(this, movementAnimationHash, 0.25f, rightLayer, false)
                .SetOnComplete(() => drawFinished = true);

        yield return new WaitUntil(() => drawFinished);

        GameEventSystem.OnPlayerWeaponDraw?.Invoke(currentWeaponController);
        IsSwitching = false;
        currentWeaponController.IsLocked = false;
    }
}
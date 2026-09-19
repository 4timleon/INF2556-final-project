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
    [Space]
    [Title("Weapon Layer:", 14)]
    [SerializeField] private int weaponLayer;

    private WeaponController currentWeaponController;
    private int currentIndex;

    private int maxWeaponControllers;
    private List<WeaponController> weaponControllers;

    private int holsterAnimationHash;
    private int movementAnimationHash;

    public WeaponController CurrentWeaponController => currentWeaponController;
    public bool IsSwitching { get; private set; }

    void Awake()
    {
        weaponControllers = new List<WeaponController>();
        currentIndex = -1;
        maxWeaponControllers = 2;

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
        if (IsSwitching || weaponControllers.Count <= 1 || currentWeaponController.IsLocked) return;

        float scrollY = scrollValue.y;
        int nextIndex = currentIndex;

        if (scrollY > 0f) // Scroll Up
        {
            nextIndex++;
            if (nextIndex >= weaponControllers.Count)
            {
                nextIndex = 0;
            }
        }
        else if (scrollY < 0f) // Scroll Down
        {
            nextIndex--;
            if (nextIndex < 0)
            {
                nextIndex = weaponControllers.Count - 1;
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

        while (weaponControllers.Count > maxWeaponControllers)
        {
            RemoveWeapon(weaponControllers.Count - 1);
        }
    }

    public void AddWeapon(WeaponController weaponController)
    {
        if (weaponControllers.Count >= maxWeaponControllers)
        {
            ReplaceWeapon(weaponController);
            return;
        }

        weaponControllers.Add(weaponController);
        TransformWeapon(weaponController);

        StartCoroutine(SwitchWeapon(weaponControllers.Count - 1));
    }

    public void RemoveWeapon(int index)
    {
        if (index < 0 || index >= weaponControllers.Count) return;

        WeaponController weaponToRemove = weaponControllers[index];

        weaponControllers.RemoveAt(index);

        if (currentIndex == index)
        {
            currentIndex = -1;
            int nextIndex = weaponControllers.Count > 0 ? 0 : -1;

            if (nextIndex != -1)
            {
                StartCoroutine(SwitchWeapon(nextIndex, weaponToRemove));
            }
            else
            {
                StartCoroutine(HolsterAndDestroy(weaponToRemove));
            }
        }
        else
        {
            Destroy(weaponToRemove.gameObject);

            if (index < currentIndex)
            {
                currentIndex--;
            }
        }
    }

    void ReplaceWeapon(WeaponController weaponController)
    {
        if (currentIndex < 0 || currentIndex >= weaponControllers.Count) return;

        WeaponController oldWeapon = weaponControllers[currentIndex];
        weaponControllers[currentIndex] = weaponController;
        TransformWeapon(weaponController);

        StartCoroutine(SwitchWeapon(currentIndex, oldWeapon));
    }

    void TransformWeapon(WeaponController weaponController)
    {
        weaponController.gameObject.SetActive(false);

        Transform weaponTransform = weaponController.transform;

        SetChildrenLayer(weaponTransform, weaponLayer);
        weaponTransform.SetParent(rightHandSocket);
        weaponTransform.localPosition = Vector3.zero;
        weaponTransform.localRotation = Quaternion.identity;
        weaponTransform.localScale = Vector3.one;
    }

    public IEnumerator SwitchWeapon(int toIndex, WeaponController weaponToDestroy = null)
    {
        if (weaponToDestroy == null && toIndex == currentIndex)
        {
            yield break;
        }

        if (toIndex < 0 || toIndex >= weaponControllers.Count || weaponControllers[toIndex] == null)
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

            if (weaponToDestroy != null && currentWeaponController == weaponToDestroy)
            {
                Destroy(weaponToDestroy.gameObject);
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

    private IEnumerator HolsterAndDestroy(WeaponController weaponToDestroy)
    {
        IsSwitching = true;
        weaponToDestroy.IsLocked = true;
        GameEventSystem.OnPlayerWeaponHolster?.Invoke();

        bool holsterFinished = false;
        animator.CrossFadeWithCallback(this, holsterAnimationHash, 0.25f, rightLayer)
                .SetOnComplete(() => holsterFinished = true);

        yield return new WaitUntil(() => holsterFinished);

        Destroy(weaponToDestroy.gameObject);
        currentWeaponController = null;
        IsSwitching = false;
    }

    void SetChildrenLayer(Transform root, int layer)
    {
        var children = root.GetComponentsInChildren<Transform>(includeInactive: true);
        foreach (var child in children)
        {
            child.gameObject.layer = layer;
        }
    }
}
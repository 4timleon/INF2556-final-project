using UnityEngine;

public class Player : MonoBehaviour
{
    public float Health { get; private set; }

    [SerializeField] private WeaponBuilder weaponBuilder;
    [SerializeField] private PlayerArmory playerArmory;

    private float preDeathTimer = 60f;

    private bool isInPreDeath = false;
    private bool isDead = false;

    void Start()
    {
        Invoke("GiveFirstWeapon", 1f);
    }

    // testing
    void GiveFirstWeapon()
    {
        playerArmory.AddWeapon(weaponBuilder.BuildWeapon());
    }

    public void TakeDamage(float value)
    {
        if (isInPreDeath) return;

        Health -= value;
        Health = Mathf.Max(Health, 0);

        Debug.Log("Health: " + Health);

        if (Health <= 0)
        {
            isInPreDeath = true;
            PreDeath();
        }
    }

    private void PreDeath()
    {
        if (isDead) return;

        //If Timer runs out the Player dies
        if (Time.time <= preDeathTimer)
        {
            isDead = true;
            Die();
        }
    }

    private void Die()
    {
        GameEventSystem.OnPlayerDeath?.Invoke();
    }
}

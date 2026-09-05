using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CorpseCollector : MonoBehaviour
{
    [SerializeField] private float maxAmountToDissolve = 10f;

    private List<Zombie> currentZombiesKilled;

    private float currentAmountToDissolve = 0;
    private float dissolveDelay = 1f;
    private bool dissolveThresholdReached = false;

    private void OnEnable()
    {
        currentZombiesKilled = new();
        GameEventSystem.OnEnemyKilled += CollectZombie;
    }

    private void OnDisable()
    {
        GameEventSystem.OnEnemyKilled -= CollectZombie;
    }

    //Remove 10 Zombies per Frame
    private void Update()
    {
        if (currentZombiesKilled.Count == 0) return;

        if (dissolveThresholdReached)
        {
            dissolveDelay -= Time.deltaTime;
            if (dissolveDelay <= 0)
            {
                dissolveDelay = 1f;
                currentAmountToDissolve = 0;
                dissolveThresholdReached = false;
                return;
            }
        }
        else
        {
            for (int i = currentZombiesKilled.Count - 1; i > 0; i--)
            {
                Zombie zombie = currentZombiesKilled[i];
                currentZombiesKilled.RemoveAt(i);

                zombie.Dissolve();
                currentAmountToDissolve++;

                if (currentAmountToDissolve >= maxAmountToDissolve)
                {
                    dissolveThresholdReached = true;
                    return;
                }
            }
        }
    }

    private void CollectZombie(Zombie zombie)
    {
        currentZombiesKilled.Add(zombie);
    }
}

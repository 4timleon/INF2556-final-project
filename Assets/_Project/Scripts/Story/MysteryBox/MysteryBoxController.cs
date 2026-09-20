using System;
using System.Collections.Generic;
using UnityEngine;
using EditorAttributes;
using Random = UnityEngine.Random;

public class MysteryBoxController : MonoBehaviour
{
    [Title("Settings:", 14)]
    [SerializeField] private List<Transform> mysteryBoxSpawns;
    [SerializeField] private Transform startingSpawn;
    [Space]
    [SerializeField] private MysteryBox mysteryBox;

    void Start()
    {
        MoveBox(startingSpawn);
    }

    void OnEnable()
    {
        GameEventSystem.OnMysteryBoxDisappear += Event_MysteryBoxDisappear;
    }

    void OnDisable()
    {
        GameEventSystem.OnMysteryBoxDisappear -= Event_MysteryBoxDisappear;
    }

    void Event_MysteryBoxDisappear(Transform lastSpawn)
    {
        Transform nextSpawn = GetRandomTransformExcept(mysteryBoxSpawns, lastSpawn);
        MoveBox(nextSpawn);
    }

    void MoveBox(Transform nextSpawn)
    {
        mysteryBox.transform.SetParent(nextSpawn);
        mysteryBox.transform.localPosition = Vector3.zero;
        mysteryBox.transform.localRotation = Quaternion.identity;

        mysteryBox.SpawnPoint = nextSpawn;

        mysteryBox.SpawnPoint = nextSpawn;
    }

    Transform GetRandomTransformExcept(List<Transform> list, Transform exclude)
    {
        if (list == null || list.Count == 0)
            return null;

        if (list.Count == 1)
            return list[0];

        int excludeIndex = list.IndexOf(exclude);

        if (excludeIndex == -1)
        {
            return list[Random.Range(0, list.Count)];
        }

        int newIndex = Random.Range(0, list.Count - 1);

        if (newIndex >= excludeIndex)
        {
            newIndex++;
        }

        return list[newIndex];
    }
}

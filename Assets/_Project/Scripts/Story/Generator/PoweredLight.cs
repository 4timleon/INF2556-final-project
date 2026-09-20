using UnityEngine;

public class PoweredLight : MonoBehaviour
{
    [Tooltip("Die Lampen-Objekte - alle Lights darunter werden geschaltet")]
    [SerializeField] private GameObject[] lampObjects;

    private Light[] lights;

    private void OnEnable()
    {
        GameEventSystem.OnPowerChanged += SetLights;
    }

    private void OnDisable()
    {
        GameEventSystem.OnPowerChanged -= SetLights;
    }
    private void Start()
    {
        // alle Lights aus allen Lampen einsammeln
        var found = new System.Collections.Generic.List<Light>();
        foreach (GameObject lamp in lampObjects)
        {
            if (lamp != null)
                found.AddRange(lamp.GetComponentsInChildren<Light>());
        }
        lights = found.ToArray();

        SetLights(GameData.IsPowerOn);
    }

    private void SetLights(bool state)
    {
        foreach (Light l in lights)
        {
            if (l != null) l.enabled = state;
        }
    }
}
using System.Text;
using TMPro;
using UnityEngine;

public class MerchantCardStats : MonoBehaviour {

    public enum StatTypes {
        Damage,
        Accuracy,
        Handling,
        Reload_Time,
        Firerate
    }

    [SerializeField] private TextMeshProUGUI damageStat;
    [SerializeField] private TextMeshProUGUI stat2;
    [SerializeField] private TextMeshProUGUI stat3;
    [SerializeField] private TextMeshProUGUI stat4;
    [SerializeField] private TextMeshProUGUI stat5;

    private static readonly StringBuilder stringBuilder = new StringBuilder(16);

    public void SetStats(StatTypes statTypes, int value) {
        stringBuilder.Clear();
        stringBuilder.Append(value);

        switch (statTypes) {
            case StatTypes.Damage:
                damageStat.SetText(stringBuilder);
                break;
            case StatTypes.Accuracy:
                stat2.SetText(stringBuilder);
                break;
            case StatTypes.Handling:
                stat3.SetText(stringBuilder);
                break;
            case StatTypes.Reload_Time:
                stat4.SetText(stringBuilder);
                break;
            case StatTypes.Firerate:
                stat5.SetText(stringBuilder);
                break;
        }
    }
}

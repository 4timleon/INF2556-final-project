using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using WeaponNamespace;

public class MerchantCardStats : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI damage;
    [SerializeField] private TextMeshProUGUI accuracy;
    [SerializeField] private TextMeshProUGUI handling;
    [SerializeField] private TextMeshProUGUI reloadtime;
    [SerializeField] private TextMeshProUGUI firerate;
    [SerializeField] private TextMeshProUGUI magazine;

    float damageValue;
    float bulletAmount = 1;

    public void SetStat(StatType statType, float value)
    {
        if (statType == StatType.Damage)
            damageValue = value;
        else if (statType == StatType.BulletAmount)
            bulletAmount = value;
        else
        {
            var text = statType switch
            {
                StatType.Accuracy => accuracy,
                StatType.Handling => handling,
                StatType.ReloadTime => reloadtime,
                StatType.Firerate => firerate,
                StatType.MagazineSize => magazine,
                _ => null
            };

            ChangeText(text, value);
            return;
        }

        damage.text = $"{damageValue:F2}" + (bulletAmount > 1 ? $"x{bulletAmount:F0}" : "");
    }

    void ChangeText(TextMeshProUGUI text, float value)
    {
        text.text = value.ToString("F2");
    }
}

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

    [SerializeField] private Color goodColor = Color.green;
    [SerializeField] private Color badColor = Color.red;

    float damageValue;
    float bulletAmount = 1;

    public void SetStat(StatType statType, float currentValue, float value)
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
            CompareWeapons(text, currentValue, value);
            return;
        }

        damage.text = $"{damageValue:F2}" + (bulletAmount > 1 ? $"x{bulletAmount:F0}" : "");
    }

    void ChangeText(TextMeshProUGUI text, float value)
    {
        text.text = value.ToString("F2");
    }

    public void CompareWeapons(TextMeshProUGUI statText, float currentValue, float cardValue)
    {
        if (cardValue > currentValue) statText.color = goodColor;
        else statText.color = badColor;
    }
}

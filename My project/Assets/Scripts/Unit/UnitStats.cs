using System;
using System.Collections.Generic;
using UnityEngine;

public enum UnitUpgradeType
{
    Health,
    AttackPower,
    Defense,
    MoveRange,
    AttackRange,
    CriticalChance
}

public enum EquipmentSlot
{
    Weapon,
    Armor,
    Accessory
}

[Serializable]
public class UnitEquipment
{
    public string itemName;
    public EquipmentSlot slot;
    public int attackBonus;
    public int defenseBonus;
    public int moveRangeBonus;

    public string GetBonusDescription()
    {
        List<string> bonuses = new List<string>();
        if (attackBonus != 0) bonuses.Add($"공격력 {attackBonus:+#;-#;0}");
        if (defenseBonus != 0) bonuses.Add($"방어력 {defenseBonus:+#;-#;0}");
        if (moveRangeBonus != 0) bonuses.Add($"이동 거리 {moveRangeBonus:+#;-#;0}");
        return string.Join(", ", bonuses);
    }
}

[Serializable]
public class UnitProgressionRecord
{
    public string timestamp;
    public string description;
}

public class UnitStats : MonoBehaviour
{
    public int health = 100;
    public int attackPower = 10;
    public int defense = 5;
    public int moveRange = 3;
    public int attackRange = 1;
    public int supportRange = 0;
    public float criticalChance = 0.1f;
    public float criticalMultiplier = 2.0f;
    public int attackPerTurn = 1;
    public int ZOCRange = 0;

    [SerializeField] private List<UnitProgressionRecord> progressionHistory = new List<UnitProgressionRecord>();
    [SerializeField] private List<UnitEquipment> inventory = new List<UnitEquipment>();
    [SerializeField] private int equippedWeaponIndex = -1;
    [SerializeField] private int equippedArmorIndex = -1;
    [SerializeField] private int equippedAccessoryIndex = -1;

    public IReadOnlyList<UnitProgressionRecord> ProgressionHistory => progressionHistory;
    public IReadOnlyList<UnitEquipment> Inventory => inventory;
    public int EffectiveAttackPower => attackPower + GetEquipmentBonus(item => item.attackBonus);
    public int EffectiveDefense => defense + GetEquipmentBonus(item => item.defenseBonus);
    public int EffectiveMoveRange => Mathf.Max(0, moveRange + GetEquipmentBonus(item => item.moveRangeBonus));

    private void Awake()
    {
        if (progressionHistory == null)
            progressionHistory = new List<UnitProgressionRecord>();

        if (inventory == null)
            inventory = new List<UnitEquipment>();

        if (inventory.Count == 0)
        {
            inventory.Add(new UnitEquipment { itemName = "훈련용 검", slot = EquipmentSlot.Weapon, attackBonus = 3 });
            inventory.Add(new UnitEquipment { itemName = "정찰병 외투", slot = EquipmentSlot.Armor, defenseBonus = 2 });
            inventory.Add(new UnitEquipment { itemName = "기동 장화", slot = EquipmentSlot.Accessory, moveRangeBonus = 1 });
        }
    }

    public bool Upgrade(UnitUpgradeType upgradeType)
    {
        string description;
        switch (upgradeType)
        {
            case UnitUpgradeType.Health:
                health += 10;
                description = "체력 +10";
                break;
            case UnitUpgradeType.AttackPower:
                attackPower++;
                description = "공격력 +1";
                break;
            case UnitUpgradeType.Defense:
                defense++;
                description = "방어력 +1";
                break;
            case UnitUpgradeType.MoveRange:
                moveRange++;
                description = "이동 거리 +1";
                break;
            case UnitUpgradeType.AttackRange:
                attackRange++;
                description = "공격 거리 +1";
                break;
            case UnitUpgradeType.CriticalChance:
                if (criticalChance >= 1f)
                    return false;
                criticalChance = Mathf.Min(criticalChance + 0.05f, 1f);
                description = "치명타 확률 +5%";
                break;
            default:
                Debug.LogWarning($"Unsupported unit upgrade: {upgradeType}", this);
                return false;
        }

        AddHistory($"강화: {description}");
        return true;
    }

    public bool EquipItem(int inventoryIndex)
    {
        if (inventoryIndex < 0 || inventoryIndex >= inventory.Count)
        {
            Debug.LogWarning($"Invalid equipment inventory index: {inventoryIndex}", this);
            return false;
        }

        UnitEquipment item = inventory[inventoryIndex];
        int equippedIndex;
        switch (item.slot)
        {
            case EquipmentSlot.Weapon:
                equippedIndex = equippedWeaponIndex;
                equippedWeaponIndex = equippedIndex == inventoryIndex ? -1 : inventoryIndex;
                break;
            case EquipmentSlot.Armor:
                equippedIndex = equippedArmorIndex;
                equippedArmorIndex = equippedIndex == inventoryIndex ? -1 : inventoryIndex;
                break;
            case EquipmentSlot.Accessory:
                equippedIndex = equippedAccessoryIndex;
                equippedAccessoryIndex = equippedIndex == inventoryIndex ? -1 : inventoryIndex;
                break;
            default:
                Debug.LogWarning($"Unsupported equipment slot: {item.slot}", this);
                return false;
        }

        AddHistory(equippedIndex == inventoryIndex
            ? $"장비 해제: {item.itemName}"
            : $"장비 장착: {item.itemName}");
        return true;
    }

    public bool IsEquipped(int inventoryIndex)
    {
        return inventoryIndex == equippedWeaponIndex ||
               inventoryIndex == equippedArmorIndex ||
               inventoryIndex == equippedAccessoryIndex;
    }

    private int GetEquipmentBonus(Func<UnitEquipment, int> getBonus)
    {
        int bonus = 0;
        AddEquipmentBonus(equippedWeaponIndex, getBonus, ref bonus);
        AddEquipmentBonus(equippedArmorIndex, getBonus, ref bonus);
        AddEquipmentBonus(equippedAccessoryIndex, getBonus, ref bonus);
        return bonus;
    }

    private void AddEquipmentBonus(int inventoryIndex, Func<UnitEquipment, int> getBonus, ref int bonus)
    {
        if (inventoryIndex >= 0 && inventoryIndex < inventory.Count)
            bonus += getBonus(inventory[inventoryIndex]);
    }

    private void AddHistory(string description)
    {
        progressionHistory.Add(new UnitProgressionRecord
        {
            timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
            description = description
        });
    }

    public float EDPS
    {
        get
        {
            float averageDamage = EffectiveAttackPower * (1 + criticalChance * (criticalMultiplier - 1));
            return averageDamage * attackPerTurn;
        }
    }

    public float EHP
    {
        get
        {
            return health * (1 + EffectiveDefense / 100f);
        }
    }

    public float GetCombetPower()
    {
        return EDPS * EHP;
    }

    public void TakeDamage(int damage)
    {
        int actualDamage = Mathf.Max(damage - EffectiveDefense, 0);
        health -= actualDamage;
        Debug.Log($"{gameObject.name} took {actualDamage} damage. Remaining health: {health}");
        if (health <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        Debug.Log($"{gameObject.name} has died.");
        Destroy(gameObject);
    }
}

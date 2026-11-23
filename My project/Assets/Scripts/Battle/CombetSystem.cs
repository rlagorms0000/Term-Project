using UnityEngine;

public class CombetSystem : MonoBehaviour
{
    public static CombetSystem Instance { get; private set; }
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }

        Instance = this;
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    public bool ResolveCombat(UnitStats attacker, UnitStats defender)
    {
        if (attacker == null || defender == null || attacker.team == defender.team || defender.currentHealth <= 0)
        {
            Debug.LogWarning("Combat requires living units from opposing teams.", this);
            return false;
        }

        int previousHealth = defender.currentHealth;
        defender.TakeDamage(attacker.EffectiveAttackPower);
        int damage = previousHealth - defender.currentHealth;
        Debug.Log($"{attacker.name} attacked {defender.name} for {damage} damage. {defender.name} now has {defender.currentHealth} health.");
        return true;
    }
}
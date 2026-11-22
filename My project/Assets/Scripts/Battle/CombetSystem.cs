using UnityEngine;

public class CombetSystem : MonoBehaviour
{
    public static CombetSystem Instance { get; private set; }
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            Instance = this;
        }
    }
    public void ResolveCombat(UnitStats attacker, UnitStats defender)
    {
        int damage = Mathf.Max(0, attacker.EffectiveAttackPower - defender.EffectiveDefense);
        defender.health -= damage;
        Debug.Log($"{attacker.name} attacked {defender.name} for {damage} damage. {defender.name} now has {defender.health} health.");
    }
}
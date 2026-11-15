using UnityEngine;

public class UnitStats : MonoBehaviour
{
    public int health = 100;
    public int attackPower = 10;
    public int defense = 5;
    public int moveRange = 3;
    public void TakeDamage(int damage)
    {
        int actualDamage = Mathf.Max(damage - defense, 0);
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
        // Add death logic here (e.g., play animation, remove from game, etc.)
        Destroy(gameObject);
    }
}